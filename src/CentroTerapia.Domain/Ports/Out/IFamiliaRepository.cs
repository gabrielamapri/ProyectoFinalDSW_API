using CentroTerapia.Domain.Entities;

namespace CentroTerapia.Domain.Ports.Out
{
    public interface IFamiliaRepository : IRepository<Familia>
    {
        Task<Familia?> GetWithPacientesAsync(int id);
        Task<(IEnumerable<Familia> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search);
    }
}
