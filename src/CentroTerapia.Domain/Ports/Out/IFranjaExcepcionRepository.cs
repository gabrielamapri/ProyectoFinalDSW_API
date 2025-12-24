using CentroTerapia.Domain.Entities;

namespace CentroTerapia.Domain.Ports.Out
{
    public interface IFranjaExcepcionRepository : IRepository<FranjaExcepcion>
    {
        Task<bool> ExistsAsync(int franjaId, DateTime fecha);
        Task<IEnumerable<FranjaExcepcion>> GetByFranjaIdInRangeAsync(int franjaId, DateTime from, DateTime to);
        Task<(IEnumerable<FranjaExcepcion> Items, int Total)> GetPagedWithDetailsAsync(int page, int pageSize, int? terapeutaId, int? franjaId, DateTime? from, DateTime? to, string? search = null);
        Task<IEnumerable<FranjaExcepcion>> GetByTerapeutaInRangeWithDetailsAsync(int terapeutaId, DateTime from, DateTime to);
    }
}
