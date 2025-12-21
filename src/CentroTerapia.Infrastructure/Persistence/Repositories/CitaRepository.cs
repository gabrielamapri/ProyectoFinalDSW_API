using Microsoft.EntityFrameworkCore;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Ports.Out;
using CentroTerapia.Infrastructure.Persistence.Context;

namespace CentroTerapia.Infrastructure.Persistence.Repositories
{
    public class CitaRepository : Repository<Cita>, ICitaRepository
    {
        public CitaRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Cita>> GetByPacienteIdAsync(int pacienteId)
        {
            return await _dbSet
                .Where(a => a.PacienteId == pacienteId)
                .Include(a => a.Paciente)
                    .ThenInclude(p => p!.Familia)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cita>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(a => a.Fecha >= startDate && a.Fecha <= endDate)
                .Include(a => a.Paciente)
                    .ThenInclude(p => p!.Familia)
                .OrderBy(a => a.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cita>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Where(a => a.Estado == status)
                .Include(a => a.Paciente)
                    .ThenInclude(p => p!.Familia)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();
        }

        public async Task<Cita?> GetWithPacienteAndFamiliaAsync(int id)
        {
            return await _dbSet
                .Include(a => a.Paciente)
                    .ThenInclude(p => p!.Familia)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Cita>> GetAllWithRelationsAsync()
        {
            return await _dbSet
                .Include(a => a.Paciente)
                    .ThenInclude(p => p!.Familia)
                .Include(a => a.TipoSesion)
                .Include(a => a.Terapeuta)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();
        }
    }
}

