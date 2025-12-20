namespace CentroTerapia.Application.DTOs.Terapeuta
{
    public class CreateTerapeutaDto
    {
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Especialidades { get; set; } = string.Empty;
        public string Presentacion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
    }
}
