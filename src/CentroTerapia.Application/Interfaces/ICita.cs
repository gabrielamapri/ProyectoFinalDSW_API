
using CentroTerapia.Application.DTOs.Cita;

namespace CentroTerapia.Application.Interfaces
{
    public interface ICitaService
    {
        Task<CitaDto> GetByIdAsync(int id);
        Task<IEnumerable<CitaDto>> GetAllAsync();
        Task<CitaDto> CreateAsync(CreateCitaDto citaDto);
        Task<CitaDto> UpdateAsync(int id, UpdateCitaDto citaDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> CancelAsync(int id);

        Task<IEnumerable<CitaDto>> GetByPacienteIdAsync(int pacienteId);
        Task<IEnumerable<CitaDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<CitaDto>> GetByStatusAsync(string status);
    }
}

