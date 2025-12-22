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
    }
}
