using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Application.DTOs.Franja;
using CentroTerapia.Domain.Exceptions;

namespace CentroTerapia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Terapeuta")]
    public class FranjaExcepcionesController : ControllerBase
    {
        private readonly IFranjaService _service;

        public FranjaExcepcionesController(IFranjaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FranjaExcepcionDetalleDto>>> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] int? terapeutaId = null,
            [FromQuery] int? franjaId = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] string? search = null)
        {
            var (items, total) = await _service.GetExcepcionesAsync(page, pageSize, terapeutaId, franjaId, from, to, search);
            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(items);
        }

        [HttpPost("terapeuta/{terapeutaId}")]
        public async Task<ActionResult<IEnumerable<FranjaExcepcionDetalleDto>>> CreateForTerapeuta(int terapeutaId, [FromBody] AddExcepcionesTerapeutaDto dto)
        {
            try
            {
                var items = await _service.AddExceptionsByTerapeutaAsync(terapeutaId, dto);
                return Ok(items);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var ok = await _service.RemoveExceptionByIdAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
