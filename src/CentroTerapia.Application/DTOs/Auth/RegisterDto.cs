namespace CentroTerapia.Application.DTOs.Auth
{
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        // Solo puede ser "Admin", "Terapeuta" o "Padre"
        public string Role { get; set; } = "Padre";
    }
}
