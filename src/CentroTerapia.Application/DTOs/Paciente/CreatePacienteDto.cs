namespace CentroTerapia.Application.DTOs.Paciente
{
    public class CreatePacienteDto
    {
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public int? FamiliaId { get; set; }
        public string DNI { get; set; } = string.Empty;
        public string Sexo { get; set; } = string.Empty;
        public string NombreContactoEmergencia { get; set; } = string.Empty;
        public string NumeroContactoEmergencia { get; set; } = string.Empty;
    }
}
