using CentroTerapia.Application.DTOs.Familia;

namespace CentroTerapia.Application.Interfaces
{
    public interface IFamiliaService
    {
        Task<FamiliaDto> GetByIdAsync(int id);
        Task<IEnumerable<FamiliaDto>> GetAllAsync();
        Task<FamiliaDto> CreateAsync(CreateFamiliaDto dto);
        Task<FamiliaDto> UpdateAsync(int id, CreateFamiliaDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
