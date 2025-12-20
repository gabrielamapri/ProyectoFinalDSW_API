namespace CentroTerapia.Application.DTOs.Franja
{
    public class CreateFranjaDto
    {
        public int TerapeutaId { get; set; }
        public DateTime? Fecha { get; set; }
        public int? DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public bool Recurrente { get; set; } = true;
    }
}
