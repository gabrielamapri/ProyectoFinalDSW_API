namespace CentroTerapia.Application.DTOs.Paciente
{
    public class PacienteDto
    {
        public int Id { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public int AgeInYears { get; set; }
        public string Sexo { get; set; } = string.Empty;
        public string NombreContactoEmergencia { get; set; } = string.Empty;
        public string NumeroContactoEmergencia { get; set; } = string.Empty;
        public int FamiliaId { get; set; }
        public string ResponsableNombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }
}

