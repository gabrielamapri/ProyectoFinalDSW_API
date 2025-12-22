namespace CentroTerapia.Application.DTOs.Terapia
{
    public class TipoSesionDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int DuracionMinutos { get; set; }
        public decimal? Precio { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int EspecialidadId { get; set; }
        public string EspecialidadNombre { get; set; } = string.Empty;
    }
}
