using CentroTerapia.Application.DTOs.NotaSesion;

namespace CentroTerapia.Application.Interfaces
{
    public interface INotaSesionService
    {
        Task<NotaSesionDto> GetByIdAsync(int id);
        Task<IEnumerable<NotaSesionDto>> GetAllAsync();
        Task<NotaSesionDto> CreateAsync(CreateNotaSesionDto dto);
        Task<NotaSesionDto> UpdateAsync(int id, UpdateNotaSesionDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<NotaSesionDto>> GetByCitaIdAsync(int citaId);
    }
}
