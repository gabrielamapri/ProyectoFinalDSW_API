using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Application.DTOs.Reportes;
using CentroTerapia.Domain.Exceptions;

namespace CentroTerapia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Cambiar a [Authorize] cuando mejores la autenticación
    public class ReportesController : ControllerBase
    {
        private readonly IReporteService _reporteService;

        public ReportesController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        /// <summary>
        /// 1. Obtiene el historial clínico completo de un paciente
        /// </summary>
        [HttpGet("historial-paciente/{pacienteId}")]
        public async Task<ActionResult<HistorialPacienteDto>> GetHistorialPaciente(
            int pacienteId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var reporte = await _reporteService.GetHistorialPacienteAsync(pacienteId, page, pageSize);
                return Ok(reporte);
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

        /// <summary>
        /// Exporta el historial del paciente a PDF
        /// </summary>
        [HttpGet("historial-paciente/{pacienteId}/export-pdf")]
        public async Task<IActionResult> ExportHistorialPacientePdf(int pacienteId)
        {
            try
            {
                var pdfBytes = await _reporteService.ExportHistorialPacienteAsync(pacienteId);
                return File(pdfBytes, "application/pdf", $"Historial_Paciente_{pacienteId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
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

        /// <summary>
        /// 2. Obtiene citas próximas a confirmar en un rango de fechas
        /// </summary>
        [HttpGet("citas-proximas")]
        public async Task<ActionResult<CitasProximasDto>> GetCitasProximas(
            [FromQuery] DateTime fechaDesde,
            [FromQuery] DateTime fechaHasta,
            [FromQuery] int? especialidadId = null,
            [FromQuery] int? terapeutaId = null,
            [FromQuery] int? tipoSesionId = null)
        {
            try
            {
                var reporte = await _reporteService.GetCitasProximasAsync(fechaDesde, fechaHasta, especialidadId, terapeutaId, tipoSesionId);
                return Ok(reporte);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 3. Exporta citas próximas a PDF
        /// </summary>
        [HttpGet("citas-proximas/export-pdf")]
        public async Task<IActionResult> ExportCitasProximasPdf(
            [FromQuery] DateTime fechaDesde,
            [FromQuery] DateTime fechaHasta,
            [FromQuery] int? especialidadId = null,
            [FromQuery] int? terapeutaId = null,
            [FromQuery] int? tipoSesionId = null)
        {
            try
            {
                var pdf = await _reporteService.ExportCitasProximasAsync(fechaDesde, fechaHasta, especialidadId, terapeutaId, tipoSesionId);
                var fileName = $"CitasProximas_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                return File(pdf, "application/pdf", fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
