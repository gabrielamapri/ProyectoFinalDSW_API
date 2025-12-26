using CentroTerapia.Application.DTOs.Familia;

namespace CentroTerapia.Application.Interfaces
{
    public interface IFamiliaService
    {
        Task<FamiliaDto> GetByIdAsync(int id, string? userCorreo = null, string? userRol = null);
        Task<IEnumerable<FamiliaDto>> GetAllAsync(string? userCorreo = null, string? userRol = null);
        Task<(IEnumerable<FamiliaDto> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search, string? userCorreo = null, string? userRol = null);
        Task<FamiliaDto> CreateAsync(CreateFamiliaDto dto);
        Task<FamiliaDto> UpdateAsync(int id, CreateFamiliaDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
