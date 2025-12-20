using CentroTerapia.Application.DTOs.Terapeuta;

namespace CentroTerapia.Application.Interfaces
{
    public interface ITerapeutaService
    {
        Task<TerapeutaDto> GetByIdAsync(int id);
        Task<IEnumerable<TerapeutaDto>> GetAllAsync();
        Task<TerapeutaDto> CreateAsync(CreateTerapeutaDto dto);
        Task<TerapeutaDto> UpdateAsync(int id, UpdateTerapeutaDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<TerapeutaDto>> GetByActivoAsync(bool activo);
    }
}
