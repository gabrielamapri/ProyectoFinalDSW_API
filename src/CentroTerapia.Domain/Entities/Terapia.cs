namespace CentroTerapia.Domain.Entities
{
    public class Terapia
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int DuracionMinutos { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int? EdadDesde { get; set; }
        public int? EdadHasta { get; set; }
        public string Requisitos { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }
}
