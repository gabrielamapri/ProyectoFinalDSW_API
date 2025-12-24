
using CentroTerapia.Application.DTOs.Cita;

namespace CentroTerapia.Application.Interfaces
{
    public interface ICitaService
    {
        Task<CitaDto> GetByIdAsync(int id);
        Task<IEnumerable<CitaDto>> GetAllAsync();
        Task<IEnumerable<CitaDto>> GetAllAsync(string? search = null);
        Task<CitaDto> CreateAsync(CreateCitaDto citaDto);
        Task<CitaDto> UpdateAsync(int id, UpdateCitaDto citaDto);
        Task<CitaDto> ReprogramAsync(int id, ReprogramCitaDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> CancelAsync(int id);

        Task<IEnumerable<CitaDto>> GetByPacienteIdAsync(int pacienteId);
        Task<IEnumerable<CitaDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<CitaDto>> GetByStatusAsync(string status);
        Task<IEnumerable<CitaAlertaDto>> GetByTerapeutaAndDateRangeAsync(int terapeutaId, DateTime startDate, DateTime endDate);
    }
}

