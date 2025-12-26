using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Application.DTOs.Cita;
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

        private readonly ICitaService _appointmentService;
        private readonly IMapper _mapper;
        private readonly IFamiliaService _familiaService;

        public CitasController(ICitaService appointmentService, IMapper mapper, IFamiliaService familiaService)
        {
            _appointmentService = appointmentService;
            _mapper = mapper;
            _familiaService = familiaService;
        }

        [Authorize(Roles = "Admin,Terapeuta")]
        [HttpGet("terapeuta/{terapeutaId}/rango-fechas")]
        public async Task<ActionResult<IEnumerable<CitaAlertaDto>>> GetByTerapeutaAndDateRange(int terapeutaId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            // Si el usuario es Terapeuta, forzar el id desde el claim NameIdentifier
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
            // 1. Intentar sacar el ID de la Familia del Token
            var familiaIdClaim = User.FindFirst("FamiliaId")?.Value ?? User.FindFirst("familiaId")?.Value;
            int familiaId = 0;
            
            if (!string.IsNullOrEmpty(familiaIdClaim))
                int.TryParse(familiaIdClaim, out familiaId);

            List<int> pacienteIds = new List<int>();

            // 2. Si tenemos el ID, buscamos los pacientes de esa familia
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
                // Respaldo por Email si el FamiliaId no está en el token
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

            // LLAMADA CORREGIDA: Se pasan los IDs de pacientes y el término de búsqueda
            var citas = await _appointmentService.GetByPacienteIdsAsync(pacienteIds, search);
            
            return Ok(citas);
        }

        // --- ENDPOINTS PARA ADMINISTRACIÓN / TERAPEUTAS ---

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
            // Si el usuario es Terapeuta, forzar el id desde el claim NameIdentifier
            if (User.IsInRole("Terapeuta"))
            {
                var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out int idFromToken))
                {
                    return Forbid("No se pudo identificar el terapeuta en el token.");
                }
                terapeutaId = idFromToken;
            }
            // Admin puede consultar cualquier terapeuta
            var citas = await _appointmentService.GetByTerapeutaIdAsync(terapeutaId);
            return Ok(citas);
        }

        // --- ENDPOINTS COMPARTIDOS CON SEGURIDAD ---

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
