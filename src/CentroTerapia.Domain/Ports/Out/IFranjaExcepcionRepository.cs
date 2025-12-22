using CentroTerapia.Domain.Entities;

namespace CentroTerapia.Domain.Ports.Out
{
    public interface IFranjaExcepcionRepository : IRepository<FranjaExcepcion>
    {
        Task<bool> ExistsAsync(int franjaId, DateTime fecha);
        Task<IEnumerable<FranjaExcepcion>> GetByFranjaIdInRangeAsync(int franjaId, DateTime from, DateTime to);
    }
}
