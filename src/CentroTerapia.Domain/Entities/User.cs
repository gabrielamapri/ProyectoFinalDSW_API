namespace CentroTerapia.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string HashContrasena { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        // Solo puede ser "Admin", "Terapeuta" o "Padre"
        public string Rol { get; set; } = "Padre";
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public bool Activo { get; set; }


    }
}