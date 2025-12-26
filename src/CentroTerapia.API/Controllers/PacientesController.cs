using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Application.DTOs.Paciente;
using CentroTerapia.Domain.Exceptions;
using System.Security.Claims;

namespace CentroTerapia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Todos deben estar logueados
    public class PacientesController : ControllerBase
    {
        private readonly IPacienteService _pacienteService;

        public PacientesController(IPacienteService pacienteService)
        {
            _pacienteService = pacienteService;
        }

        // Solo Admin puede ver la lista global de pacientes
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PacienteDto>>> GetAll()
        {
            var q = HttpContext.Request.Query;
            int page = int.TryParse(q["page"], out var p) ? p : 1;
            int pageSize = int.TryParse(q["pageSize"], out var ps) ? ps : 20;
            var search = q.ContainsKey("search") ? q["search"].ToString() : null;
            var sexo = q.ContainsKey("sexo") ? q["sexo"].ToString() : null;

            var (items, total) = await _pacienteService.GetPagedAsync(page, pageSize, search, sexo);
            Response.Headers.Append("X-Total-Count", total.ToString());
            return Ok(items);
        }

        // Endpoint CRÍTICO para que el Padre llene su combo de "Nueva Cita"
        [Authorize(Roles = "Padre")]
        [HttpGet("mis-hijos")]
        public async Task<ActionResult<IEnumerable<PacienteDto>>> GetMisHijos()
        {
            // Extraemos el FamiliaId del Token
            var familiaIdClaim = User.FindFirst("FamiliaId")?.Value ?? User.FindFirst("familiaId")?.Value;
            
            if (string.IsNullOrEmpty(familiaIdClaim) || !int.TryParse(familiaIdClaim, out int familiaId))
            {
                return BadRequest(new { message = "No se pudo identificar la familia en el token." });
            }

            var pacientes = await _pacienteService.GetByFamiliaIdAsync(familiaId);
            return Ok(pacientes);
        }

        [Authorize(Roles = "Admin,Terapeuta,Padre")]
        [HttpGet("{id}")]
        public async Task<ActionResult<PacienteDto>> GetById(int id)
        {
            var paciente = await _pacienteService.GetByIdAsync(id);
            
            // Seguridad extra: Si es Padre, validar que el paciente sea suyo
            if (User.IsInRole("Padre"))
            {
                var familiaIdClaim = User.FindFirst("FamiliaId")?.Value ?? User.FindFirst("familiaId")?.Value;
                if (paciente.FamiliaId.ToString() != familiaIdClaim)
                    return Forbid();
            }

            return Ok(paciente);
        }

        // --- ACCIONES EXCLUSIVAS DE ADMIN ---

        [Authorize(Roles = "Admin")]
        [HttpPost]

        [Authorize(Roles = "Admin,Terapeuta,Padre")]
        [HttpPost]
        public async Task<ActionResult<PacienteDto>> Create([FromBody] CreatePacienteDto dto)
        {
            // Si el usuario es Padre, forzar el FamiliaId desde el token
            if (User.IsInRole("Padre"))
            {
                var familiaIdClaim = User.FindFirst("FamiliaId")?.Value ?? User.FindFirst("familiaId")?.Value;
                if (string.IsNullOrEmpty(familiaIdClaim) || !int.TryParse(familiaIdClaim, out int familiaId))
                {
                    return BadRequest(new { message = "No se pudo identificar la familia en el token." });
                }
                dto.FamiliaId = familiaId;
            }
            try {
                var paciente = await _pacienteService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = paciente.Id }, paciente);
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<PacienteDto>> Update(int id, [FromBody] UpdatePacienteDto dto)
        {
            try {
                var paciente = await _pacienteService.UpdateAsync(id, dto);
                return Ok(paciente);
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _pacienteService.DeleteAsync(id);
            return NoContent();
        }
    }
}

