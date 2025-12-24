using System;

namespace CentroTerapia.Application.DTOs.Franja
{
    public class AddExcepcionesTerapeutaDto
    {
        public DateTime Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public string? Motivo { get; set; }
        public int? FranjaId { get; set; }
    }
}
