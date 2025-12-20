using System;
using System.Collections.Generic;
using CentroTerapia.Application.DTOs.Paciente;

namespace CentroTerapia.Application.DTOs.Familia
{
    public class FamiliaDto
    {
        public int Id { get; set; }
        public string? TelefonoContacto { get; set; }

        public string? Responsable1Nombre { get; set; }
        public string? Responsable1Apellido { get; set; }
        public string? Responsable1DNI { get; set; }
        public string? Responsable1Direccion { get; set; }
        public string? Responsable1Email { get; set; }
        public string? Responsable1Telefono { get; set; }
        public string? Responsable1Relacion { get; set; }

        public string? Responsable2Nombre { get; set; }
        public string? Responsable2Apellido { get; set; }
        public string? Responsable2DNI { get; set; }
        public string? Responsable2Direccion { get; set; }
        public string? Responsable2Email { get; set; }
        public string? Responsable2Telefono { get; set; }
        public string? Responsable2Relacion { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        // Resumen de pacientes (opcional)
        public IEnumerable<PacienteDto>? Pacientes { get; set; }
    }
}
