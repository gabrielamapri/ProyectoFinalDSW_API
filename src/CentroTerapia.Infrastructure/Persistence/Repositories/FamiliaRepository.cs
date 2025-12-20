using Microsoft.EntityFrameworkCore;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Ports.Out;
using CentroTerapia.Infrastructure.Persistence.Context;

namespace CentroTerapia.Infrastructure.Persistence.Repositories
{
    public class FamiliaRepository : Repository<Familia>, IFamiliaRepository
    {
        public FamiliaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Familia?> GetWithPacientesAsync(int id)
        {
            return await _dbSet
                .Include(f => f.Pacientes)
                .FirstOrDefaultAsync(f => EF.Property<int>(f, "Id") == id);
        }

        public async Task<(IEnumerable<Familia> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search)
        {
            var query = _dbSet.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(f => (f.Responsable1Nombre ?? string.Empty).ToLower().Contains(s) || (f.Responsable1Apellido ?? string.Empty).ToLower().Contains(s));
            }

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }
    }
}
