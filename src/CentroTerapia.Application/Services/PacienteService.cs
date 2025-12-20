using AutoMapper;
using Microsoft.Extensions.Logging;
using CentroTerapia.Application.DTOs.Paciente;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Ports.Out;
using CentroTerapia.Domain.Exceptions;

namespace CentroTerapia.Application.Services
{
    public class PacienteService : IPacienteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PacienteService> _logger;

        public PacienteService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PacienteService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PacienteDto> CreateAsync(CreatePacienteDto dto)
        {
            var paciente = _mapper.Map<Paciente>(dto);
            var created = await _unitOfWork.Pacientes.CreateAsync(paciente);
            await _unitOfWork.SaveChangesAsync();
            var withDetails = await _unitOfWork.Pacientes.GetWithFamiliaAndCitasAsync(created.Id);
            return _mapper.Map<PacienteDto>(withDetails ?? created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exists = await _unitOfWork.Pacientes.ExistsAsync(id);
            if (!exists) throw new NotFoundException("Paciente", id);
            var result = await _unitOfWork.Pacientes.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<IEnumerable<PacienteDto>> GetAllAsync()
        {
            var pacientes = await _unitOfWork.Pacientes.GetAllAsync();
            return _mapper.Map<IEnumerable<PacienteDto>>(pacientes);
        }

        public async Task<(IEnumerable<PacienteDto> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search, string? sexo)
        {
            CentroTerapia.Domain.Enums.Sexo? sexoEnum = null;
            if (!string.IsNullOrWhiteSpace(sexo))
            {
                if (Enum.TryParse<CentroTerapia.Domain.Enums.Sexo>(sexo, true, out var parsed)) sexoEnum = parsed;
            }

            var (items, total) = await _unitOfWork.Pacientes.GetPagedAsync(page <= 0 ? 1 : page, pageSize <=0 ? 10 : pageSize, search, sexoEnum);
            return (_mapper.Map<IEnumerable<PacienteDto>>(items), total);
        }

        public async Task<PacienteDto> GetByIdAsync(int id)
        {
            var paciente = await _unitOfWork.Pacientes.GetWithFamiliaAndCitasAsync(id);
            if (paciente == null) throw new NotFoundException("Paciente", id);
            return _mapper.Map<PacienteDto>(paciente);
        }

        public async Task<IEnumerable<PacienteDto>> GetByFamiliaIdAsync(int familiaId)
        {
            var pacientes = await _unitOfWork.Pacientes.GetByFamiliaIdAsync(familiaId);
            return _mapper.Map<IEnumerable<PacienteDto>>(pacientes);
        }

        public async Task<PacienteDto> UpdateAsync(int id, UpdatePacienteDto dto)
        {
            var paciente = await _unitOfWork.Pacientes.GetByIdAsync(id);
            if (paciente == null) throw new NotFoundException("Paciente", id);
            _mapper.Map(dto, paciente);
            var updated = await _unitOfWork.Pacientes.UpdateAsync(paciente);
            await _unitOfWork.SaveChangesAsync();
            var withDetails = await _unitOfWork.Pacientes.GetWithFamiliaAndCitasAsync(updated.Id);
            return _mapper.Map<PacienteDto>(withDetails ?? updated);
        }
    }
}
