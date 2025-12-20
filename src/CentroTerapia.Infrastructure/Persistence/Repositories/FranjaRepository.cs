using Microsoft.EntityFrameworkCore;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Ports.Out;
using CentroTerapia.Infrastructure.Persistence.Context;

namespace CentroTerapia.Infrastructure.Persistence.Repositories
{
    public class FranjaRepository : Repository<FranjaDisponibilidad>, IFranjaRepository
    {
        public FranjaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<FranjaDisponibilidad>> GetByTerapeutaIdAsync(int terapeutaId)
        {
            return await _dbSet
                .Where(f => f.TerapeutaId == terapeutaId)
                .ToListAsync();
        }
    }
}
