using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Application.DTOs.Paciente;
using CentroTerapia.Domain.Exceptions;

namespace CentroTerapia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PacientesController: ControllerBase
    {
        private readonly IPacienteService _pacienteService;

            public PacientesController(IPacienteService pacienteService)
            {
                _pacienteService = pacienteService;
            }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PacienteDto>>> GetAll()
        {
            // support paging and filtering
            var q = HttpContext.Request.Query;
            int page = int.TryParse(q["page"], out var p) ? p : 1;
            int pageSize = int.TryParse(q["pageSize"], out var ps) ? ps : 20;
            var search = q.ContainsKey("search") ? q["search"].ToString() : null;
            var sexo = q.ContainsKey("sexo") ? q["sexo"].ToString() : null;

            var (items, total) = await _pacienteService.GetPagedAsync(page, pageSize, search, sexo);
            Response.Headers.Append("X-Total-Count", total.ToString());
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PacienteDto>> GetById(int id)
        {
            var paciente = await _pacienteService.GetByIdAsync(id);
            return Ok(paciente);
        }

        [HttpGet("familia/{familiaId}")]
        public async Task<ActionResult<IEnumerable<PacienteDto>>> GetByFamiliaId(int familiaId)
        {
            try
            {
                var pacientes = await _pacienteService.GetByFamiliaIdAsync(familiaId);
                return Ok(pacientes);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<PacienteDto>> Create([FromBody] CentroTerapia.Application.DTOs.Paciente.CreatePacienteDto dto)
        {
            try
            {
                var paciente = await _pacienteService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = paciente.Id }, paciente);
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
        public async Task<ActionResult<PacienteDto>> Update(int id, [FromBody] CentroTerapia.Application.DTOs.Paciente.UpdatePacienteDto dto)
        {
            try
            {
                var paciente = await _pacienteService.UpdateAsync(id, dto);
                return Ok(paciente);
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

        [HttpPost("{id}/familia/{familiaId}")]
        public async Task<ActionResult<PacienteDto>> AssignFamily(int id, int familiaId)
        {
            try
            {
                var paciente = await _pacienteService.AssignFamilyAsync(id, familiaId);
                return Ok(paciente);
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _pacienteService.DeleteAsync(id);
                return NoContent();
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
    }
}

