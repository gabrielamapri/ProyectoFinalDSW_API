using CentroTerapia.Domain.Entities;

namespace CentroTerapia.Domain.Ports.Out
{
    public interface ITerapeutaRepository : IRepository<Terapeuta>
    {
        Task<Terapeuta?> GetWithDetailsAsync(int id);
        Task<IEnumerable<Terapeuta>> GetByActivoAsync(bool activo);
        Task<(IEnumerable<Terapeuta> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search, bool? activo);
    }
}
