using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Application.DTOs.Terapeuta;
using CentroTerapia.Domain.Exceptions;

namespace CentroTerapia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TerapeutasController : ControllerBase
    {
        private readonly ITerapeutaService _service;

        public TerapeutasController(ITerapeutaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TerapeutaDto>>> GetAll()
        {
            var q = HttpContext.Request.Query;
            int page = int.TryParse(q["page"], out var p) ? p : 1;
            int pageSize = int.TryParse(q["pageSize"], out var ps) ? ps : 20;
            var search = q.ContainsKey("search") ? q["search"].ToString() : null;
            bool? activo = null;
            if (q.ContainsKey("activo") && bool.TryParse(q["activo"], out var a)) activo = a;

            int? especialidadId = null;
            if (q.ContainsKey("especialidadId") && int.TryParse(q["especialidadId"].ToString(), out var eid)) especialidadId = eid;
            var (items, total) = await _service.GetPagedAsync(page, pageSize, search, activo, especialidadId);
            Response.Headers.Append("X-Total-Count", total.ToString());
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TerapeutaDto>> GetById(int id)
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

        [HttpPost]
        public async Task<ActionResult<TerapeutaDto>> Create([FromBody] CreateTerapeutaDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TerapeutaDto>> Update(int id, [FromBody] UpdateTerapeutaDto dto)
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
