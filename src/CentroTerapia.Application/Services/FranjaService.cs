using AutoMapper;
using Microsoft.Extensions.Logging;
using CentroTerapia.Application.DTOs.Franja;
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
            var items = await _unitOfWork.Franjas.GetAllAsync();
            return _mapper.Map<IEnumerable<FranjaDisponibilidadDto>>(items);
        }

        public async Task<FranjaDisponibilidadDto> GetByIdAsync(int id)
        {
            var item = await _unitOfWork.Franjas.GetByIdAsync(id);
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
            var item = await _unitOfWork.Franjas.GetByIdAsync(id);
            if (item == null) throw new NotFoundException("FranjaDisponibilidad", id);
            _mapper.Map(dto, item);
            var updated = await _unitOfWork.Franjas.UpdateAsync(item);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<FranjaDisponibilidadDto>(updated);
        }

        public async Task<IEnumerable<CentroTerapia.Application.DTOs.Franja.SlotDto>> GetAvailableSlotsAsync(int terapeutaId, DateTime date, int duracionMinutos)
        {
            var result = new List<CentroTerapia.Application.DTOs.Franja.SlotDto>();

            var franjas = await _unitOfWork.Franjas.GetByTerapeutaIdAsync(terapeutaId);
            if (franjas == null || !franjas.Any()) return result;

            // filter franjas applicable for the date
            var applicable = franjas.Where(f =>
                (f.Recurrente && f.DiaSemana.HasValue && f.DiaSemana.Value == (int)date.DayOfWeek)
                || (!f.Recurrente && f.Fecha.HasValue && f.Fecha.Value.Date == date.Date)
            ).ToList();

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
    }
}
