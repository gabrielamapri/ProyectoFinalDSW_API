using System;

namespace CentroTerapia.Application.DTOs.Cita
{
    public class CitaAlertaDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int PacienteId { get; set; }
        public string PacienteNombre { get; set; } = string.Empty;
        public string ResponsableNombre { get; set; } = string.Empty;
        public string ResponsableTelefono { get; set; } = string.Empty;
        public string ResponsableEmail { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int? TipoSesionId { get; set; }
        public string TipoSesionNombre { get; set; } = string.Empty;
    }
}
