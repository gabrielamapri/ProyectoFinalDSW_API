using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Application.DTOs.Familia;
using CentroTerapia.Application.DTOs.Paciente;
using CentroTerapia.Domain.Exceptions;

namespace CentroTerapia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FamiliasController : ControllerBase
    {
        private readonly IFamiliaService _service;
        private readonly IPacienteService _pacienteService;

        public FamiliasController(IFamiliaService service, IPacienteService pacienteService)
        {
            _service = service;
            _pacienteService = pacienteService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FamiliaDto>>> GetAll()
        {
            var q = HttpContext.Request.Query;
            int page = int.TryParse(q["page"], out var p) ? p : 1;
            int pageSize = int.TryParse(q["pageSize"], out var ps) ? ps : 20;
            var search = q.ContainsKey("search") ? q["search"].ToString() : null;

            var (items, total) = await _service.GetPagedAsync(page, pageSize, search);
            Response.Headers.Append("X-Total-Count", total.ToString());
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FamiliaDto>> GetById(int id)
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
        public async Task<ActionResult<FamiliaDto>> Create([FromBody] CreateFamiliaDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPost("{familiaId}/pacientes")]
        public async Task<ActionResult> AddDependiente(int familiaId, [FromBody] CreatePacienteDto dto)
        {
            try
            {
                dto.FamiliaId = familiaId;
                var paciente = await _pacienteService.CreateAsync(dto);
                return Created($"/api/pacientes/{paciente.Id}", paciente);
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

        [HttpPut("{id}")]
        public async Task<ActionResult<FamiliaDto>> Update(int id, [FromBody] CreateFamiliaDto dto)
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
