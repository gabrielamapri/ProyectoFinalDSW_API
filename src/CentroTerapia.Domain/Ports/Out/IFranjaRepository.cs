using CentroTerapia.Domain.Entities;

namespace CentroTerapia.Domain.Ports.Out
{
    public interface IFranjaRepository : IRepository<FranjaDisponibilidad>
    {
        Task<IEnumerable<FranjaDisponibilidad>> GetByTerapeutaIdAsync(int terapeutaId);
        Task<IEnumerable<FranjaDisponibilidad>> GetAllWithRelationsAsync();
        Task<FranjaDisponibilidad?> GetByIdWithRelationsAsync(int id);
    }
}
