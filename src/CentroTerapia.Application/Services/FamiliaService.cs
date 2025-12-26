using System;
using System.Linq;
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

            // Bloquear eliminación si existen pacientes asociados
            var pacientes = await _unitOfWork.Pacientes.GetByFamiliaIdAsync(id) ?? Enumerable.Empty<Paciente>();
            if (pacientes.Any())
            {
                throw new BusinessRuleException(
                    "FamiliaTienePacientes",
                    "No se puede eliminar. Tiene pacientes asociados.");
            }

            var result = await _unitOfWork.Familias.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }


        public async Task<IEnumerable<FamiliaDto>> GetAllAsync(string? userCorreo = null, string? userRol = null)
        {
            var items = await _unitOfWork.Familias.GetAllAsync();
            _logger.LogInformation($"[GetAllAsync] userRol: {userRol}, userCorreo: {userCorreo}");
            // Si es padre, solo mostrar la familia donde el correo coincide
            if (userRol == "Padre" && !string.IsNullOrEmpty(userCorreo))
            {
                var prevCount = items.Count();
                items = items.Where(f => (f.ResponsablePrincipalEmail ?? "").Trim().ToLower() == userCorreo.Trim().ToLower());
                _logger.LogInformation($"[GetAllAsync] Familias filtradas: {items.Count()} de {prevCount}");
            }
            // Si es Terapeuta o Admin, no se filtra: pueden ver todas las familias
            return _mapper.Map<IEnumerable<FamiliaDto>>(items);
        }

        public async Task<(IEnumerable<FamiliaDto> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search, string? userCorreo = null, string? userRol = null)
        {
            var allItems = await _unitOfWork.Familias.GetAllAsync();
            _logger.LogInformation($"[GetPagedAsync] userRol: {userRol}, userCorreo: {userCorreo}");
            // Si es padre, solo mostrar la familia donde el correo coincide
            if (userRol == "Padre" && !string.IsNullOrEmpty(userCorreo))
            {
                var prevCount = allItems.Count();
                allItems = allItems.Where(f => (f.ResponsablePrincipalEmail ?? "").Trim().ToLower() == userCorreo.Trim().ToLower());
                _logger.LogInformation($"[GetPagedAsync] Familias filtradas: {allItems.Count()} de {prevCount}");
            }
            // Si es Terapeuta o Admin, no se filtra: pueden ver todas las familias
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                allItems = allItems.Where(f =>
                    (f.ResponsablePrincipalNombre ?? string.Empty).ToLower().Contains(s) ||
                    (f.ResponsablePrincipalApellido ?? string.Empty).ToLower().Contains(s) ||
                    (f.ResponsablePrincipalDNI ?? string.Empty).ToLower().Contains(s) ||
                    (f.Responsable2Nombre ?? string.Empty).ToLower().Contains(s) ||
                    (f.Responsable2Apellido ?? string.Empty).ToLower().Contains(s) ||
                    (f.Responsable2DNI ?? string.Empty).ToLower().Contains(s)
                );
            }
            var total = allItems.Count();
            var items = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return (_mapper.Map<IEnumerable<FamiliaDto>>(items), total);
        }


        public async Task<FamiliaDto> GetByIdAsync(int id, string? userCorreo = null, string? userRol = null)
        {
            var item = await _unitOfWork.Familias.GetWithPacientesAsync(id);
            if (item == null) throw new NotFoundException("Familia", id);
            // Si es padre, solo puede ver su propia familia
            if (userRol == "Padre" && !string.IsNullOrEmpty(userCorreo) && item.ResponsablePrincipalEmail != userCorreo)
                throw new NotFoundException("Familia", id);
            return _mapper.Map<FamiliaDto>(item);
        }

        public async Task<FamiliaDto> UpdateAsync(int id, CreateFamiliaDto dto)
        {
            var familia = await _unitOfWork.Familias.GetByIdAsync(id);
            if (familia == null) throw new NotFoundException("Familia", id);
            _mapper.Map(dto, familia);
            // Actualizar timestamp de modificación
            familia.FechaActualizacion = DateTime.UtcNow;
            var updated = await _unitOfWork.Familias.UpdateAsync(familia);
            await _unitOfWork.SaveChangesAsync();
            var withDetails = await _unitOfWork.Familias.GetWithPacientesAsync(updated.Id);
            return _mapper.Map<FamiliaDto>(withDetails ?? updated);
        }
    }
}
