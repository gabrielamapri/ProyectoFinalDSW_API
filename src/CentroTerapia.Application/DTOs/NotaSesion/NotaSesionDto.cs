namespace CentroTerapia.Application.DTOs.NotaSesion
{
    public class NotaSesionDto
    {
        public int Id { get; set; }
        public int CitaId { get; set; }
        public DateTime? CitaFecha { get; set; }
        public int TerapeutaId { get; set; }
        public string TerapeutaNombre { get; set; } = string.Empty;
        public string PacienteNombre { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }
}
