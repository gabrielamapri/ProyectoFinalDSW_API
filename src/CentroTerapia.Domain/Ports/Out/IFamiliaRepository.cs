using CentroTerapia.Domain.Entities;

namespace CentroTerapia.Domain.Ports.Out
{
    public interface IFamiliaRepository : IRepository<Familia>
    {
        Task<Familia?> GetWithPacientesAsync(int id);
    }
}
