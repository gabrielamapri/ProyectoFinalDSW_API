namespace CentroTerapia.Application.DTOs.Cita
{
    public class CreateCitaDto
    {
        public DateTime Fecha { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public int PacienteId { get; set; }
        public int? TerapeutaId { get; set; }
        public int? TerapiaId { get; set; }
        public int? DuracionMinutos { get; set; }
    }
}

