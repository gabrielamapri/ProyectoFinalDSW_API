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
                .Where(p => EF.Property<int>(p, "FamiliaId") == familiaId)
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
    }
}
