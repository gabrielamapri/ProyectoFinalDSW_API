using CentroTerapia.Application.DTOs.Familia;

namespace CentroTerapia.Application.Interfaces
{
    public interface IFamiliaService
    {
        Task<FamiliaDto> GetByIdAsync(int id);
        Task<IEnumerable<FamiliaDto>> GetAllAsync();
        Task<(IEnumerable<FamiliaDto> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search);
        Task<FamiliaDto> CreateAsync(CreateFamiliaDto dto);
        Task<FamiliaDto> UpdateAsync(int id, CreateFamiliaDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
