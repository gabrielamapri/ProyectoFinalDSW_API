using CentroTerapia.Application.DTOs.Reportes;

namespace CentroTerapia.Application.Interfaces
{
    public interface IReporteService
    {
        // 1. Historial Clínico del Paciente
        Task<HistorialPacienteDto> GetHistorialPacienteAsync(int pacienteId, int page = 1, int pageSize = 10);

        // 1b. Exportar Historial a PDF
        Task<byte[]> ExportHistorialPacienteAsync(int pacienteId);

        // 2. Control de Asistencia
        Task<ControlAsistenciaDto> GetControlAsistenciaAsync(DateTime fechaInicio, DateTime fechaFin);

        // 3. Estado de Cuenta Familia
        Task<EstadoCuentaFamiliaDto> GetEstadoCuentaFamiliaAsync(int familiaId);

        // 4. Reporte de Progreso del Niño (para padres)
        Task<ReporteProgresoNinoDto> GetReporteProgresoNinoAsync(int pacienteId, int mes, int año);

        // 5. Citas Próximas a Confirmar
        Task<CitasProximasDto> GetCitasProximasAsync(DateTime fechaDesde, DateTime fechaHasta, int? especialidadId = null, int? terapeutaId = null, int? tipoSesionId = null);
        Task<byte[]> ExportCitasProximasAsync(DateTime fechaDesde, DateTime fechaHasta, int? especialidadId = null, int? terapeutaId = null, int? tipoSesionId = null);
    }
}
