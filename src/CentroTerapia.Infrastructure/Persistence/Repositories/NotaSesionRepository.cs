using Microsoft.EntityFrameworkCore;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Ports.Out;
using CentroTerapia.Infrastructure.Persistence.Context;

namespace CentroTerapia.Infrastructure.Persistence.Repositories
{
    public class NotaSesionRepository : Repository<NotaSesion>, INotaSesionRepository
    {
        public NotaSesionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<NotaSesion>> GetByCitaIdAsync(int citaId)
        {
            return await _dbSet
                .Where(n => n.CitaId == citaId)
                .Include(n => n.Terapeuta)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotaSesion>> GetAllAsync()
        {
            return await _dbSet
                .Include(n => n.Terapeuta)
                .Include(n => n.Cita)
                    .ThenInclude(c => c.Paciente)
                .ToListAsync();
        }

        public async Task<NotaSesion?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Where(n => n.Id == id)
                .Include(n => n.Terapeuta)
                .Include(n => n.Cita)
                    .ThenInclude(c => c.Paciente)
                .FirstOrDefaultAsync();
        }
    }
}
