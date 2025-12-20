namespace CentroTerapia.Domain.Entities
{
    public class Terapeuta
    {
        public int Id { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        // Replace free-text `Especialidades` with FK to `Especialidad`
        public int? EspecialidadId { get; set; }
        public Especialidad? Especialidad { get; set; }
        public string Presentacion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public ICollection<FranjaDisponibilidad> Franjas { get; set; } = new List<FranjaDisponibilidad>();
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}

