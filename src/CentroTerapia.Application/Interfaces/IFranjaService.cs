using CentroTerapia.Application.DTOs.Franja;

namespace CentroTerapia.Application.Interfaces
{
    public interface IFranjaService
    {
        Task<FranjaDisponibilidadDto> GetByIdAsync(int id);
        Task<IEnumerable<FranjaDisponibilidadDto>> GetAllAsync();
        Task<IEnumerable<FranjaDisponibilidadDto>> GetAllAsync(string? search = null);
        Task<FranjaDisponibilidadDto> CreateAsync(CreateFranjaDto dto);
        Task<(IEnumerable<FranjaExcepcionDetalleDto> Items, int Total)> GetExcepcionesAsync(int page, int pageSize, int? terapeutaId, int? franjaId, DateTime? from, DateTime? to, string? search = null);
        Task<IEnumerable<FranjaExcepcionDetalleDto>> AddExceptionsByTerapeutaAsync(int terapeutaId, AddExcepcionesTerapeutaDto dto);
        Task<bool> RemoveExceptionByIdAsync(int excepcionId);
        Task<FranjaDisponibilidadDto> UpdateAsync(int id, CreateFranjaDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<FranjaDisponibilidadDto>> GetByTerapeutaIdAsync(int terapeutaId);
        Task<IEnumerable<CentroTerapia.Application.DTOs.Franja.SlotDto>> GetAvailableSlotsAsync(int terapeutaId, DateTime date, int duracionMinutos);
        Task<IEnumerable<DateTime>> GetAvailableDatesAsync(int terapeutaId, DateTime start, DateTime end, int duracionMinutos);
    }
}
