namespace CentroTerapia.Domain.Entities
{
    public class Paciente
    {
        public int Id { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Sexo { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public string NombreContactoEmergencia { get; set; } = string.Empty;
        public string NumeroContactoEmergencia { get; set; } = string.Empty;
        

        public int FamiliaId { get; set; }
        public Familia? Familia { get; set; }

        public ICollection<Cita> Citas { get; set; } = new List<Cita>();

        public int CalcularEdad()
        {
            var today = DateTime.Today;
            var age = today.Year - FechaNacimiento.Year;
            if (FechaNacimiento.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}

