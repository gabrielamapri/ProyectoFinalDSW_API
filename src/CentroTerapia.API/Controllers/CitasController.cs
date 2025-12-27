using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Application.DTOs.Cita;
using CentroTerapia.Application.DTOs.NotaSesion;
using CentroTerapia.Domain.Exceptions;
using AutoMapper;
using System.Security.Claims;

namespace CentroTerapia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class CitasController : ControllerBase
    {
        private readonly ICitaService _appointmentService;
        private readonly IMapper _mapper;
        private readonly IFamiliaService _familiaService;
        private readonly INotaSesionService _notaSesionService;

        public CitasController(ICitaService appointmentService, IMapper mapper, IFamiliaService familiaService, INotaSesionService notaSesionService)
        {
            _appointmentService = appointmentService;
            _mapper = mapper;
            _familiaService = familiaService;
            _notaSesionService = notaSesionService;
        }

        /// <summary>
        /// Marca una cita como Completada y crea la nota de sesión (solo Admin y Terapeuta)
        /// </summary>
        [Authorize(Roles = "Admin,Terapeuta")]
        [HttpPatch("{id}/completar")]
        public async Task<ActionResult> CompletarCita(int id, [FromBody] CreateNotaSesionDto notaDto)
        {
            if (string.IsNullOrWhiteSpace(notaDto.Notas))
                return BadRequest(new { message = "La nota de sesión es obligatoria." });

            // Validar que la cita existe
            var cita = await _appointmentService.GetByIdAsync(id);
            if (cita == null) return NotFound(new { message = "Cita no encontrada." });

            // Actualizar estado a Completado
            var updateDto = new UpdateCitaDto { Estado = "Completado" };
            await _appointmentService.UpdateAsync(id, updateDto);

            // Crear nota de sesión
            notaDto.CitaId = id;
            if (notaDto.TerapeutaId == 0 && cita.TerapeutaId.HasValue)
                notaDto.TerapeutaId = cita.TerapeutaId.Value;
            await _notaSesionService.CreateAsync(notaDto);

            return Ok(new { message = "Cita completada y nota registrada." });
        }

        [Authorize(Roles = "Admin,Terapeuta")]
        [HttpPatch("{id}/noasistio")]
        public async Task<ActionResult> MarcarNoAsistio(int id)
        {
            try
            {
                await _appointmentService.MarcarNoAsistioAsync(id);
                return Ok(new { message = "Cita marcada como No Asistió" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Terapeuta")]
        [HttpGet("terapeuta/{terapeutaId}/rango-fechas")]
        public async Task<ActionResult<IEnumerable<CitaAlertaDto>>> GetByTerapeutaAndDateRange(int terapeutaId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (User.IsInRole("Terapeuta"))
            {
                var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out int idFromToken))
                {
                    return Forbid("No se pudo identificar el terapeuta en el token.");
                }
                terapeutaId = idFromToken;
            }
            var citas = await _appointmentService.GetByTerapeutaAndDateRangeAsync(terapeutaId, startDate, endDate);
            return Ok(citas);
        }

        [Authorize(Roles = "Padre")]
        [HttpGet("mis-citas")]
        public async Task<ActionResult<IEnumerable<CitaDto>>> GetCitasDeMisHijos([FromQuery] string? search = null)
        {
            var familiaIdClaim = User.FindFirst("FamiliaId")?.Value ?? User.FindFirst("familiaId")?.Value;
            int familiaId = 0;
            if (!string.IsNullOrEmpty(familiaIdClaim))
                int.TryParse(familiaIdClaim, out familiaId);
            List<int> pacienteIds = new List<int>();
            if (familiaId > 0)
            {
                var familia = await _familiaService.GetByIdAsync(familiaId);
                if (familia != null)
                {
                    pacienteIds = familia.Pacientes?.Select(p => p.Id).ToList() ?? new List<int>();
                }
            }
            else
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(email))
                {
                    var familias = await _familiaService.GetAllAsync(email, "Padre");
                    var familia = familias.FirstOrDefault();
                    if (familia != null)
                    {
                        pacienteIds = familia.Pacientes?.Select(p => p.Id).ToList() ?? new List<int>();
                    }
                }
            }
            if (pacienteIds.Count == 0)
                return Ok(new List<CitaDto>());
            var citas = await _appointmentService.GetByPacienteIdsAsync(pacienteIds, search);
            return Ok(citas);
        }

        [Authorize(Roles = "Admin,Terapeuta")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CitaDto>>> GetAll([FromQuery] string? search = null)
        {
            var citas = await _appointmentService.GetAllAsync(search);
            return Ok(citas);
        }

        [Authorize(Roles = "Admin,Terapeuta")]
        [HttpGet("terapeuta/{terapeutaId}")]
        public async Task<ActionResult<IEnumerable<CitaDto>>> GetByTerapeutaId(int terapeutaId)
        {
            if (User.IsInRole("Terapeuta"))
            {
                var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out int idFromToken))
                {
                    return Forbid("No se pudo identificar el terapeuta en el token.");
                }
                terapeutaId = idFromToken;
            }
            var citas = await _appointmentService.GetByTerapeutaIdAsync(terapeutaId);
            return Ok(citas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CitaDto>> GetById(int id)
        {
            var cita = await _appointmentService.GetByIdAsync(id);
            if (cita == null) return NotFound();
            return Ok(cita);
        }

        [Authorize(Roles = "Admin,Terapeuta")]
        [HttpGet("status/{estado}")]
        public async Task<ActionResult<IEnumerable<CitaDto>>> GetByEstado(string estado)
        {
            var citas = await _appointmentService.GetByStatusAsync(estado);
            return Ok(citas);
        }

        [HttpPost]
        public async Task<ActionResult<CitaDto>> Create([FromBody] CreateCitaDto dto)
        {
            try
            {
                var citaDto = await _appointmentService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = citaDto.Id }, citaDto);
            }
            catch (Exception ex) when (ex is NotFoundException || ex is BusinessRuleException)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/cancelar")]
        public async Task<ActionResult> Cancelar(int id)
        {
            try 
            {
                await _appointmentService.CancelAsync(id);
                return Ok(new { message = "Cita cancelada con éxito" });
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/reprogramar")]
        public async Task<ActionResult<CitaDto>> Reprogramar(int id, [FromBody] ReprogramCitaDto dto)
        {
            try
            {
                var citaDto = await _appointmentService.ReprogramAsync(id, dto);
                return Ok(citaDto);
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
