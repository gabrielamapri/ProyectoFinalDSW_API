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

        public async Task<IEnumerable<FranjaExcepcionDto>> AddExceptionAsync(int franjaId, AddFranjaExcepcionDto dto)
        {
            var franja = await _unitOfWork.Franjas.GetByIdWithRelationsAsync(franjaId);
            if (franja == null) throw new NotFoundException("FranjaDisponibilidad", franjaId);

            var fecha = dto.Fecha.Date;
            var exists = await _unitOfWork.Excepciones.ExistsAsync(franjaId, fecha);
            if (exists) throw new BusinessRuleException("AlreadyExists", "Ya existe una excepción para esa fecha.");

            var entity = new Domain.Entities.FranjaExcepcion { FranjaId = franjaId, Fecha = fecha, Motivo = dto.Motivo };
            await _unitOfWork.Excepciones.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            // return all exceptions for this franja (simple convenience)
            var list = await _unitOfWork.Excepciones.GetByFranjaIdInRangeAsync(franjaId, fecha.AddYears(-1), fecha.AddYears(1));
            return _mapper.Map<IEnumerable<FranjaExcepcionDto>>(list);
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
