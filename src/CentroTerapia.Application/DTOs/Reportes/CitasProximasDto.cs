namespace CentroTerapia.Application.DTOs.Reportes
{
    public class CitasProximasDto
    {
        public DateTime FechaConsulta { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public int TotalCitas { get; set; }
        public List<CitaProximaDetalleDto> Citas { get; set; } = new();
    }

    public class CitaProximaDetalleDto
    {
        public int CitaId { get; set; }
        public DateTime Fecha { get; set; }
        public int HoraInicio { get; set; }
        public int MinutoInicio { get; set; }
        public string PacienteNombre { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public string TipoSesion { get; set; } = string.Empty;
        public string TerapeutaNombre { get; set; } = string.Empty;
        public string ResponsableNombre { get; set; } = string.Empty;
        public string ResponsableTelefono { get; set; } = string.Empty;
    }
}
