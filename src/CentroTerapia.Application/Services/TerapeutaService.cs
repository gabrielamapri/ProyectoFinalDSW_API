using AutoMapper;
using Microsoft.Extensions.Logging;
using CentroTerapia.Application.DTOs.Terapeuta;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Domain.Ports.Out;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Exceptions;

namespace CentroTerapia.Application.Services
{
    public class TerapeutaService : ITerapeutaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TerapeutaService> _logger;

        public TerapeutaService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TerapeutaService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TerapeutaDto> CreateAsync(CreateTerapeutaDto dto)
        {
            var entity = _mapper.Map<Terapeuta>(dto);
            var created = await _unitOfWork.Terapeutas.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            var withDetails = await _unitOfWork.Terapeutas.GetWithDetailsAsync(created.Id);
            return _mapper.Map<TerapeutaDto>(withDetails ?? created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exists = await _unitOfWork.Terapeutas.ExistsAsync(id);
            if (!exists) throw new NotFoundException("Terapeuta", id);
            var result = await _unitOfWork.Terapeutas.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<IEnumerable<TerapeutaDto>> GetAllAsync()
        {
            var list = await _unitOfWork.Terapeutas.GetAllAsync();
            return _mapper.Map<IEnumerable<TerapeutaDto>>(list);
        }

        public async Task<TerapeutaDto> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.Terapeutas.GetWithDetailsAsync(id);
            if (entity == null) throw new NotFoundException("Terapeuta", id);
            return _mapper.Map<TerapeutaDto>(entity);
        }

        public async Task<IEnumerable<TerapeutaDto>> GetByActivoAsync(bool activo)
        {
            var list = await _unitOfWork.Terapeutas.GetByActivoAsync(activo);
            return _mapper.Map<IEnumerable<TerapeutaDto>>(list);
        }

        public async Task<TerapeutaDto> UpdateAsync(int id, UpdateTerapeutaDto dto)
        {
            var entity = await _unitOfWork.Terapeutas.GetByIdAsync(id);
            if (entity == null) throw new NotFoundException("Terapeuta", id);
            _mapper.Map(dto, entity);
            var updated = await _unitOfWork.Terapeutas.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            var withDetails = await _unitOfWork.Terapeutas.GetWithDetailsAsync(updated.Id);
            return _mapper.Map<TerapeutaDto>(withDetails ?? updated);
        }
    }
}
