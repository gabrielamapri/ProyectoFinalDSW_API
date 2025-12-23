namespace CentroTerapia.Application.DTOs.Cita
{
    public class CitaDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public int PacienteId { get; set; }
        public string PacienteNombre { get; set; } = string.Empty;
        public bool PuedeSerCancelada { get; set; }
        public int? TipoSesionId { get; set; }
        public string TipoSesionNombre { get; set; } = string.Empty;
        public string EspecialidadNombre { get; set; } = string.Empty;
        public int? TerapeutaId { get; set; }
        public string TerapeutaNombre { get; set; } = string.Empty;
        public int DuracionMinutos { get; set; }
        public decimal? Precio { get; set; }
    }
}

