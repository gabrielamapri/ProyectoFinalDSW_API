namespace CentroTerapia.Application.DTOs.Terapeuta
{
    public class TerapeutaDto
    {
        public int Id { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string? DNI { get; set; }
        public string? Correo { get; set; }
        public int? EspecialidadId { get; set; }
        public string EspecialidadNombre { get; set; } = string.Empty;
        public string Presentacion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}

