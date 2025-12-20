using AutoMapper;
using Microsoft.Extensions.Logging;
using CentroTerapia.Application.DTOs.Familia;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Exceptions;
using CentroTerapia.Domain.Ports.Out;

namespace CentroTerapia.Application.Services
{
    public class FamiliaService : IFamiliaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<FamiliaService> _logger;

        public FamiliaService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<FamiliaService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<FamiliaDto> CreateAsync(CreateFamiliaDto dto)
        {
            var familia = _mapper.Map<Familia>(dto);
            var created = await _unitOfWork.Familias.CreateAsync(familia);
            await _unitOfWork.SaveChangesAsync();
            var withDetails = await _unitOfWork.Familias.GetWithPacientesAsync(created.Id);
            return _mapper.Map<FamiliaDto>(withDetails ?? created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exists = await _unitOfWork.Familias.ExistsAsync(id);
            if (!exists) throw new NotFoundException("Familia", id);
            var result = await _unitOfWork.Familias.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<IEnumerable<FamiliaDto>> GetAllAsync()
        {
            var items = await _unitOfWork.Familias.GetAllAsync();
            return _mapper.Map<IEnumerable<FamiliaDto>>(items);
        }

        public async Task<FamiliaDto> GetByIdAsync(int id)
        {
            var item = await _unitOfWork.Familias.GetWithPacientesAsync(id);
            if (item == null) throw new NotFoundException("Familia", id);
            return _mapper.Map<FamiliaDto>(item);
        }

        public async Task<FamiliaDto> UpdateAsync(int id, CreateFamiliaDto dto)
        {
            var familia = await _unitOfWork.Familias.GetByIdAsync(id);
            if (familia == null) throw new NotFoundException("Familia", id);
            _mapper.Map(dto, familia);
            var updated = await _unitOfWork.Familias.UpdateAsync(familia);
            await _unitOfWork.SaveChangesAsync();
            var withDetails = await _unitOfWork.Familias.GetWithPacientesAsync(updated.Id);
            return _mapper.Map<FamiliaDto>(withDetails ?? updated);
        }
    }
}
