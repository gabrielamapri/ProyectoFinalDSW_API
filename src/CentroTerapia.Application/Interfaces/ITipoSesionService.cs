using CentroTerapia.Application.DTOs.Terapia;

namespace CentroTerapia.Application.Interfaces
{
    public interface ITipoSesionService
    {
        Task<TipoSesionDto> GetByIdAsync(int id);
        Task<IEnumerable<TipoSesionDto>> GetAllAsync();
        Task<IEnumerable<TipoSesionDto>> GetAllAsync(string? search = null);
        Task<TipoSesionDto> CreateAsync(CreateTipoSesionDto dto);
        Task<TipoSesionDto> UpdateAsync(int id, UpdateTipoSesionDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
