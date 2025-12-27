using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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

        // Crear especialidad
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([FromBody] EspecialidadDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { message = "El nombre es obligatorio." });
            var entity = new CentroTerapia.Domain.Entities.Especialidad
            {
                Nombre = dto.Nombre ?? string.Empty,
                Descripcion = dto.Descripcion
            };
            _db.Especialidades.Add(entity);
            await _db.SaveChangesAsync();
            return Ok(new { entity.Id, entity.Nombre, entity.Descripcion });
        }

        // Editar especialidad
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(int id, [FromBody] EspecialidadDto dto)
        {
            var entity = await _db.Especialidades.FindAsync(id);
            if (entity == null) return NotFound(new { message = "No encontrada" });
            if (!string.IsNullOrWhiteSpace(dto.Nombre)) entity.Nombre = dto.Nombre;
            entity.Descripcion = dto.Descripcion;
            await _db.SaveChangesAsync();
            return Ok(new { entity.Id, entity.Nombre, entity.Descripcion });
        }

        // Eliminar especialidad
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var entity = await _db.Especialidades.FindAsync(id);
            if (entity == null) return NotFound(new { message = "No encontrada" });
            _db.Especialidades.Remove(entity);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Puede ser por terapeutas o tipos de sesión asociados
                return Conflict(new { message = "No se puede eliminar. Tiene terapeutas o tipos de sesión asociados." });
            }
            return NoContent();
        }

        // DTO interno para recibir datos
        public class EspecialidadDto
        {
            public int? Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
        }
    }
}