
using CentroTerapia.Domain.Entities;

namespace CentroTerapia.Domain.Ports.Out
{
    public interface ICitaRepository : IRepository<Cita>
    {
        Task<IEnumerable<Cita>> GetByPacienteIdAsync(int pacienteId);
        Task<IEnumerable<Cita>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Cita>> GetByStatusAsync(string status);
        Task<Cita?> GetWithPacienteAndFamiliaAsync(int id);
        // Returns all citas including related Paciente, Familia, TipoSesion and Terapeuta
        Task<IEnumerable<Cita>> GetAllWithRelationsAsync();
    }
}

