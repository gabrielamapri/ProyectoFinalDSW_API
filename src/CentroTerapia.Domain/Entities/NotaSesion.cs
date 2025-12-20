namespace CentroTerapia.Domain.Entities
{
    public class NotaSesion
    {
        public int Id { get; set; }
        public int CitaId { get; set; }
        public Cita? Cita { get; set; }
        public int TerapeutaId { get; set; }
        public Terapeuta? Terapeuta { get; set; }
        public string Notas { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}

