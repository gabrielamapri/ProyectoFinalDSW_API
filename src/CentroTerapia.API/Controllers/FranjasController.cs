using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Application.DTOs.Franja;
using CentroTerapia.Domain.Exceptions;

namespace CentroTerapia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FranjasController : ControllerBase
    {
        private readonly IFranjaService _service;

        public FranjasController(IFranjaService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<FranjaDisponibilidadDto>>> GetAll([FromQuery] string? search = null)
        {
            var items = await _service.GetAllAsync(search);
            return Ok(items);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
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
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<FranjaDisponibilidadDto>>> GetByTerapeuta(int terapeutaId)
        {
            var items = await _service.GetByTerapeutaIdAsync(terapeutaId);
            return Ok(items);
        }

        [HttpGet("{terapeutaId}/slots")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CentroTerapia.Application.DTOs.Franja.SlotDto>>> GetSlots(int terapeutaId, [FromQuery] DateTime date, [FromQuery] int duracion = 60)
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

        [HttpPut("{id}")]
        public async Task<ActionResult<FranjaDisponibilidadDto>> Update(int id, [FromBody] CreateFranjaDto dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                return Ok(updated);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

    }
}
