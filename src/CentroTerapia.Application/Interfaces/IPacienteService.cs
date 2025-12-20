using CentroTerapia.Application.DTOs.Paciente;

namespace CentroTerapia.Application.Interfaces
{
    public interface IPacienteService
    {
        Task<PacienteDto> GetByIdAsync(int id);
        Task<IEnumerable<PacienteDto>> GetAllAsync();
        Task<(IEnumerable<PacienteDto> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search, string? sexo);
        Task<PacienteDto> CreateAsync(CreatePacienteDto dto);
        Task<PacienteDto> UpdateAsync(int id, UpdatePacienteDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<PacienteDto>> GetByFamiliaIdAsync(int familiaId);
        Task<PacienteDto> AssignFamilyAsync(int pacienteId, int familiaId);
    }
}
