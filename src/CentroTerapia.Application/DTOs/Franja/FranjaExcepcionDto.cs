using System;

namespace CentroTerapia.Application.DTOs.Franja
{
    public class FranjaExcepcionDto
    {
        public int Id { get; set; }
        public int FranjaId { get; set; }
        public DateTime Fecha { get; set; }
        public string? Motivo { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
