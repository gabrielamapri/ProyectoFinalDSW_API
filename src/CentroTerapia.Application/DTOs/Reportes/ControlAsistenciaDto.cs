namespace CentroTerapia.Application.DTOs.Reportes
{
    public class ControlAsistenciaDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalCitasProgramadas { get; set; }
        public int TotalAsistencias { get; set; }
        public int TotalCancelaciones { get; set; }
        public int TotalInasistencias { get; set; }
        public decimal PorcentajeAsistencia { get; set; }
        public List<AsistenciaPacienteDto> Pacientes { get; set; } = new();
    }

    public class AsistenciaPacienteDto
    {
        public int PacienteId { get; set; }
        public string PacienteNombre { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string Especialidad { get; set; } = string.Empty;
        public int TotalCitas { get; set; }
        public int Asistencias { get; set; }
        public int Cancelaciones { get; set; }
        public int Inasistencias { get; set; }
        public decimal PorcentajeAsistencia { get; set; }
        public List<DetalleAsistenciaDto> Detalles { get; set; } = new();
    }

    public class DetalleAsistenciaDto
    {
        public int CitaId { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Motivo { get; set; }
    }
}
