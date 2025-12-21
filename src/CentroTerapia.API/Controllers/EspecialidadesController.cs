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
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var items = await _db.Especialidades
                .AsNoTracking()
                .Select(e => new { e.Id, e.Nombre })
                .ToListAsync();
            return Ok(items);
        }
    }
}
