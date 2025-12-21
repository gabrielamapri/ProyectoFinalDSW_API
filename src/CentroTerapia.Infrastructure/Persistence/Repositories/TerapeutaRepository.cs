using Microsoft.EntityFrameworkCore;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Ports.Out;
using CentroTerapia.Infrastructure.Persistence.Context;

namespace CentroTerapia.Infrastructure.Persistence.Repositories
{
    public class TerapeutaRepository : Repository<Terapeuta>, ITerapeutaRepository
    {
        public TerapeutaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Terapeuta>> GetByActivoAsync(bool activo)
        {
            return await _dbSet.Where(t => EF.Property<bool>(t, "Activo") == activo)
                .Include(t => t.Especialidad)
                .ToListAsync();
        }

        public async Task<Terapeuta?> GetWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(t => t.Franjas)
                .Include(t => t.Citas)
                .Include(t => t.Especialidad)
                .FirstOrDefaultAsync(t => EF.Property<int>(t, "Id") == id);
        }

        public async Task<(IEnumerable<Terapeuta> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search, bool? activo)
        {
            var query = _dbSet.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(t => t.Nombres.ToLower().Contains(s) || t.Apellidos.ToLower().Contains(s));
            }
            if (activo.HasValue)
            {
                query = query.Where(t => t.Activo == activo.Value);
            }

            var total = await query.CountAsync();
            var items = await query
                .Include(t => t.Especialidad)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (items, total);
        }
    }
}
