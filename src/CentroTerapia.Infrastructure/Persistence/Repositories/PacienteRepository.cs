using Microsoft.EntityFrameworkCore;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Ports.Out;
using CentroTerapia.Infrastructure.Persistence.Context;

namespace CentroTerapia.Infrastructure.Persistence.Repositories
{
    public class PacienteRepository : Repository<Paciente>, IPacienteRepository
    {
        public PacienteRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Paciente>> GetByFamiliaIdAsync(int familiaId)
        {
            return await _dbSet
                .Where(p => p.FamiliaId.HasValue && p.FamiliaId.Value == familiaId)
                .Include(p => p.Familia)
                .ToListAsync();
        }

        public async Task<Paciente?> GetWithFamiliaAndCitasAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Familia)
                .Include(p => p.Citas)
                .FirstOrDefaultAsync(p => EF.Property<int>(p, "Id") == id);
        }

        public async Task<(IEnumerable<Paciente> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search, CentroTerapia.Domain.Enums.Sexo? sexo)
        {
            var query = _dbSet.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p => p.Nombres.ToLower().Contains(s) || p.Apellidos.ToLower().Contains(s));
            }

            if (sexo.HasValue)
            {
                query = query.Where(p => p.Sexo == sexo.Value);
            }

            var total = await query.CountAsync();
            var items = await query
                .Include(p => p.Familia)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
