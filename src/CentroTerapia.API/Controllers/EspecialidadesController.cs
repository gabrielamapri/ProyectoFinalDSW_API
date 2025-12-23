using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CentroTerapia.Infrastructure.Persistence.Context;

namespace CentroTerapia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EspecialidadesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public EspecialidadesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] string? search = null)
        {
            var query = _db.Especialidades.AsNoTracking();
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower().Trim();
                query = query.Where(e => e.Nombre.ToLower().Contains(searchLower));
            }
            
            var items = await query
                .Select(e => new { e.Id, e.Nombre, e.Descripcion })
                .ToListAsync();
            return Ok(items);
        }
    }
}
