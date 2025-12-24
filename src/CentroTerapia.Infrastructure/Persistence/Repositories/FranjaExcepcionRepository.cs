using Microsoft.EntityFrameworkCore;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Ports.Out;
using CentroTerapia.Infrastructure.Persistence.Context;

namespace CentroTerapia.Infrastructure.Persistence.Repositories
{
    public class FranjaExcepcionRepository : Repository<FranjaExcepcion>, IFranjaExcepcionRepository
    {
        public FranjaExcepcionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsAsync(int franjaId, DateTime fecha)
        {
            return await _dbSet.AnyAsync(e => e.FranjaId == franjaId && e.Fecha.Date == fecha.Date);
        }

        public async Task<IEnumerable<FranjaExcepcion>> GetByFranjaIdInRangeAsync(int franjaId, DateTime from, DateTime to)
        {
            return await _dbSet
                .Where(e => e.FranjaId == franjaId && e.Fecha.Date >= from.Date && e.Fecha.Date <= to.Date)
                .ToListAsync();
        }

        public async Task<(IEnumerable<FranjaExcepcion> Items, int Total)> GetPagedWithDetailsAsync(int page, int pageSize, int? terapeutaId, int? franjaId, DateTime? from, DateTime? to, string? search = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _dbSet
                .Include(e => e.Franja)
                    .ThenInclude(f => f.Terapeuta)
                .AsQueryable();

            if (franjaId.HasValue)
            {
                query = query.Where(e => e.FranjaId == franjaId.Value);
            }

            if (terapeutaId.HasValue)
            {
                query = query.Where(e => e.Franja != null && e.Franja.TerapeutaId == terapeutaId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(e =>
                    e.Franja != null &&
                    e.Franja.Terapeuta != null &&
                    ((e.Franja.Terapeuta.Nombres + " " + e.Franja.Terapeuta.Apellidos).ToLower().Contains(term)));
            }

            if (from.HasValue)
            {
                var d = from.Value.Date;
                query = query.Where(e => e.Fecha.Date >= d);
            }

            if (to.HasValue)
            {
                var d = to.Value.Date;
                query = query.Where(e => e.Fecha.Date <= d);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(e => e.Fecha)
                .ThenByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<IEnumerable<FranjaExcepcion>> GetByTerapeutaInRangeWithDetailsAsync(int terapeutaId, DateTime from, DateTime to)
        {
            var start = from.Date;
            var end = to.Date;

            return await _dbSet
                .Include(e => e.Franja)
                    .ThenInclude(f => f.Terapeuta)
                .Where(e => e.Franja != null && e.Franja.TerapeutaId == terapeutaId && e.Fecha.Date >= start && e.Fecha.Date <= end)
                .ToListAsync();
        }
    }
}
