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

        public override async Task<IEnumerable<FranjaDisponibilidad>> GetAllAsync()
        {
            return await _dbSet
                .Include(f => f.Terapeuta)
                .ToListAsync();
        }

        public override async Task<FranjaDisponibilidad?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(f => f.Terapeuta)
                .FirstOrDefaultAsync(f => EF.Property<int>(f, "Id") == id);
        }

        public async Task<IEnumerable<FranjaDisponibilidad>> GetByTerapeutaIdAsync(int terapeutaId)
        {
            return await _dbSet
                .Include(f => f.Terapeuta)
                .Where(f => f.TerapeutaId == terapeutaId)
                .ToListAsync();
        }
    }
}
