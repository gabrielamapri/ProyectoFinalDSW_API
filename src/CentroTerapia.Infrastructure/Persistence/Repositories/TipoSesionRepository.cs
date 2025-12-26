using Microsoft.EntityFrameworkCore;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Ports.Out;
using CentroTerapia.Infrastructure.Persistence.Context;

namespace CentroTerapia.Infrastructure.Persistence.Repositories
{
    public class TipoSesionRepository : Repository<TipoSesion>, ITipoSesionRepository
    {
        // Usamos _context de la clase base si es accesible, 
        // o lo declaramos aquí si es privado en la base.
        private readonly ApplicationDbContext _db;

        public TipoSesionRepository(ApplicationDbContext context) : base(context)
        {
            _db = context;
        }

        // 1. Quitamos el 'override' porque tu clase base 'Repository' no tiene los métodos como virtual.
        // 2. Usamos 'new' para ocultar el método base, o simplemente nos aseguramos de que 
        // el servicio llame a la implementación de la interfaz ITipoSesionRepository.
        
        public new async Task<IEnumerable<TipoSesion>> GetAllAsync()
        {
            // Nota: He cambiado 'TipoSesiones' por 'Set<TipoSesion>()' 
            // para que funcione sin importar cómo se llame la propiedad en el DbContext.
            return await _db.Set<TipoSesion>()
                .Include(t => t.Especialidad)
                .ToListAsync();
        }

        public new async Task<TipoSesion?> GetByIdAsync(int id)
        {
            return await _db.Set<TipoSesion>()
                .Include(t => t.Especialidad)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}