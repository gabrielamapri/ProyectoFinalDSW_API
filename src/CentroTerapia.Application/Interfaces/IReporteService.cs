using CentroTerapia.Application.DTOs.Reportes;

namespace CentroTerapia.Application.Interfaces
{
    public interface IReporteService
    {
        // 1. Historial Clínico del Paciente
        Task<HistorialPacienteDto> GetHistorialPacienteAsync(int pacienteId, int page = 1, int pageSize = 10);

        // 1b. Exportar Historial a PDF
        Task<byte[]> ExportHistorialPacienteAsync(int pacienteId);

        // 2. Citas Próximas a Confirmar
        Task<CitasProximasDto> GetCitasProximasAsync(DateTime fechaDesde, DateTime fechaHasta, int? especialidadId = null, int? terapeutaId = null, int? tipoSesionId = null);
        Task<byte[]> ExportCitasProximasAsync(DateTime fechaDesde, DateTime fechaHasta, int? especialidadId = null, int? terapeutaId = null, int? tipoSesionId = null);
    }
}
