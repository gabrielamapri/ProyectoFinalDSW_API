namespace CentroTerapia.Application.DTOs.Reportes
{
    public class ReporteProgresoNinoDto
    {
        public int PacienteId { get; set; }
        public string PacienteNombre { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string Especialidad { get; set; } = string.Empty;
        public string TerapeutaNombre { get; set; } = string.Empty;
        public DateTime MesReporte { get; set; }
        public int TotalSesiones { get; set; }
        public int SesionesAsistidas { get; set; }
        public int SesionesInasistidas { get; set; }
        public decimal PorcentajeAsistencia { get; set; }
        public List<AreaTrabajoDto> AreasDetrabajo { get; set; } = new();
        public List<string> TareasParaCasa { get; set; } = new();
        public List<string> ProximosObjetivos { get; set; } = new();
        public string? Recomendaciones { get; set; }
    }

    public class AreaTrabajoDto
    {
        public string Area { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty; // "Mejorando", "Bueno", "Necesita trabajo"
        public string? Descripcion { get; set; }
    }
}
