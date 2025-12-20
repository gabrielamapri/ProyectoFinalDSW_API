namespace CentroTerapia.Application.DTOs.Terapia
{
    public class CreateTipoSesionDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int DuracionMinutos { get; set; }
        public decimal? Precio { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
