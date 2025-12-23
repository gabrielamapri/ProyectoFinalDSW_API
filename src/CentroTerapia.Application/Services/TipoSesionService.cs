using AutoMapper;
using Microsoft.Extensions.Logging;
using CentroTerapia.Application.DTOs.Terapia;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Exceptions;
using CentroTerapia.Domain.Ports.Out;

namespace CentroTerapia.Application.Services
{
    public class TipoSesionService : ITipoSesionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TipoSesionService> _logger;

        public TipoSesionService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TipoSesionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TipoSesionDto> CreateAsync(CreateTipoSesionDto dto)
        {
            var entity = _mapper.Map<TipoSesion>(dto);
            var created = await _unitOfWork.TiposSesion.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<TipoSesionDto>(created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exists = await _unitOfWork.TiposSesion.ExistsAsync(id);
            if (!exists) throw new NotFoundException("TipoSesion", id);
            var result = await _unitOfWork.TiposSesion.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<IEnumerable<TipoSesionDto>> GetAllAsync()
        {
            var items = await _unitOfWork.TiposSesion.GetAllAsync();
            return _mapper.Map<IEnumerable<TipoSesionDto>>(items);
        }

        public async Task<IEnumerable<TipoSesionDto>> GetAllAsync(string? search = null)
        {
            var items = await _unitOfWork.TiposSesion.GetAllAsync();
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower().Trim();
                items = items.Where(t => 
                    t.Nombre != null && t.Nombre.ToLower().Contains(searchLower)
                ).ToList();
            }
            
            return _mapper.Map<IEnumerable<TipoSesionDto>>(items);
        }

        public async Task<TipoSesionDto> GetByIdAsync(int id)
        {
            var item = await _unitOfWork.TiposSesion.GetByIdAsync(id);
            if (item == null) throw new NotFoundException("TipoSesion", id);
            return _mapper.Map<TipoSesionDto>(item);
        }

        public async Task<TipoSesionDto> UpdateAsync(int id, UpdateTipoSesionDto dto)
        {
            var item = await _unitOfWork.TiposSesion.GetByIdAsync(id);
            if (item == null) throw new NotFoundException("TipoSesion", id);
            _mapper.Map(dto, item);
            var updated = await _unitOfWork.TiposSesion.UpdateAsync(item);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<TipoSesionDto>(updated);
        }
    }
}
