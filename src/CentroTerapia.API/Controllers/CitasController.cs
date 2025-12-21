using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Application.DTOs.Cita;
using CentroTerapia.Domain.Exceptions;
using AutoMapper;

namespace CentroTerapia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitasController : ControllerBase
    {
        private readonly ICitaService _appointmentService;
        private readonly IMapper _mapper;

        public CitasController(ICitaService appointmentService, IMapper mapper)
        {
            _appointmentService = appointmentService;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CitaDto>>> GetAll()
        {
            var Citas = await _appointmentService.GetAllAsync();
            var citas = _mapper.Map<IEnumerable<CitaDto>>(Citas);
            return Ok(citas);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<CitaDto>> GetById(int id)
        {
            var Cita = await _appointmentService.GetByIdAsync(id);
            var cita = _mapper.Map<CitaDto>(Cita);
            return Ok(cita);
        }

        [AllowAnonymous]
        [HttpGet("paciente/{pacienteId}")]
        public async Task<ActionResult<IEnumerable<CitaDto>>> GetByPacienteId(int pacienteId)
        {
            try
            {
                var Citas = await _appointmentService.GetByPacienteIdAsync(pacienteId);
                var citas = _mapper.Map<IEnumerable<CitaDto>>(Citas);
                return Ok(citas);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("status/{estado}")]
        public async Task<ActionResult<IEnumerable<CitaDto>>> GetByEstado(string estado)
        {
            try
            {
                var Citas = await _appointmentService.GetByStatusAsync(estado);
                var citas = _mapper.Map<IEnumerable<CitaDto>>(Citas);
                return Ok(citas);
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("rango-fechas")]
        public async Task<ActionResult<IEnumerable<CitaDto>>> GetByFechaRango([FromQuery] DateTime inicio, [FromQuery] DateTime fin)
        {
            try
            {
                var Citas = await _appointmentService.GetByDateRangeAsync(inicio, fin);
                var citas = _mapper.Map<IEnumerable<CitaDto>>(Citas);
                return Ok(citas);
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<CitaDto>> Create([FromBody] CreateCitaDto dto)
        {
            try
            {
            var Cita = await _appointmentService.CreateAsync(_mapper.Map<CreateCitaDto>(dto));
                var cita = _mapper.Map<CitaDto>(Cita);
                return CreatedAtAction(nameof(GetById), new { id = cita.Id }, cita);
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
        public async Task<ActionResult<CitaDto>> Update(int id, [FromBody] UpdateCitaDto dto)
        {
            try
            {
                var Cita = await _appointmentService.UpdateAsync(id, _mapper.Map<UpdateCitaDto>(dto));
                var cita = _mapper.Map<CitaDto>(Cita);
                return Ok(cita);
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

        [HttpPatch("{id}/cancelar")]
        public async Task<ActionResult> Cancelar(int id)
        {
            try
            {
                await _appointmentService.CancelAsync(id);
                return Ok(new { message = "Cita cancelada" });
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
                await _appointmentService.DeleteAsync(id);
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


