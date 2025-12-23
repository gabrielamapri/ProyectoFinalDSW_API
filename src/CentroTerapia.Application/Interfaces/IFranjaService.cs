using CentroTerapia.Application.DTOs.Franja;

namespace CentroTerapia.Application.Interfaces
{
    public interface IFranjaService
    {
        Task<FranjaDisponibilidadDto> GetByIdAsync(int id);
        Task<IEnumerable<FranjaDisponibilidadDto>> GetAllAsync();
        Task<FranjaDisponibilidadDto> CreateAsync(CreateFranjaDto dto);
        Task<IEnumerable<FranjaExcepcionDto>> AddExceptionAsync(int franjaId, AddFranjaExcepcionDto dto);
        Task<bool> RemoveExceptionAsync(int franjaId, DateTime fecha);
        Task<FranjaDisponibilidadDto> UpdateAsync(int id, CreateFranjaDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<FranjaDisponibilidadDto>> GetByTerapeutaIdAsync(int terapeutaId);
        Task<IEnumerable<CentroTerapia.Application.DTOs.Franja.SlotDto>> GetAvailableSlotsAsync(int terapeutaId, DateTime date, int duracionMinutos);
        Task<IEnumerable<DateTime>> GetAvailableDatesAsync(int terapeutaId, DateTime start, DateTime end, int duracionMinutos);
    }
}
