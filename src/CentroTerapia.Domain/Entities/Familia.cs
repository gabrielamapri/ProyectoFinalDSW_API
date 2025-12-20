using System;
using System.Collections.Generic;

namespace CentroTerapia.Domain.Entities
{
    public class Familia
    {
        public int Id { get; set; }

        public string? TelefonoContacto { get; set; }

        public string? ResponsablePrincipalNombre { get; set; }
        public string? ResponsablePrincipalApellido { get; set; }
        public string? ResponsablePrincipalDNI { get; set; }
        public string? ResponsablePrincipalDireccion { get; set; }
        public string? ResponsablePrincipalEmail { get; set; }
        public string? ResponsablePrincipalTelefono { get; set; }
        public string? ResponsablePrincipalRelacion { get; set; }

        public string? Responsable2Nombre { get; set; }
        public string? Responsable2Apellido { get; set; }
        public string? Responsable2DNI { get; set; }
        public string? Responsable2Direccion { get; set; }
        
        public string? Responsable2Telefono { get; set; }
        public string? Responsable2Relacion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }

        // Relación con pacientes
        public ICollection<Paciente>? Pacientes { get; set; } = new List<Paciente>();
    }
}
