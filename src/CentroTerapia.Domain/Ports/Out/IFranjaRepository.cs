using CentroTerapia.Domain.Entities;

namespace CentroTerapia.Domain.Ports.Out
{
    public interface IFranjaRepository : IRepository<FranjaDisponibilidad>
    {
        Task<IEnumerable<FranjaDisponibilidad>> GetByTerapeutaIdAsync(int terapeutaId);
    }
}
