namespace CentroTerapia.Application.DTOs.Reportes
{
    public class CitaDetalleDto
    {
        public int CitaId { get; set; }
        public DateTime Fecha { get; set; }
        public int HoraInicio { get; set; }
        public int MinutoInicio { get; set; }
        public string PacienteNombre { get; set; } = "";
        public string Especialidad { get; set; } = "";
        public string TipoSesion { get; set; } = "";
        public string TerapeutaNombre { get; set; } = "";
        public string Estado { get; set; } = "";
        public string Motivo { get; set; } = "";
    }

    public class HistorialCitasDto
    {
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public int? TerapeutaId { get; set; }
        public int? EspecialidadId { get; set; }
        public int? TipoSesionId { get; set; }
        public string? EstadoFiltro { get; set; }
        public int TotalCitas { get; set; }
        public int Completadas { get; set; }
        public int Canceladas { get; set; }
        public int Programadas { get; set; }
        public List<CitaDetalleDto> Citas { get; set; } = new();
        public PaginationDto Pagination { get; set; } = new PaginationDto();
    }
}
