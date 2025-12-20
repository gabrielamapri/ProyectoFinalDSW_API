namespace CentroTerapia.Application.DTOs.NotaSesion
{
    public class CreateNotaSesionDto
    {
        public int CitaId { get; set; }
        public int TerapeutaId { get; set; }
        public string Notas { get; set; } = string.Empty;
    }
}
