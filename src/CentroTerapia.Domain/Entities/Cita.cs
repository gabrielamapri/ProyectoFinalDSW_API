namespace CentroTerapia.Domain.Entities
{
    public class Cita
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string Estado { get; set; } = "Scheduled"; // Scheduled, Confirmed, Attended, NoShow, Cancelled
        public string? Notas { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relaciones para centro de terapias
        public int PacienteId { get; set; }
        public Paciente? Paciente { get; set; }

        public int? TerapeutaId { get; set; }
        public Terapeuta? Terapeuta { get; set; }

        public int? TerapiaId { get; set; }
        public TipoSesion? Terapia { get; set; }

        public int DuracionMinutos { get; set; }

        // Concurrency control
        public byte[]? RowVersion { get; set; }

        public bool IsFutureAppointment() => Fecha > DateTime.Now;

        public bool CanBeCancelada() => Estado == "Scheduled" && IsFutureAppointment();
    }
}
