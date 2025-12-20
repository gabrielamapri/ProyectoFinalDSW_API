namespace CentroTerapia.Domain.Entities
{
    public class TipoSesion
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int DuracionMinutos { get; set; }
        public decimal? Precio { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
