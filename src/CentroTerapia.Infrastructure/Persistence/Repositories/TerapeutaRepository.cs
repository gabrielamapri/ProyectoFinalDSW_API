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
            return await _dbSet.Where(t => EF.Property<bool>(t, "Activo") == activo).ToListAsync();
        }

        public async Task<Terapeuta?> GetWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(t => t.Franjas)
                .Include(t => t.Citas)
                .FirstOrDefaultAsync(t => EF.Property<int>(t, "Id") == id);
        }
    }
}
