using System;

namespace CentroTerapia.Domain.Entities
{
    public class FranjaExcepcion
    {
        public int Id { get; set; }
        public int FranjaId { get; set; }
        public FranjaDisponibilidad? Franja { get; set; }
        public DateTime Fecha { get; set; }
        public string? Motivo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
