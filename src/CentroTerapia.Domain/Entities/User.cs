namespace CentroTerapia.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string HashContrasena { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Rol { get; set; } = "Recepcionista"; // e.g., Admin
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public bool Activo { get; set; }


    }
}