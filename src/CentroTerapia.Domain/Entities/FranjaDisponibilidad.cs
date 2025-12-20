namespace CentroTerapia.Domain.Entities
{
    public class FranjaDisponibilidad
    {
        public int Id { get; set; }
        public int TerapeutaId { get; set; }
        public Terapeuta? Terapeuta { get; set; }
        public DateTime? Fecha { get; set; }
        public int? DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public bool Recurrente { get; set; } = true;
    }
}
