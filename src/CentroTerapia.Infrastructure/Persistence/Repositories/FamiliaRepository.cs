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
    }
}
