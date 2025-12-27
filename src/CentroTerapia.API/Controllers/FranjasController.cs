using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Application.DTOs.Franja;
using CentroTerapia.Domain.Exceptions;

namespace CentroTerapia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Terapeuta,Padre")]
    public class FranjasController : ControllerBase
    {
        private readonly IFranjaService _service;

        public FranjasController(IFranjaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FranjaDisponibilidadDto>>> GetAll([FromQuery] string? search = null)
        {
            var items = await _service.GetAllAsync(search);
            return Ok(items);
        }

        // Endpoint seguro: devuelve solo las franjas del terapeuta autenticado
        [HttpGet("mis-franjas")]
        [Authorize(Roles = "Terapeuta")]
        public async Task<ActionResult<IEnumerable<FranjaDisponibilidadDto>>> GetOwnFranjas()
        {
            // Extraer el id del terapeuta desde el token JWT
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type.EndsWith("/nameidentifier") || c.Type.ToLower().Contains("id"));
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
                return Unauthorized(new { message = "No se pudo identificar el terapeuta en el token." });
            if (!int.TryParse(userIdClaim.Value, out int terapeutaId))
                return Unauthorized(new { message = "El id del terapeuta no es válido." });
            var items = await _service.GetByTerapeutaIdAsync(terapeutaId);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FranjaDisponibilidadDto>> GetById(int id)
        {
            try
            {
                var item = await _service.GetByIdAsync(id);
                return Ok(item);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("terapeuta/{terapeutaId}")]
        public async Task<ActionResult<IEnumerable<FranjaDisponibilidadDto>>> GetByTerapeuta(int terapeutaId)
        {
            var items = await _service.GetByTerapeutaIdAsync(terapeutaId);
            return Ok(items);
        }

        [HttpGet("{terapeutaId}/slots")]
        public async Task<ActionResult<IEnumerable<SlotDto>>> GetSlots(int terapeutaId, [FromQuery] DateTime date, [FromQuery] int duracion = 60)
        {
            if (duracion <= 0) duracion = 60;
            var slots = await _service.GetAvailableSlotsAsync(terapeutaId, date.Date, duracion);
            return Ok(slots);
        }

        [HttpGet("{terapeutaId}/available-dates")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetAvailableDates(int terapeutaId, [FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] int duracion = 60)
        {
            if (duracion <= 0) duracion = 60;
            if (end < start) return BadRequest(new { message = "'end' must be greater or equal to 'start'" });
            var dates = await _service.GetAvailableDatesAsync(terapeutaId, start.Date, end.Date, duracion);
            return Ok(dates);
        }

        [HttpPost]
        public async Task<ActionResult<FranjaDisponibilidadDto>> Create([FromBody] CreateFranjaDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (result.FutureAppointments != null && result.FutureAppointments.Count > 0)
                {
                    return Ok(new {
                        warning = "La franja fue eliminada, pero había citas futuras asociadas. Avisar a:",
                        affectedAppointments = result.FutureAppointments
                    });
                }
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

    }
}
