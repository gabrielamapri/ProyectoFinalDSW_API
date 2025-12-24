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
        // use repository method that includes related entities so mapping fills names
        var Citas = await _unitOfWork.Citas.GetAllWithRelationsAsync();
        _logger.LogInformation("{Count} Citas retrieved.", Citas.Count());
        return _mapper.Map<IEnumerable<CitaDto>>(Citas);
    }

    public async Task<IEnumerable<CitaDto>> GetAllAsync(string? search = null)
    {
        _logger.LogInformation("Retrieving Citas with search: {Search}", search ?? "(none)");
        var Citas = await _unitOfWork.Citas.GetAllWithRelationsAsync();
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower().Trim();
            Citas = Citas.Where(c =>
                (c.Paciente != null && (c.Paciente.Nombres + " " + c.Paciente.Apellidos).ToLower().Contains(searchLower)) ||
                (c.Terapeuta != null && (c.Terapeuta.Nombres + " " + c.Terapeuta.Apellidos).ToLower().Contains(searchLower)) ||
                (c.TipoSesion != null && c.TipoSesion.Nombre != null && c.TipoSesion.Nombre.ToLower().Contains(searchLower)) ||
                (c.TipoSesion != null && c.TipoSesion.Especialidad != null && c.TipoSesion.Especialidad.Nombre != null && c.TipoSesion.Especialidad.Nombre.ToLower().Contains(searchLower)) ||
                (c.Estado != null && c.Estado.ToLower().Contains(searchLower)) ||
                c.Fecha.ToString("yyyy-MM-dd").Contains(searchLower) ||
                c.Fecha.ToString("dd/MM/yyyy").Contains(searchLower)
            ).ToList();
        }
        
        _logger.LogInformation("{Count} Citas retrieved after search.", Citas.Count());
        return _mapper.Map<IEnumerable<CitaDto>>(Citas);
    }

        public async Task<CitaDto> CreateAsync(CreateCitaDto dto)
    {
            _logger.LogInformation("Creating a new Cita for Paciente ID {PacienteID} on {Fecha}", dto.PacienteId, dto.Fecha);

            // Normalize incoming date: use UTC for DB operations and Local for franja/time checks
            var dtoUtc = dto.Fecha.Kind == DateTimeKind.Utc ? dto.Fecha : dto.Fecha.ToUniversalTime();
            var dtoLocal = dtoUtc.ToLocalTime();

            if (dtoLocal <= DateTime.Now)
            {
                _logger.LogWarning("Attempted to create Cita in the past for Paciente ID {PacienteID} on {Fecha}", dto.PacienteId, dto.Fecha);
                throw new BusinessRuleException(
                    "PastAppointment",
                    "Cannot schedule Citas in the past.");
            }

        // determine duration
        int duration = dto.DuracionMinutos ?? 0;
            if (duration <= 0 && dto.TipoSesionId.HasValue)
        {
                var tipo = await _unitOfWork.TiposSesion.GetByIdAsync(dto.TipoSesionId.Value);
            if (tipo != null) duration = tipo.DuracionMinutos;
        }
        if (duration <= 0) duration = 45; // default to 45 minutes for this center

        // Additional checks: patient same-day/specialty and min gaps
            var dayStartCheck = dtoUtc.Date;
            var dayEndCheck = dtoUtc.Date.AddDays(1).AddTicks(-1);
        var patientAppointments = (await _unitOfWork.Citas.GetByDateRangeAsync(dayStartCheck, dayEndCheck)).Where(a => a.PacienteId == dto.PacienteId).ToList();

        // If Terapeuta provided, verify franja (respecting FranjaExcepcion) and check overlaps with buffer
        if (dto.TerapeutaId.HasValue)
        {
            var franjas = await _unitOfWork.Franjas.GetByTerapeutaIdAsync(dto.TerapeutaId.Value);
                // use local time of the requested appointment to compare with franja TimeSpans
                var appointmentTime = dtoLocal.TimeOfDay;
                var appointmentEndTime = appointmentTime.Add(TimeSpan.FromMinutes(duration));

            var covers = false;
                foreach (var f in franjas)
                {
                    // compare weekday/date using local representation
                    var isApplicable = (f.Recurrente && f.DiaSemana.HasValue && f.DiaSemana.Value == (int)dtoLocal.DayOfWeek)
                        || (!f.Recurrente && f.Fecha.HasValue && f.Fecha.Value.Date == dtoLocal.Date);
                    if (!isApplicable) continue;
                    var hasException = await _unitOfWork.Excepciones.ExistsAsync(f.Id, dtoLocal.Date);
                    if (hasException) continue;
                    if (appointmentTime >= f.HoraInicio && appointmentEndTime <= f.HoraFin)
                    {
                        covers = true;
                        break;
                    }
                }

            if (!covers)
            {
                throw new BusinessRuleException("NoAvailability", "No existe una franja disponible del terapeuta en la fecha/hora solicitada.");
            }

            // check overlaps for therapist (no buffer; therapists may have back-to-back appointments)
                // use UTC window for DB queries (DB stores UTC timestamps)
                var windowStart = dtoUtc.AddMinutes(-duration);
                var windowEnd = dtoUtc.AddMinutes(duration);
                var potential = await _unitOfWork.Citas.GetByDateRangeAsync(windowStart, windowEnd);
                var newStart = dtoUtc;
                var newEnd = dtoUtc.AddMinutes(duration);
            var overlaps = potential.Where(a => a.TerapeutaId == dto.TerapeutaId && a.Estado != "Cancelled")
                .Any(a =>
                {
                    var existingStart = a.Fecha;
                    var existingStartUtc = existingStart.Kind == DateTimeKind.Utc ? existingStart : DateTime.SpecifyKind(existingStart, DateTimeKind.Local).ToUniversalTime();
                    var existingDuration = a.DuracionMinutos > 0 ? a.DuracionMinutos : duration;
                    var existingEndUtc = existingStartUtc.AddMinutes(existingDuration);
                    // therapists allowed back-to-back: use standard interval intersection on UTC times
                    return existingStartUtc < newEnd && existingEndUtc > newStart;
                });

            if (overlaps)
            {
                throw new BusinessRuleException("ConflictoHorario", "El terapeuta tiene otra cita muy cercana o solapada en ese horario.");
            }
        }

            // Patient-level rules: require at least 30 minutes gap between any of the patient's appointments.
        if (patientAppointments.Any())
        {
            // determine requested specialty (from Terapeuta if provided)
            int? requestedEspecialidadId = null;
            if (dto.TerapeutaId.HasValue)
            {
                var t = await _unitOfWork.Terapeutas.GetByIdAsync(dto.TerapeutaId.Value);
                requestedEspecialidadId = t?.EspecialidadId;
            }

            // check same-day same-specialty: patient cannot book two appointments
            // with the same speciality on the same date
            if (requestedEspecialidadId.HasValue)
            {
                foreach (var pA in patientAppointments.Where(a => a.TerapeutaId.HasValue))
                {
                    var existingTer = await _unitOfWork.Terapeutas.GetByIdAsync(pA.TerapeutaId!.Value);
                    if (existingTer != null && existingTer.EspecialidadId == requestedEspecialidadId.Value)
                    {
                        throw new BusinessRuleException("OnePerSpecialityPerDay", "Ya existe una cita para la misma especialidad en esa fecha.");
                    }
                }
            }

                // check 30-minute gap between patient's appointments (use UTC for DB-stored times)
                foreach (var pA in patientAppointments.Where(a => a.Estado != "Cancelled"))
                {
                    var existingStart = pA.Fecha;
                    var existingStartUtc = existingStart.Kind == DateTimeKind.Utc ? existingStart : DateTime.SpecifyKind(existingStart, DateTimeKind.Local).ToUniversalTime();
                    var existingDuration = pA.DuracionMinutos > 0 ? pA.DuracionMinutos : duration;
                    var existingEndUtc = existingStartUtc.AddMinutes(existingDuration);
                    var newStartUtc = dtoUtc;
                    var newEndUtc = dtoUtc.AddMinutes(duration);
                    var gapOk = existingEndUtc.AddMinutes(30) <= newStartUtc || newEndUtc.AddMinutes(30) <= existingStartUtc;
                    if (!gapOk)
                    {
                        throw new BusinessRuleException("MinGap", "Debe haber al menos 30 minutos entre el fin de una cita y el inicio de la siguiente.");
                    }
                }
        }

            var Cita = _mapper.Map<Cita>(dto);
            // Store the wall-clock local time (preserve the hour the user selected).
            // Use dtoLocal (computed above) and persist as Unspecified so the DB keeps the same numeric hour.
            Cita.Fecha = DateTime.SpecifyKind(dtoLocal, DateTimeKind.Unspecified);
            Cita.DuracionMinutos = duration;

        // Start transaction to make creation atomic and avoid race conditions
        await _unitOfWork.BeginTransactionAsync();
        try
        {
                // re-check overlaps right before saving (to avoid race conditions)
            if (dto.TerapeutaId.HasValue)
            {
                var windowStart = dtoUtc.AddMinutes(-duration);
                var windowEnd = dtoUtc.AddMinutes(duration);
                var potential = await _unitOfWork.Citas.GetByDateRangeAsync(windowStart, windowEnd);
                var newStart = dtoUtc;
                var newEnd = dtoUtc.AddMinutes(duration);
                var overlapsNow = potential.Where(a => a.TerapeutaId == dto.TerapeutaId && a.Estado != "Cancelled")
                    .Any(a =>
                    {
                        var existingStart = a.Fecha;
                        var existingStartUtc = existingStart.Kind == DateTimeKind.Utc ? existingStart : DateTime.SpecifyKind(existingStart, DateTimeKind.Local).ToUniversalTime();
                        var existingDuration = a.DuracionMinutos > 0 ? a.DuracionMinutos : duration;
                        var existingEndUtc = existingStartUtc.AddMinutes(existingDuration);
                        return existingStartUtc < newEnd && existingEndUtc > newStart;
                    });
                if (overlapsNow)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new BusinessRuleException("ConflictoHorario", "El terapeuta tiene otra cita muy cercana o solapada en ese horario.");
                }
            }

                // re-check patient gaps (use UTC for DB ranges)
                dayStartCheck = dtoUtc.Date;
                dayEndCheck = dtoUtc.Date.AddDays(1).AddTicks(-1);
                var patientAppointmentsNow = (await _unitOfWork.Citas.GetByDateRangeAsync(dayStartCheck, dayEndCheck)).Where(a => a.PacienteId == dto.PacienteId).ToList();
            if (patientAppointmentsNow.Any())
            {
                // enforce same-specialty-per-day: patient cannot book two appointments
                // with the same speciality on the same date
                int? requestedEspecialidadId = null;
                if (dto.TerapeutaId.HasValue)
                {
                    var t = await _unitOfWork.Terapeutas.GetByIdAsync(dto.TerapeutaId.Value);
                    requestedEspecialidadId = t?.EspecialidadId;
                }
                if (requestedEspecialidadId.HasValue)
                {
                    foreach (var pA in patientAppointmentsNow.Where(a => a.TerapeutaId.HasValue))
                    {
                        var existingTer = await _unitOfWork.Terapeutas.GetByIdAsync(pA.TerapeutaId!.Value);
                        if (existingTer != null && existingTer.EspecialidadId == requestedEspecialidadId.Value)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            throw new BusinessRuleException("OnePerSpecialityPerDay", "Ya existe una cita para la misma especialidad en esa fecha.");
                        }
                    }
                }
                    foreach (var pA in patientAppointmentsNow.Where(a => a.Estado != "Cancelled"))
                    {
                        var existingStart = pA.Fecha;
                        var existingStartUtc = existingStart.Kind == DateTimeKind.Utc ? existingStart : DateTime.SpecifyKind(existingStart, DateTimeKind.Local).ToUniversalTime();
                        var existingDuration = pA.DuracionMinutos > 0 ? pA.DuracionMinutos : duration;
                        var existingEndUtc = existingStartUtc.AddMinutes(existingDuration);
                        var newStartUtc = dtoUtc;
                        var newEndUtc = dtoUtc.AddMinutes(duration);
                        var gapOk = existingEndUtc.AddMinutes(30) <= newStartUtc || newEndUtc.AddMinutes(30) <= existingStartUtc;
                        if (!gapOk)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            throw new BusinessRuleException("MinGap", "Debe haber al menos 30 minutos entre el fin de una cita y el inicio de la siguiente.");
                        }
                    }
            }

            var createdAppointment = await _unitOfWork.Citas.CreateAsync(Cita);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var appointmentWithDetails = await _unitOfWork.Citas.GetWithPacienteAndFamiliaAsync(createdAppointment.Id);
            _logger.LogInformation("Cita for Paciente ID {PacienteID} created successfully with ID {Id}.", dto.PacienteId, createdAppointment.Id);
            return _mapper.Map<CitaDto>(appointmentWithDetails!);
        }
        catch
        {
            try { await _unitOfWork.RollbackTransactionAsync(); } catch {}
            throw;
        }
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
        if (duration <= 0 && dto.TipoSesionId.HasValue)
        {
            var tipo = await _unitOfWork.TiposSesion.GetByIdAsync(dto.TipoSesionId.Value);
            if (tipo != null) duration = tipo.DuracionMinutos;
        }
        if (duration <= 0) duration = Cita.DuracionMinutos > 0 ? Cita.DuracionMinutos : 45;

        // If therapist changed or date changed, validate availability and overlaps
        var newTerapeutaId = dto.TerapeutaId ?? Cita.TerapeutaId;
        var newFecha = dto.Fecha;
        // Normalize newFecha for comparisons: compute once so it's available later when persisting
        var newUtc = newFecha.Kind == DateTimeKind.Utc ? newFecha : newFecha.ToUniversalTime();
        var newLocal = newUtc.ToLocalTime();
        if (newTerapeutaId.HasValue)
        {

            var franjas = await _unitOfWork.Franjas.GetByTerapeutaIdAsync(newTerapeutaId.Value);
            var appointmentTime = newLocal.TimeOfDay;
            var appointmentEndTime = appointmentTime.Add(TimeSpan.FromMinutes(duration));

            var covers = franjas.Any(f =>
                ((f.Recurrente && f.DiaSemana.HasValue && f.DiaSemana.Value == (int)newLocal.DayOfWeek) || (!f.Recurrente && f.Fecha.HasValue && f.Fecha.Value.Date == newLocal.Date))
                && appointmentTime >= f.HoraInicio && appointmentEndTime <= f.HoraFin);

            if (!covers)
            {
                throw new BusinessRuleException("NoAvailability", "No existe una franja disponible del terapeuta en la fecha/hora solicitada.");
            }

            // check overlaps excluding this appointment — use UTC windows for DB queries
            var windowStart = newUtc.AddMinutes(-duration);
            var windowEnd = newUtc.AddMinutes(duration);
            var potential = await _unitOfWork.Citas.GetByDateRangeAsync(windowStart, windowEnd);
            var overlaps = potential.Where(a => a.TerapeutaId == newTerapeutaId && a.Id != Cita.Id && a.Estado != "Cancelled")
                .Any(a =>
                {
                    var existingStart = a.Fecha;
                    var existingDuration = a.DuracionMinutos > 0 ? a.DuracionMinutos : duration;
                    var existingEnd = existingStart.AddMinutes(existingDuration);
                    var newStart = newUtc;
                    var newEnd = newUtc.AddMinutes(duration);
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
        // Store the wall-clock local time for the appointment (preserve selected hour).
        // `newLocal` was computed earlier for availability checks; persist it as Unspecified.
        Cita.Fecha = DateTime.SpecifyKind(newLocal, DateTimeKind.Unspecified);
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

    public async Task<IEnumerable<CitaAlertaDto>> GetByTerapeutaAndDateRangeAsync(int terapeutaId, DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            throw new BusinessRuleException(
                "InvalidDateRange",
                "Start date must be before end date.");
        }

        var citas = await _unitOfWork.Citas.GetByTerapeutaAndDateRangeAsync(terapeutaId, startDate, endDate);
        return _mapper.Map<IEnumerable<CitaAlertaDto>>(citas);
    }

    public async Task<CitaDto> ReprogramAsync(int id, DTOs.Cita.ReprogramCitaDto dto)
    {
        var Cita = await _unitOfWork.Citas.GetWithPacienteAndFamiliaAsync(id);
        if (Cita == null)
        {
            throw new NotFoundException("Cita", id);
        }

        // enforce 12-hour restriction from current scheduled time
        var hoursUntil = (Cita.Fecha - DateTime.Now).TotalHours;
        if (hoursUntil < 12)
        {
            throw new BusinessRuleException(
                "RescheduleNotAllowed",
                "No se puede anular o reprogramar citas con menos de 12 horas de anticipacion, Comunicarse via telefonica.");
        }

        // normalize incoming date
        var newUtc = dto.Fecha.Kind == DateTimeKind.Utc ? dto.Fecha : dto.Fecha.ToUniversalTime();
        var newLocal = newUtc.ToLocalTime();

        if (newLocal <= DateTime.Now)
        {
            throw new BusinessRuleException(
                "PastAppointment",
                "Cannot reschedule to a past date.");
        }

        // determine duration
        int duration = dto.DuracionMinutos ?? Cita.DuracionMinutos;
        if (duration <= 0 && dto.DuracionMinutos == null && Cita.TipoSesionId.HasValue)
        {
            var tipo = await _unitOfWork.TiposSesion.GetByIdAsync(Cita.TipoSesionId.Value);
            if (tipo != null) duration = tipo.DuracionMinutos;
        }
        if (duration <= 0) duration = Cita.DuracionMinutos > 0 ? Cita.DuracionMinutos : 45;

        var terapeutaId = Cita.TerapeutaId;

        // If therapist assigned, verify franja and overlaps
        if (terapeutaId.HasValue)
        {
            var franjas = await _unitOfWork.Franjas.GetByTerapeutaIdAsync(terapeutaId.Value);
            var appointmentTime = newLocal.TimeOfDay;
            var appointmentEndTime = appointmentTime.Add(TimeSpan.FromMinutes(duration));

            var covers = false;
            foreach (var f in franjas)
            {
                var isApplicable = (f.Recurrente && f.DiaSemana.HasValue && f.DiaSemana.Value == (int)newLocal.DayOfWeek)
                    || (!f.Recurrente && f.Fecha.HasValue && f.Fecha.Value.Date == newLocal.Date);
                if (!isApplicable) continue;
                var hasException = await _unitOfWork.Excepciones.ExistsAsync(f.Id, newLocal.Date);
                if (hasException) continue;
                if (appointmentTime >= f.HoraInicio && appointmentEndTime <= f.HoraFin)
                {
                    covers = true;
                    break;
                }
            }

            if (!covers)
            {
                throw new BusinessRuleException("NoAvailability", "No existe una franja disponible del terapeuta en la fecha/hora solicitada.");
            }

            // check overlaps excluding this appointment — therapists may be back-to-back
            var windowStart = newUtc.AddMinutes(-duration);
            var windowEnd = newUtc.AddMinutes(duration);
            var potential = await _unitOfWork.Citas.GetByDateRangeAsync(windowStart, windowEnd);
            var overlaps = potential.Where(a => a.TerapeutaId == terapeutaId && a.Id != Cita.Id && a.Estado != "Cancelled")
                .Any(a =>
                {
                    var existingStart = a.Fecha;
                    var existingStartUtc = existingStart.Kind == DateTimeKind.Utc ? existingStart : DateTime.SpecifyKind(existingStart, DateTimeKind.Local).ToUniversalTime();
                    var existingDuration = a.DuracionMinutos > 0 ? a.DuracionMinutos : duration;
                    var existingEndUtc = existingStartUtc.AddMinutes(existingDuration);
                    var newStart = newUtc;
                    var newEnd = newUtc.AddMinutes(duration);
                    return existingStartUtc < newEnd && existingEndUtc > newStart;
                });

            if (overlaps)
            {
                throw new BusinessRuleException("ConflictoHorario", "El terapeuta tiene otra cita muy cercana o solapada en ese horario.");
            }
        }

        // Patient-level rules: same-speciality per day and 30-minute gap
        var dayStartCheck = newUtc.Date;
        var dayEndCheck = newUtc.Date.AddDays(1).AddTicks(-1);
        var patientAppointments = (await _unitOfWork.Citas.GetByDateRangeAsync(dayStartCheck, dayEndCheck)).Where(a => a.PacienteId == Cita.PacienteId && a.Id != Cita.Id).ToList();

        if (patientAppointments.Any())
        {
            int? requestedEspecialidadId = null;
            if (terapeutaId.HasValue)
            {
                var t = await _unitOfWork.Terapeutas.GetByIdAsync(terapeutaId.Value);
                requestedEspecialidadId = t?.EspecialidadId;
            }

            if (requestedEspecialidadId.HasValue)
            {
                foreach (var pA in patientAppointments.Where(a => a.TerapeutaId.HasValue))
                {
                    var existingTer = await _unitOfWork.Terapeutas.GetByIdAsync(pA.TerapeutaId!.Value);
                    if (existingTer != null && existingTer.EspecialidadId == requestedEspecialidadId.Value)
                    {
                        throw new BusinessRuleException("OnePerSpecialityPerDay", "Ya existe una cita para la misma especialidad en esa fecha.");
                    }
                }
            }

            foreach (var pA in patientAppointments.Where(a => a.Estado != "Cancelled"))
            {
                var existingStart = pA.Fecha;
                var existingStartUtc = existingStart.Kind == DateTimeKind.Utc ? existingStart : DateTime.SpecifyKind(existingStart, DateTimeKind.Local).ToUniversalTime();
                var existingDuration = pA.DuracionMinutos > 0 ? pA.DuracionMinutos : duration;
                var existingEndUtc = existingStartUtc.AddMinutes(existingDuration);
                var newStartUtc = newUtc;
                var newEndUtc = newUtc.AddMinutes(duration);
                var gapOk = existingEndUtc.AddMinutes(30) <= newStartUtc || newEndUtc.AddMinutes(30) <= existingStartUtc;
                if (!gapOk)
                {
                    throw new BusinessRuleException("MinGap", "Debe haber al menos 30 minutos entre el fin de una cita y el inicio de la siguiente.");
                }
            }
        }

        // persist changes inside a transaction and re-check to avoid races
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // re-check therapist overlaps
            if (terapeutaId.HasValue)
            {
                var windowStart = newUtc.AddMinutes(-duration);
                var windowEnd = newUtc.AddMinutes(duration);
                var potentialNow = await _unitOfWork.Citas.GetByDateRangeAsync(windowStart, windowEnd);
                var overlapsNow = potentialNow.Where(a => a.TerapeutaId == terapeutaId && a.Id != Cita.Id && a.Estado != "Cancelled")
                    .Any(a =>
                    {
                        var existingStart = a.Fecha;
                        var existingStartUtc = existingStart.Kind == DateTimeKind.Utc ? existingStart : DateTime.SpecifyKind(existingStart, DateTimeKind.Local).ToUniversalTime();
                        var existingDuration = a.DuracionMinutos > 0 ? a.DuracionMinutos : duration;
                        var existingEndUtc = existingStartUtc.AddMinutes(existingDuration);
                        var newStart = newUtc;
                        var newEnd = newUtc.AddMinutes(duration);
                        return existingStartUtc < newEnd && existingEndUtc > newStart;
                    });
                if (overlapsNow)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new BusinessRuleException("ConflictoHorario", "El terapeuta tiene otra cita muy cercana o solapada en ese horario.");
                }
            }

            // re-check patient gaps for the day
            dayStartCheck = newUtc.Date;
            dayEndCheck = newUtc.Date.AddDays(1).AddTicks(-1);
            var patientAppointmentsNow = (await _unitOfWork.Citas.GetByDateRangeAsync(dayStartCheck, dayEndCheck)).Where(a => a.PacienteId == Cita.PacienteId && a.Id != Cita.Id).ToList();
            if (patientAppointmentsNow.Any())
            {
                int? requestedEspecialidadId = null;
                if (terapeutaId.HasValue)
                {
                    var t = await _unitOfWork.Terapeutas.GetByIdAsync(terapeutaId.Value);
                    requestedEspecialidadId = t?.EspecialidadId;
                }
                if (requestedEspecialidadId.HasValue)
                {
                    foreach (var pA in patientAppointmentsNow.Where(a => a.TerapeutaId.HasValue))
                    {
                        var existingTer = await _unitOfWork.Terapeutas.GetByIdAsync(pA.TerapeutaId!.Value);
                        if (existingTer != null && existingTer.EspecialidadId == requestedEspecialidadId.Value)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            throw new BusinessRuleException("OnePerSpecialityPerDay", "Ya existe una cita para la misma especialidad en esa fecha.");
                        }
                    }
                }
                foreach (var pA in patientAppointmentsNow.Where(a => a.Estado != "Cancelled"))
                {
                    var existingStart = pA.Fecha;
                    var existingStartUtc = existingStart.Kind == DateTimeKind.Utc ? existingStart : DateTime.SpecifyKind(existingStart, DateTimeKind.Local).ToUniversalTime();
                    var existingDuration = pA.DuracionMinutos > 0 ? pA.DuracionMinutos : duration;
                    var existingEndUtc = existingStartUtc.AddMinutes(existingDuration);
                    var newStartUtc = newUtc;
                    var newEndUtc = newUtc.AddMinutes(duration);
                    var gapOk = existingEndUtc.AddMinutes(30) <= newStartUtc || newEndUtc.AddMinutes(30) <= existingStartUtc;
                    if (!gapOk)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        throw new BusinessRuleException("MinGap", "Debe haber al menos 30 minutos entre el fin de una cita y el inicio de la siguiente.");
                    }
                }
            }

            // apply changes
            Cita.Fecha = DateTime.SpecifyKind(newLocal, DateTimeKind.Unspecified);
            Cita.DuracionMinutos = duration;

            var updated = await _unitOfWork.Citas.UpdateAsync(Cita);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var appointmentWithDetails = await _unitOfWork.Citas.GetWithPacienteAndFamiliaAsync(updated.Id);
            return _mapper.Map<CitaDto>(appointmentWithDetails!);
        }
        catch
        {
            try { await _unitOfWork.RollbackTransactionAsync(); } catch { }
            throw;
        }
    }
}

