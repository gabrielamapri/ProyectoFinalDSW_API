using AutoMapper;
using Microsoft.Extensions.Logging;
using CentroTerapia.Application.DTOs.Cita;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Exceptions;
using CentroTerapia.Domain.Ports.Out;

namespace CentroTerapia.Application.Services;

    public class CitaService : ICitaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CitaService> _logger;

    public CitaService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CitaService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CitaDto> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving Cita with ID {Id}.", id);

        var Cita = await _unitOfWork.Citas.GetWithPacienteAndFamiliaAsync(id);
        if (Cita == null)
        {
            _logger.LogWarning("Cita with ID {AppointmentId} not found.", id);
            throw new NotFoundException("Cita", id);
        }
        _logger.LogInformation("Cita on {Fecha} found.", Cita.Fecha);
        return _mapper.Map<CitaDto>(Cita);
    }

    public async Task<IEnumerable<CitaDto>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all Citas.");
        var Citas = await _unitOfWork.Citas.GetAllAsync();
        _logger.LogInformation("{Count} Citas retrieved.", Citas.Count());
        return _mapper.Map<IEnumerable<CitaDto>>(Citas);
    }

    public async Task<CitaDto> CreateAsync(CreateCitaDto dto)
    {
        _logger.LogInformation("Creating a new Cita for Paciente ID {PacienteID} on {Fecha}", dto.PacienteId, dto.Fecha);

        if (dto.Fecha <= DateTime.Now)
        {
            _logger.LogWarning("Attempted to create Cita in the past for Paciente ID {PacienteID} on {Fecha}", dto.PacienteId, dto.Fecha);
            throw new BusinessRuleException(
                "PastAppointment",
                "Cannot schedule Citas in the past.");
        }

        // determine duration
        int duration = dto.DuracionMinutos ?? 0;
        if (duration <= 0 && dto.TerapiaId.HasValue)
        {
            var tipo = await _unitOfWork.TiposSesion.GetByIdAsync(dto.TerapiaId.Value);
            if (tipo != null) duration = tipo.DuracionMinutos;
        }
        if (duration <= 0) duration = 60; // default

        // if Terapeuta provided, verify franja and check overlaps
        if (dto.TerapeutaId.HasValue)
        {
            var franjas = await _unitOfWork.Franjas.GetByTerapeutaIdAsync(dto.TerapeutaId.Value);
            var appointmentTime = dto.Fecha.TimeOfDay;
            var appointmentEndTime = appointmentTime.Add(TimeSpan.FromMinutes(duration));

            var covers = franjas.Any(f =>
                (f.Recurrente && f.DiaSemana.HasValue && f.DiaSemana.Value == (int)dto.Fecha.DayOfWeek || (!f.Recurrente && f.Fecha.HasValue && f.Fecha.Value.Date == dto.Fecha.Date))
                && appointmentTime >= f.HoraInicio && appointmentEndTime <= f.HoraFin);

            if (!covers)
            {
                throw new BusinessRuleException("NoAvailability", "No existe una franja disponible del terapeuta en la fecha/hora solicitada.");
            }

            // check overlaps
            var windowStart = dto.Fecha.AddMinutes(-duration);
            var windowEnd = dto.Fecha.AddMinutes(duration);
            var potential = await _unitOfWork.Citas.GetByDateRangeAsync(windowStart, windowEnd);
            var overlaps = potential.Where(a => a.TerapeutaId == dto.TerapeutaId)
                .Any(a =>
                {
                    var existingStart = a.Fecha;
                    var existingDuration = a.DuracionMinutos > 0 ? a.DuracionMinutos : duration;
                    var existingEnd = existingStart.AddMinutes(existingDuration);
                    var newStart = dto.Fecha;
                    var newEnd = dto.Fecha.AddMinutes(duration);
                    return existingStart < newEnd && existingEnd > newStart;
                });

            if (overlaps)
            {
                throw new BusinessRuleException("ConflictoHorario", "El terapeuta tiene otra cita en ese horario.");
            }
        }

        var Cita = _mapper.Map<Cita>(dto);
        Cita.DuracionMinutos = duration;

        var createdAppointment = await _unitOfWork.Citas.CreateAsync(Cita);
        await _unitOfWork.SaveChangesAsync();
        var appointmentWithDetails = await _unitOfWork.Citas.GetWithPacienteAndFamiliaAsync(createdAppointment.Id);
        _logger.LogInformation("Cita for Paciente ID {PacienteID} created successfully with ID {Id}.", dto.PacienteId, createdAppointment.Id);
        return _mapper.Map<CitaDto>(appointmentWithDetails!);
    }

    public async Task<CitaDto> UpdateAsync(int id, UpdateCitaDto dto)
    {
        var Cita = await _unitOfWork.Citas.GetWithPacienteAndFamiliaAsync(id);
        if (Cita == null)
        {
            throw new NotFoundException("Cita", id);
        }

        // If rescheduling (changing Fecha) enforce 12-hour restriction
        if (dto.Fecha != Cita.Fecha)
        {
            var hoursUntil = (Cita.Fecha - DateTime.Now).TotalHours;
            if (hoursUntil < 12)
            {
                throw new BusinessRuleException(
                    "RescheduleNotAllowed",
                    "No se puede anular o reprogramar citas con menos de 12 horas de anticipacion, Comunicarse via telefonica.");
            }

            if (dto.Fecha <= DateTime.Now)
            {
                throw new BusinessRuleException(
                    "PastAppointment",
                    "Cannot reschedule to a past date.");
            }
        }

        // Determine new duration if provided
        int duration = dto.DuracionMinutos ?? Cita.DuracionMinutos;
        if (duration <= 0 && dto.TerapiaId.HasValue)
        {
            var tipo = await _unitOfWork.TiposSesion.GetByIdAsync(dto.TerapiaId.Value);
            if (tipo != null) duration = tipo.DuracionMinutos;
        }
        if (duration <= 0) duration = Cita.DuracionMinutos > 0 ? Cita.DuracionMinutos : 60;

        // If therapist changed or date changed, validate availability and overlaps
        var newTerapeutaId = dto.TerapeutaId ?? Cita.TerapeutaId;
        var newFecha = dto.Fecha;
        if (newTerapeutaId.HasValue)
        {
            var franjas = await _unitOfWork.Franjas.GetByTerapeutaIdAsync(newTerapeutaId.Value);
            var appointmentTime = newFecha.TimeOfDay;
            var appointmentEndTime = appointmentTime.Add(TimeSpan.FromMinutes(duration));

            var covers = franjas.Any(f =>
                (f.Recurrente && f.DiaSemana.HasValue && f.DiaSemana.Value == (int)newFecha.DayOfWeek || (!f.Recurrente && f.Fecha.HasValue && f.Fecha.Value.Date == newFecha.Date))
                && appointmentTime >= f.HoraInicio && appointmentEndTime <= f.HoraFin);

            if (!covers)
            {
                throw new BusinessRuleException("NoAvailability", "No existe una franja disponible del terapeuta en la fecha/hora solicitada.");
            }

            // check overlaps excluding this appointment
            var windowStart = newFecha.AddMinutes(-duration);
            var windowEnd = newFecha.AddMinutes(duration);
            var potential = await _unitOfWork.Citas.GetByDateRangeAsync(windowStart, windowEnd);
            var overlaps = potential.Where(a => a.TerapeutaId == newTerapeutaId && a.Id != Cita.Id)
                .Any(a =>
                {
                    var existingStart = a.Fecha;
                    var existingDuration = a.DuracionMinutos > 0 ? a.DuracionMinutos : duration;
                    var existingEnd = existingStart.AddMinutes(existingDuration);
                    var newStart = newFecha;
                    var newEnd = newFecha.AddMinutes(duration);
                    return existingStart < newEnd && existingEnd > newStart;
                });

            if (overlaps)
            {
                throw new BusinessRuleException("ConflictoHorario", "El terapeuta tiene otra cita en ese horario.");
            }
        }

        var validStatuses = new[] { "Scheduled", "Completed", "Cancelled" };
        if (!validStatuses.Contains(dto.Estado))
        {
            throw new BusinessRuleException(
                "InvalidStatus",
                $"Status must be one of: {string.Join(", ", validStatuses)}");
        }

        _mapper.Map(dto, Cita);
        Cita.DuracionMinutos = duration;

        var updatedAppointment = await _unitOfWork.Citas.UpdateAsync(Cita);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CitaDto>(updatedAppointment);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Attempting to delete Cita with ID {AppointmentID}.", id);
        var Cita = await _unitOfWork.Citas.GetByIdAsync(id);
        if (Cita == null)
        {
            _logger.LogWarning("Cita with ID {AppointmentID} not found. Cannot delete.", id);
            throw new NotFoundException("Cita", id);
        }

        var result = await _unitOfWork.Citas.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Cita with ID {AppointmentID} deleted successfully.", id);
        return result;
    }

    public async Task<bool> CancelAsync(int id)
    {
        var Cita = await _unitOfWork.Citas.GetByIdAsync(id);
        if (Cita == null)
        {
            throw new NotFoundException("Cita", id);
        }

        // enforce 12-hour restriction
        var hoursUntil = (Cita.Fecha - DateTime.Now).TotalHours;
        if (hoursUntil < 12)
        {
            throw new BusinessRuleException(
                "CannotCancelOrReschedule",
                "No se puede anular o reprogramar citas con menos de 12 horas de anticipacion, Comunicarse via telefonica.");
        }

        if (!Cita.CanBeCancelada())
        {
            throw new BusinessRuleException(
                "CannotCancel",
                "Only future scheduled Citas can be cancelled.");
        }

        Cita.Estado = "Cancelled";

        await _unitOfWork.Citas.UpdateAsync(Cita);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<CitaDto>> GetByPacienteIdAsync(int pacienteId)
    {
        var Citas = await _unitOfWork.Citas.GetByPacienteIdAsync(pacienteId);
        return _mapper.Map<IEnumerable<CitaDto>>(Citas);
    }

    public async Task<IEnumerable<CitaDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            throw new BusinessRuleException(
                "InvalidDateRange",
                "Start date must be before end date.");
        }

        var Citas = await _unitOfWork.Citas.GetByDateRangeAsync(startDate, endDate);
        return _mapper.Map<IEnumerable<CitaDto>>(Citas);
    }

    public async Task<IEnumerable<CitaDto>> GetByStatusAsync(string status)
    {
        var validStatuses = new[] { "Scheduled", "Completed", "Cancelled" };
        if (!validStatuses.Contains(status))
        {
            throw new BusinessRuleException(
                "InvalidStatus",
                $"Status must be one of: {string.Join(", ", validStatuses)}");
        }

        var Citas = await _unitOfWork.Citas.GetByStatusAsync(status);
        return _mapper.Map<IEnumerable<CitaDto>>(Citas);
    }
}

