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

        public new async Task<IEnumerable<Familia>> GetAllAsync()
        {
            return await _dbSet.Include(f => f.Pacientes).ToListAsync();
        }

        public async Task<Familia?> GetWithPacientesAsync(int id)
        {
            return await _dbSet
                .Include(f => f.Pacientes)
                .FirstOrDefaultAsync(f => EF.Property<int>(f, "Id") == id);
        }

        public async Task<(IEnumerable<Familia> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search)
        {
            // Incluir pacientes para que el DTO de familia tenga los dependientes en el listado
            var query = _dbSet
                .Include(f => f.Pacientes)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(f => 
                    (f.ResponsablePrincipalNombre ?? string.Empty).ToLower().Contains(s) || 
                    (f.ResponsablePrincipalApellido ?? string.Empty).ToLower().Contains(s) ||
                    (f.ResponsablePrincipalDNI ?? string.Empty).ToLower().Contains(s) ||
                    (f.Responsable2Nombre ?? string.Empty).ToLower().Contains(s) ||
                    (f.Responsable2Apellido ?? string.Empty).ToLower().Contains(s) ||
                    (f.Responsable2DNI ?? string.Empty).ToLower().Contains(s)
                );
            }

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }
    }
}
