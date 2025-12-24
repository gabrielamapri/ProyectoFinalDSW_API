using AutoMapper;
using Microsoft.Extensions.Logging;
using CentroTerapia.Application.DTOs.Franja;
using System.Linq;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Exceptions;
using CentroTerapia.Domain.Ports.Out;

namespace CentroTerapia.Application.Services
{
    public class FranjaService : IFranjaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<FranjaService> _logger;

        public FranjaService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<FranjaService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<FranjaDisponibilidadDto> CreateAsync(CreateFranjaDto dto)
        {
            var entity = _mapper.Map<FranjaDisponibilidad>(dto);
            var created = await _unitOfWork.Franjas.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<FranjaDisponibilidadDto>(created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exists = await _unitOfWork.Franjas.ExistsAsync(id);
            if (!exists) throw new NotFoundException("FranjaDisponibilidad", id);
            var result = await _unitOfWork.Franjas.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<IEnumerable<FranjaDisponibilidadDto>> GetAllAsync()
        {
            var items = await _unitOfWork.Franjas.GetAllWithRelationsAsync();
            return _mapper.Map<IEnumerable<FranjaDisponibilidadDto>>(items);
        }

        public async Task<IEnumerable<FranjaDisponibilidadDto>> GetAllAsync(string? search = null)
        {
            var items = await _unitOfWork.Franjas.GetAllWithRelationsAsync();
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower().Trim();
                items = items.Where(f =>
                    (f.Terapeuta != null && (f.Terapeuta.Nombres + " " + f.Terapeuta.Apellidos).ToLower().Contains(searchLower)) ||
                    (f.Fecha.HasValue && f.Fecha.Value.ToString("yyyy-MM-dd").Contains(searchLower)) ||
                    (f.Fecha.HasValue && f.Fecha.Value.ToString("dd/MM/yyyy").Contains(searchLower)) ||
                    (f.DiaSemana.HasValue && f.DiaSemana.Value.ToString().Contains(searchLower))
                ).ToList();
            }
            
            return _mapper.Map<IEnumerable<FranjaDisponibilidadDto>>(items);
        }
        public async Task<FranjaDisponibilidadDto> GetByIdAsync(int id)
        {
            var item = await _unitOfWork.Franjas.GetByIdWithRelationsAsync(id);
            if (item == null) throw new NotFoundException("FranjaDisponibilidad", id);
            return _mapper.Map<FranjaDisponibilidadDto>(item);
        }
        public async Task<IEnumerable<FranjaDisponibilidadDto>> GetByTerapeutaIdAsync(int terapeutaId)
        {
            var items = await _unitOfWork.Franjas.GetByTerapeutaIdAsync(terapeutaId);
            return _mapper.Map<IEnumerable<FranjaDisponibilidadDto>>(items);
        }

        public async Task<FranjaDisponibilidadDto> UpdateAsync(int id, CreateFranjaDto dto)
        {
            var item = await _unitOfWork.Franjas.GetByIdWithRelationsAsync(id);
            if (item == null) throw new NotFoundException("FranjaDisponibilidad", id);
            _mapper.Map(dto, item);
            var updated = await _unitOfWork.Franjas.UpdateAsync(item);
            await _unitOfWork.SaveChangesAsync();
            // reload with relations to include Terapeuta
            var reloaded = await _unitOfWork.Franjas.GetByIdWithRelationsAsync(updated.Id);
            return _mapper.Map<FranjaDisponibilidadDto>(reloaded ?? updated);
        }

        public async Task<IEnumerable<CentroTerapia.Application.DTOs.Franja.SlotDto>> GetAvailableSlotsAsync(int terapeutaId, DateTime date, int duracionMinutos)
        {
            var result = new List<CentroTerapia.Application.DTOs.Franja.SlotDto>();

            var franjas = await _unitOfWork.Franjas.GetByTerapeutaIdAsync(terapeutaId);
            if (franjas == null || !franjas.Any()) return result;

            // filter franjas applicable for the date
            var applicable = new List<FranjaDisponibilidad>();
            foreach (var f in franjas)
            {
                // Log values to help debug weekday mapping issues (temporary)
                _logger.LogInformation("FranjaId={FranjaId} TerapeutaId={TerapeutaId} DiaSemana={DiaSemana} DateDayOfWeek={DateDayOfWeek} Date={Date} Recurrente={Recurrente}",
                    f.Id, f.TerapeutaId, f.DiaSemana, (int)date.DayOfWeek, date.ToString("yyyy-MM-dd"), f.Recurrente);

                var isApplicable = (f.Recurrente && f.DiaSemana.HasValue && f.DiaSemana.Value == (int)date.DayOfWeek)
                    || (!f.Recurrente && f.Fecha.HasValue && f.Fecha.Value.Date == date.Date);
                if (!isApplicable) continue;
                // check for exception for this franja on the date
                var hasException = await _unitOfWork.Excepciones.ExistsAsync(f.Id, date.Date);
                if (hasException) continue;
                applicable.Add(f);
            }

            if (!applicable.Any()) return result;

            // fetch appointments for that day for the therapist
            var dayStart = date.Date;
            var dayEnd = date.Date.AddDays(1).AddTicks(-1);
            var citas = (await _unitOfWork.Citas.GetByDateRangeAsync(dayStart, dayEnd)).Where(c => c.TerapeutaId == terapeutaId && c.Estado != "Cancelled").ToList();

            foreach (var f in applicable)
            {
                var start = date.Date.Add(f.HoraInicio);
                var end = date.Date.Add(f.HoraFin);
                var slotStart = start;
                while (slotStart.AddMinutes(duracionMinutos) <= end)
                {
                    var slotEnd = slotStart.AddMinutes(duracionMinutos);
                    // check overlap with existing citas
                    var overlaps = citas.Any(a =>
                    {
                        var existingStart = a.Fecha;
                        var existingDuration = a.DuracionMinutos > 0 ? a.DuracionMinutos : duracionMinutos;
                        var existingEnd = existingStart.AddMinutes(existingDuration);
                        return existingStart < slotEnd && existingEnd > slotStart;
                    });

                    if (!overlaps)
                    {
                        result.Add(new CentroTerapia.Application.DTOs.Franja.SlotDto { Inicio = slotStart, Fin = slotEnd });
                    }

                    slotStart = slotStart.AddMinutes(duracionMinutos);
                }
            }

            return result.OrderBy(s => s.Inicio);
        }

        public async Task<IEnumerable<DateTime>> GetAvailableDatesAsync(int terapeutaId, DateTime start, DateTime end, int duracionMinutos)
        {
            var dates = new List<DateTime>();
            if (end < start) return dates;

            // Load franjas and citas once to avoid N queries per day
            var franjas = (await _unitOfWork.Franjas.GetByTerapeutaIdAsync(terapeutaId)).ToList();
            if (!franjas.Any()) return dates;

            var dayStart = start.Date;
            var dayEnd = end.Date.AddDays(1).AddTicks(-1);
            var citas = (await _unitOfWork.Citas.GetByDateRangeAsync(dayStart, dayEnd))
                .Where(c => c.TerapeutaId == terapeutaId && c.Estado != "Cancelled")
                .ToList();

            // Load all exceptions for the range
            var allExceptions = new HashSet<(int, DateTime)>();
            foreach (var f in franjas)
            {
                var exceptions = await _unitOfWork.Excepciones.GetByFranjaIdInRangeAsync(f.Id, dayStart.Date, dayEnd.Date);
                foreach (var exc in exceptions)
                {
                    allExceptions.Add((f.Id, exc.Fecha.Date));
                }
            }

            // Now iterate through dates and filter in memory
            var current = start.Date;
            while (current <= end.Date)
            {
                // Check if any franja is applicable for this date
                var applicable = franjas.Where(f =>
                {
                    var isApplicable = (f.Recurrente && f.DiaSemana.HasValue && f.DiaSemana.Value == (int)current.DayOfWeek)
                        || (!f.Recurrente && f.Fecha.HasValue && f.Fecha.Value.Date == current.Date);
                    if (!isApplicable) return false;
                    return !allExceptions.Contains((f.Id, current.Date));
                }).ToList();

                if (applicable.Any())
                {
                    // Check if there are available slots on this date
                    bool hasSlots = false;
                    foreach (var f in applicable)
                    {
                        var start_time = current.Date.Add(f.HoraInicio);
                        var end_time = current.Date.Add(f.HoraFin);
                        var slotStart = start_time;
                        while (slotStart.AddMinutes(duracionMinutos) <= end_time)
                        {
                            var slotEnd = slotStart.AddMinutes(duracionMinutos);
                            var overlaps = citas.Any(a =>
                            {
                                var existingStart = a.Fecha;
                                var existingDuration = a.DuracionMinutos > 0 ? a.DuracionMinutos : duracionMinutos;
                                var existingEnd = existingStart.AddMinutes(existingDuration);
                                return existingStart < slotEnd && existingEnd > slotStart;
                            });
                            if (!overlaps)
                            {
                                hasSlots = true;
                                break;
                            }
                            slotStart = slotStart.AddMinutes(duracionMinutos);
                        }
                        if (hasSlots) break;
                    }
                    if (hasSlots) dates.Add(current);
                }

                current = current.AddDays(1);
            }

            return dates;
        }

        public async Task<IEnumerable<FranjaExcepcionDto>> AddExceptionAsync(int franjaId, AddFranjaExcepcionDto dto)
        {
            var franja = await _unitOfWork.Franjas.GetByIdWithRelationsAsync(franjaId);
            if (franja == null) throw new NotFoundException("FranjaDisponibilidad", franjaId);

            var desde = dto.Fecha.Date;
            var hasta = dto.Hasta?.Date ?? desde;
            if (hasta < desde) throw new BusinessRuleException("InvalidRange", "La fecha 'Hasta' no puede ser menor que 'Desde'.");

            var existing = await _unitOfWork.Excepciones.GetByFranjaIdInRangeAsync(franjaId, desde, hasta);
            if (existing.Any()) throw new BusinessRuleException("AlreadyExists", "Ya existe una excepción en alguna fecha del rango seleccionado.");

            var current = desde;
            while (current <= hasta)
            {
                var entity = new Domain.Entities.FranjaExcepcion { FranjaId = franjaId, Fecha = current, Motivo = dto.Motivo };
                await _unitOfWork.Excepciones.CreateAsync(entity);
                current = current.AddDays(1);
            }
            await _unitOfWork.SaveChangesAsync();

            // return all exceptions for this franja (simple convenience)
            var list = await _unitOfWork.Excepciones.GetByFranjaIdInRangeAsync(franjaId, desde.AddYears(-1), hasta.AddYears(1));
            return _mapper.Map<IEnumerable<FranjaExcepcionDto>>(list);
        }

        public async Task<(IEnumerable<FranjaExcepcionDetalleDto> Items, int Total)> GetExcepcionesAsync(int page, int pageSize, int? terapeutaId, int? franjaId, DateTime? from, DateTime? to, string? search = null)
        {
            var (items, total) = await _unitOfWork.Excepciones.GetPagedWithDetailsAsync(page, pageSize, terapeutaId, franjaId, from?.Date, to?.Date, search);
            return (_mapper.Map<IEnumerable<FranjaExcepcionDetalleDto>>(items), total);
        }

        public async Task<IEnumerable<FranjaExcepcionDetalleDto>> AddExceptionsByTerapeutaAsync(int terapeutaId, AddExcepcionesTerapeutaDto dto)
        {
            var desde = dto.Desde.Date;
            var hasta = dto.Hasta?.Date ?? desde;
            if (hasta < desde) throw new BusinessRuleException("InvalidRange", "La fecha 'Hasta' no puede ser menor que 'Desde'.");

            var franjas = dto.FranjaId.HasValue
                ? new List<FranjaDisponibilidad>()
                : (await _unitOfWork.Franjas.GetByTerapeutaIdAsync(terapeutaId)).ToList();

            if (dto.FranjaId.HasValue)
            {
                var franja = await _unitOfWork.Franjas.GetByIdWithRelationsAsync(dto.FranjaId.Value);
                if (franja != null) franjas.Add(franja);
            }

            if (!franjas.Any()) throw new NotFoundException("FranjaDisponibilidad", dto.FranjaId ?? terapeutaId);

            if (dto.FranjaId.HasValue && franjas.First().TerapeutaId != terapeutaId)
            {
                throw new BusinessRuleException("InvalidFranja", "La franja no pertenece al terapeuta indicado.");
            }

            var createdAny = false;
            foreach (var franja in franjas)
            {
                var existing = await _unitOfWork.Excepciones.GetByFranjaIdInRangeAsync(franja.Id, desde, hasta);
                var blockedDates = new HashSet<DateTime>(existing.Select(e => e.Fecha.Date));

                var current = desde;
                while (current <= hasta)
                {
                    var applies = franja.Recurrente
                        ? (franja.DiaSemana.HasValue && franja.DiaSemana.Value == (int)current.DayOfWeek)
                        : (franja.Fecha.HasValue && franja.Fecha.Value.Date == current.Date);

                    if (applies && !blockedDates.Contains(current.Date))
                    {
                        await _unitOfWork.Excepciones.CreateAsync(new Domain.Entities.FranjaExcepcion
                        {
                            FranjaId = franja.Id,
                            Fecha = current,
                            Motivo = dto.Motivo
                        });
                        createdAny = true;
                    }
                    current = current.AddDays(1);
                }
            }

            if (createdAny)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            var exceptions = await _unitOfWork.Excepciones.GetByTerapeutaInRangeWithDetailsAsync(terapeutaId, desde, hasta);

            return _mapper.Map<IEnumerable<FranjaExcepcionDetalleDto>>(exceptions);
        }

        public async Task<bool> RemoveExceptionByIdAsync(int excepcionId)
        {
            var entity = _unitOfWork.Excepciones.Query().FirstOrDefault(e => e.Id == excepcionId);
            if (entity == null) return false;
            var deleted = await _unitOfWork.Excepciones.DeleteAsync(entity.Id);
            if (deleted)
            {
                await _unitOfWork.SaveChangesAsync();
            }
            return deleted;
        }

        public async Task<bool> RemoveExceptionAsync(int franjaId, DateTime fecha)
        {
            var f = _unitOfWork.Excepciones.Query().Where(e => e.FranjaId == franjaId && e.Fecha == fecha.Date).FirstOrDefault();
            if (f == null) return false;
            var deleted = await _unitOfWork.Excepciones.DeleteAsync(f.Id);
            await _unitOfWork.SaveChangesAsync();
            return deleted;
        }
    }
}
