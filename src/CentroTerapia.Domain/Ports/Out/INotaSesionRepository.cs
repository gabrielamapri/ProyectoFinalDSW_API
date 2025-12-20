using CentroTerapia.Domain.Entities;

namespace CentroTerapia.Domain.Ports.Out
{
    public interface INotaSesionRepository : IRepository<NotaSesion>
    {
        Task<IEnumerable<NotaSesion>> GetByCitaIdAsync(int citaId);
    }
}
