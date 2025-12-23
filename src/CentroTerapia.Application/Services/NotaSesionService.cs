using AutoMapper;
using Microsoft.Extensions.Logging;
using CentroTerapia.Application.DTOs.NotaSesion;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Exceptions;
using CentroTerapia.Domain.Ports.Out;

namespace CentroTerapia.Application.Services
{
    public class NotaSesionService : INotaSesionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<NotaSesionService> _logger;

        public NotaSesionService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<NotaSesionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<NotaSesionDto> CreateAsync(CreateNotaSesionDto dto)
        {
            var entity = _mapper.Map<NotaSesion>(dto);
            var created = await _unitOfWork.NotasSesion.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<NotaSesionDto>(created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exists = await _unitOfWork.NotasSesion.ExistsAsync(id);
            if (!exists) throw new NotFoundException("NotaSesion", id);
            var result = await _unitOfWork.NotasSesion.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<IEnumerable<NotaSesionDto>> GetAllAsync()
        {
            var items = await _unitOfWork.NotasSesion.GetAllAsync();
            return _mapper.Map<IEnumerable<NotaSesionDto>>(items);
        }

        public async Task<IEnumerable<NotaSesionDto>> GetAllAsync(string? search = null)
        {
            var items = await _unitOfWork.NotasSesion.GetAllAsync();
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower().Trim();
                items = items.Where(n =>
                    (n.Cita != null && n.Cita.Paciente != null && (n.Cita.Paciente.Nombres + " " + n.Cita.Paciente.Apellidos).ToLower().Contains(searchLower)) ||
                    (n.Terapeuta != null && (n.Terapeuta.Nombres + " " + n.Terapeuta.Apellidos).ToLower().Contains(searchLower)) ||
                    (n.Cita != null && n.Cita.Fecha.ToString("yyyy-MM-dd").Contains(searchLower)) ||
                    (n.Cita != null && n.Cita.Fecha.ToString("dd/MM/yyyy").Contains(searchLower)) ||
                    (n.FechaCreacion.ToString("yyyy-MM-dd").Contains(searchLower)) ||
                    (n.FechaCreacion.ToString("dd/MM/yyyy").Contains(searchLower))
                ).ToList();
            }
            
            return _mapper.Map<IEnumerable<NotaSesionDto>>(items);
        }

        public async Task<NotaSesionDto> GetByIdAsync(int id)
        {
            var item = await _unitOfWork.NotasSesion.GetByIdAsync(id);
            if (item == null) throw new NotFoundException("NotaSesion", id);
            return _mapper.Map<NotaSesionDto>(item);
        }

        public async Task<IEnumerable<NotaSesionDto>> GetByCitaIdAsync(int citaId)
        {
            var items = await _unitOfWork.NotasSesion.GetByCitaIdAsync(citaId);
            return _mapper.Map<IEnumerable<NotaSesionDto>>(items);
        }

        public async Task<NotaSesionDto> UpdateAsync(int id, UpdateNotaSesionDto dto)
        {
            var item = await _unitOfWork.NotasSesion.GetByIdAsync(id);
            if (item == null) throw new NotFoundException("NotaSesion", id);
            _mapper.Map(dto, item);
            var updated = await _unitOfWork.NotasSesion.UpdateAsync(item);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<NotaSesionDto>(updated);
        }
    }
}
