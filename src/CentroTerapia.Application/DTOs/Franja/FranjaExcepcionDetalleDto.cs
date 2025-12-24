using System;

namespace CentroTerapia.Application.DTOs.Franja
{
    public class FranjaExcepcionDetalleDto
    {
        public int Id { get; set; }
        public int FranjaId { get; set; }
        public int TerapeutaId { get; set; }
        public string? TerapeutaNombre { get; set; }
        public DateTime? FechaFranja { get; set; }
        public int? DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public bool Recurrente { get; set; }
        public DateTime Fecha { get; set; }
        public string? Motivo { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
