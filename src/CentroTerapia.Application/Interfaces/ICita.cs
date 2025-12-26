using CentroTerapia.Application.DTOs.Cita;

namespace CentroTerapia.Application.Interfaces
{
    public interface ICitaService
    {
        Task<CitaDto> GetByIdAsync(int id);
        Task<IEnumerable<CitaDto>> GetAllAsync(string? search = null);
        Task<IEnumerable<CitaDto>> GetByPacienteIdAsync(int pacienteId);
        Task<IEnumerable<CitaDto>> GetByPacienteIdsAsync(IEnumerable<int> pacienteIds, string? search = null);
        Task<IEnumerable<CitaDto>> GetByTerapeutaIdAsync(int terapeutaId);
        Task<IEnumerable<CitaDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<CitaDto>> GetByStatusAsync(string status);
        Task<IEnumerable<CitaAlertaDto>> GetByTerapeutaAndDateRangeAsync(int terapeutaId, DateTime startDate, DateTime endDate);
        Task<CitaDto> CreateAsync(CreateCitaDto dto); // Cambiado a 'dto' para match
        Task<CitaDto> UpdateAsync(int id, UpdateCitaDto dto); // Cambiado a 'dto' para match
        Task<CitaDto> ReprogramAsync(int id, ReprogramCitaDto dto);
        Task<bool> CancelAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}

