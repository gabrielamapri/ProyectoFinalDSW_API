using System.Collections.Generic;
using CentroTerapia.Application.DTOs.Cita;

namespace CentroTerapia.Application.DTOs.Franja
{
    public class DeleteFranjaResultDto
    {
        public bool Deleted { get; set; }
        public List<CitaAlertaDto> FutureAppointments { get; set; } = new();
    }
}