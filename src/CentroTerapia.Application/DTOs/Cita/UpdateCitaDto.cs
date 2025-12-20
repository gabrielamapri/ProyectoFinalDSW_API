namespace CentroTerapia.Application.DTOs.Cita
{
    public class UpdateCitaDto
    {
        public DateTime Fecha { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public int? TerapeutaId { get; set; }
        public int? TipoSesionId { get; set; }
        public int? DuracionMinutos { get; set; }
    }
}

