namespace CentroTerapia.Application.DTOs.Reportes
{
    public class CitasProximasDto
    {
        public DateTime FechaConsulta { get; set; }
        public int DiasDesdHoy { get; set; }
        public List<CitaProximaDetalleDto> Citas { get; set; } = new();
    }

    public class CitaProximaDetalleDto
    {
        public int CitaId { get; set; }
        public DateTime Fecha { get; set; }
        public int HoraInicio { get; set; }
        public int MinutoInicio { get; set; }
        public string PacienteNombre { get; set; } = string.Empty;
        public int PacienteEdad { get; set; }
        public string Especialidad { get; set; } = string.Empty;
        public string TerapeutaNombre { get; set; } = string.Empty;
        public string ResponsableNombre { get; set; } = string.Empty;
        public string ResponsableTelefono { get; set; } = string.Empty;
        public string ResponsableEmail { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool Confirmada { get; set; } = false;
    }
}
