namespace CentroTerapia.Application.DTOs.Familia
{
    public class CreateFamiliaDto
    {
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
    }
}
