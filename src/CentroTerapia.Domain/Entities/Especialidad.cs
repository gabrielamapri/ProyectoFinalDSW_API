using System;

namespace CentroTerapia.Domain.Entities
{
    public class Especialidad
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public ICollection<Terapeuta> Terapeutas { get; set; } = new List<Terapeuta>();
    }
}
