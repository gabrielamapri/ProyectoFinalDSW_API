namespace CentroTerapia.Application.DTOs.Reportes
{
    public class HistorialPacienteDto
    {
        public int PacienteId { get; set; }
        public string PacienteNombre { get; set; } = string.Empty;
        public string PacienteDNI { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string ResponsableNombre { get; set; } = string.Empty;
        public string ResponsableDNI { get; set; } = string.Empty;
        public string ResponsableTelefono { get; set; } = string.Empty;
        public string ResponsableEmail { get; set; } = string.Empty;
        public int TotalCitas { get; set; }
        public int CitasCompletadas { get; set; }
        public int CitasCanceladas { get; set; }
        public int CitasProgramadas { get; set; }
        public List<CitaHistorialDto> Citas { get; set; } = new();
        public List<NotaHistorialDto> NotasRecientes { get; set; } = new();
        public PaginationDto? Pagination { get; set; }
    }

    public class CitaHistorialDto
    {
        public int CitaId { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string TerapeutaNombre { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public string TipoSesion { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
    }

    public class NotaHistorialDto
    {
        public int NotaId { get; set; }
        public DateTime CitaFecha { get; set; }
        public string Notas { get; set; } = string.Empty;
        public string TerapeutaNombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }
}
