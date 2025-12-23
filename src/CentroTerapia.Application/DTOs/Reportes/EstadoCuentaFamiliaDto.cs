namespace CentroTerapia.Application.DTOs.Reportes
{
    public class EstadoCuentaFamiliaDto
    {
        public int FamiliaId { get; set; }
        public string ResponsablePrincipalNombre { get; set; } = string.Empty;
        public string ResponsablePrincipalEmail { get; set; } = string.Empty;
        public string ResponsablePrincipalTelefono { get; set; } = string.Empty;
        public List<EstadoCuentaPacienteDto> Pacientes { get; set; } = new();
        public decimal TotalDeuda { get; set; }
        public decimal TotalPagado { get; set; }
    }

    public class EstadoCuentaPacienteDto
    {
        public int PacienteId { get; set; }
        public string PacienteNombre { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string Especialidad { get; set; } = string.Empty;
        public int SesionesRealizadas { get; set; }
        public decimal PrecioPorSesion { get; set; }
        public decimal TotalSesiones { get; set; }
        public decimal MontoAbonado { get; set; }
        public decimal SaldoDeuda { get; set; }
        public int SesionesProximas { get; set; }
        public decimal CostoSesionesProximas { get; set; }
        public decimal DeudaTotal { get; set; }
    }
}
