using AutoMapper;
using Microsoft.Extensions.Logging;
using CentroTerapia.Application.DTOs.Cita;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Exceptions;
using CentroTerapia.Domain.Ports.Out;

namespace CentroTerapia.Application.Services
{
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

        #region Read Operations

        public async Task<CitaDto> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving Cita with ID {Id}.", id);
            var cita = await _unitOfWork.Citas.GetWithPacienteAndFamiliaAsync(id);
            if (cita == null)
            {
                _logger.LogWarning("Cita with ID {Id} not found.", id);
                throw new NotFoundException("Cita", id);
            }
            return _mapper.Map<CitaDto>(cita);
        }

        public async Task<IEnumerable<CitaDto>> GetAllAsync(string? search = null)
        {
            _logger.LogInformation("Retrieving Citas. Search: {Search}", search ?? "(none)");
            var citas = await _unitOfWork.Citas.GetAllWithRelationsAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                citas = FilterCitasBySearch(citas, search);
            }

            return _mapper.Map<IEnumerable<CitaDto>>(citas);
        }

        public async Task<IEnumerable<CitaDto>> GetByPacienteIdsAsync(IEnumerable<int> pacienteIds, string? search = null)
        {
            _logger.LogInformation("Retrieving Citas for PacienteIds. Search: {Search}", search ?? "(none)");
            var citas = await _unitOfWork.Citas.GetByPacienteIdsAsync(pacienteIds);

            if (!string.IsNullOrWhiteSpace(search))
            {
                citas = FilterCitasBySearch(citas, search);
            }

            return _mapper.Map<IEnumerable<CitaDto>>(citas);
        }

        public async Task<IEnumerable<CitaDto>> GetByPacienteIdAsync(int pacienteId)
        {
            var citas = await _unitOfWork.Citas.GetByPacienteIdAsync(pacienteId);
            return _mapper.Map<IEnumerable<CitaDto>>(citas);
        }

        public async Task<IEnumerable<CitaDto>> GetByTerapeutaIdAsync(int terapeutaId)
        {
            var citas = await _unitOfWork.Citas.GetAllWithRelationsAsync();
            var filtradas = citas.Where(c => c.TerapeutaId == terapeutaId).ToList();
            return _mapper.Map<IEnumerable<CitaDto>>(filtradas);
        }

        public async Task<IEnumerable<CitaDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var citas = await _unitOfWork.Citas.GetByDateRangeAsync(startDate, endDate);
            return _mapper.Map<IEnumerable<CitaDto>>(citas);
        }

        public async Task<IEnumerable<CitaDto>> GetByStatusAsync(string status)
        {
            var citas = await _unitOfWork.Citas.GetByStatusAsync(status);
            return _mapper.Map<IEnumerable<CitaDto>>(citas);
        }

        public async Task<IEnumerable<CitaAlertaDto>> GetByTerapeutaAndDateRangeAsync(int terapeutaId, DateTime startDate, DateTime endDate)
        {
            var citas = await _unitOfWork.Citas.GetByTerapeutaAndDateRangeAsync(terapeutaId, startDate, endDate);
            return _mapper.Map<IEnumerable<CitaAlertaDto>>(citas);
        }

        #endregion

        #region Create & Update

        public async Task<CitaDto> CreateAsync(CreateCitaDto dto)
        {
            _logger.LogInformation("Creating a new Cita for Paciente ID {PacienteID}", dto.PacienteId);

            var paciente = await _unitOfWork.Pacientes.GetByIdAsync(dto.PacienteId);
            if (paciente == null) throw new NotFoundException("Paciente", dto.PacienteId);

            var dtoUtc = dto.Fecha.Kind == DateTimeKind.Utc ? dto.Fecha : dto.Fecha.ToUniversalTime();
            var dtoLocal = dtoUtc.ToLocalTime();

            if (dtoLocal <= DateTime.Now)
            {
                throw new BusinessRuleException("PastAppointment", "No se puede programar citas en horas pasadas.");
            }

            int duration = await GetDuration(dto.DuracionMinutos, dto.TipoSesionId);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await ValidateTherapistAvailability(dto.TerapeutaId, dtoLocal, dtoUtc, duration);
                await ValidatePatientRules(dto.PacienteId, dto.TerapeutaId, dtoUtc, duration);

                var cita = _mapper.Map<Cita>(dto);
                cita.Fecha = DateTime.SpecifyKind(dtoLocal, DateTimeKind.Unspecified);
                cita.DuracionMinutos = duration;
                cita.Estado = "Scheduled";

                var created = await _unitOfWork.Citas.CreateAsync(cita);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var result = await _unitOfWork.Citas.GetWithPacienteAndFamiliaAsync(created.Id);
                return _mapper.Map<CitaDto>(result!);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<CitaDto> UpdateAsync(int id, UpdateCitaDto dto)
        {
            var cita = await _unitOfWork.Citas.GetByIdAsync(id);
            if (cita == null) throw new NotFoundException("Cita", id);

            if (dto.Fecha != default) cita.Fecha = dto.Fecha;
            if (!string.IsNullOrWhiteSpace(dto.Motivo)) cita.Motivo = dto.Motivo;
            if (!string.IsNullOrWhiteSpace(dto.Estado)) cita.Estado = dto.Estado;
            if (!string.IsNullOrWhiteSpace(dto.Notas)) cita.Notas = dto.Notas;
            if (dto.TerapeutaId.HasValue) cita.TerapeutaId = dto.TerapeutaId;
            if (dto.TipoSesionId.HasValue) cita.TipoSesionId = dto.TipoSesionId;
            if (dto.DuracionMinutos.HasValue) cita.DuracionMinutos = dto.DuracionMinutos.Value;

            await _unitOfWork.Citas.UpdateAsync(cita);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CitaDto>(cita);
        }

        public async Task<CitaDto> ReprogramAsync(int id, ReprogramCitaDto dto)
        {
            var cita = await _unitOfWork.Citas.GetByIdAsync(id);
            if (cita == null) throw new NotFoundException("Cita", id);

            if ((cita.Fecha - DateTime.Now).TotalHours < 12)
            {
                throw new BusinessRuleException("RescheduleNotAllowed", "Menos de 12 horas para la cita.");
            }

            var newUtc = dto.Fecha.Kind == DateTimeKind.Utc ? dto.Fecha : dto.Fecha.ToUniversalTime();
            var newLocal = newUtc.ToLocalTime();
            int duration = dto.DuracionMinutos ?? cita.DuracionMinutos;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await ValidateTherapistAvailability(cita.TerapeutaId, newLocal, newUtc, duration, id);
                await ValidatePatientRules(cita.PacienteId, cita.TerapeutaId, newUtc, duration, id);

                cita.Fecha = DateTime.SpecifyKind(newLocal, DateTimeKind.Unspecified);
                cita.DuracionMinutos = duration;

                await _unitOfWork.Citas.UpdateAsync(cita);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return _mapper.Map<CitaDto>(cita);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> CancelAsync(int id)
        {
            var cita = await _unitOfWork.Citas.GetByIdAsync(id);
            if (cita == null) throw new NotFoundException("Cita", id);

            if ((cita.Fecha - DateTime.Now).TotalHours < 12)
                throw new BusinessRuleException("CannotCancel", "Anulaciones requieren 12 horas de anticipación.");

            cita.Estado = "Cancelled";
            await _unitOfWork.Citas.UpdateAsync(cita);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cita = await _unitOfWork.Citas.GetByIdAsync(id);
            if (cita == null) throw new NotFoundException("Cita", id);
            var result = await _unitOfWork.Citas.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        #endregion

        #region Private Helpers

        private IEnumerable<Cita> FilterCitasBySearch(IEnumerable<Cita> citas, string search)
        {
            var s = search.ToLower().Trim();
            return citas.Where(c =>
                (c.Paciente != null && (c.Paciente.Nombres + " " + c.Paciente.Apellidos).ToLower().Contains(s)) ||
                (c.Terapeuta != null && (c.Terapeuta.Nombres + " " + c.Terapeuta.Apellidos).ToLower().Contains(s)) ||
                (c.TipoSesion != null && c.TipoSesion.Nombre != null && c.TipoSesion.Nombre.ToLower().Contains(s)) ||
                (c.Estado != null && c.Estado.ToLower().Contains(s)) ||
                (!string.IsNullOrEmpty(c.Motivo) && c.Motivo.ToLower().Contains(s))
            ).ToList();
        }

        private async Task<int> GetDuration(int? dtoDuration, int? tipoSesionId)
        {
            if (dtoDuration.HasValue && dtoDuration > 0) return dtoDuration.Value;
            if (tipoSesionId.HasValue)
            {
                var tipo = await _unitOfWork.TiposSesion.GetByIdAsync(tipoSesionId.Value);
                if (tipo != null) return tipo.DuracionMinutos;
            }
            return 45;
        }

        private async Task ValidateTherapistAvailability(int? terapeutaId, DateTime localDate, DateTime utcDate, int duration, int? excludeId = null)
        {
            if (!terapeutaId.HasValue) return;
            var franjas = await _unitOfWork.Franjas.GetByTerapeutaIdAsync(terapeutaId.Value);
            var timeStart = localDate.TimeOfDay;
            var timeEnd = timeStart.Add(TimeSpan.FromMinutes(duration));

            bool isInsideFranja = false;
            foreach (var f in franjas)
            {
                bool applicable = (f.Recurrente && f.DiaSemana == (int)localDate.DayOfWeek) ||
                                  (!f.Recurrente && f.Fecha?.Date == localDate.Date);
                if (applicable && timeStart >= f.HoraInicio && timeEnd <= f.HoraFin)
                {
                    if (!await _unitOfWork.Excepciones.ExistsAsync(f.Id, localDate.Date))
                    {
                        isInsideFranja = true;
                        break;
                    }
                }
            }
            if (!isInsideFranja) throw new BusinessRuleException("NoAvailability", "Fuera del horario del terapeuta.");

            var potential = await _unitOfWork.Citas.GetByDateRangeAsync(utcDate.AddMinutes(-duration), utcDate.AddMinutes(duration));
            var overlap = potential.Any(a => a.TerapeutaId == terapeutaId && a.Id != excludeId && a.Estado != "Cancelled" &&
                utcDate < a.Fecha.AddMinutes(a.DuracionMinutos) && utcDate.AddMinutes(duration) > a.Fecha);
            if (overlap) throw new BusinessRuleException("ConflictoHorario", "El terapeuta ya tiene una cita.");
        }

        private async Task ValidatePatientRules(int pacienteId, int? terapeutaId, DateTime utcDate, int duration, int? excludeId = null)
        {
            var dayStart = utcDate.Date;
            var dayEnd = dayStart.AddDays(1).AddTicks(-1);
            var todayCitas = (await _unitOfWork.Citas.GetByDateRangeAsync(dayStart, dayEnd))
                                .Where(a => a.PacienteId == pacienteId && a.Id != excludeId && a.Estado != "Cancelled").ToList();

            if (terapeutaId.HasValue)
            {
                var t = await _unitOfWork.Terapeutas.GetByIdAsync(terapeutaId.Value);
                if (t != null)
                {
                    foreach (var c in todayCitas)
                    {
                        if (!c.TerapeutaId.HasValue) continue;
                        var existingT = await _unitOfWork.Terapeutas.GetByIdAsync(c.TerapeutaId.Value);
                        if (existingT != null && existingT.EspecialidadId == t.EspecialidadId)
                        {
                            throw new BusinessRuleException("OnePerSpecialityPerDay", "Ya tiene cita de esta especialidad hoy.");
                        }
                    }
                }
            }

            foreach (var c in todayCitas)
            {
                var cEnd = c.Fecha.AddMinutes(c.DuracionMinutos);
                var newEnd = utcDate.AddMinutes(duration);
                if (!(cEnd.AddMinutes(30) <= utcDate || newEnd.AddMinutes(30) <= c.Fecha))
                    throw new BusinessRuleException("MinGap", "Debe haber 30 min entre citas.");
            }
        }

        #endregion
    }
}


