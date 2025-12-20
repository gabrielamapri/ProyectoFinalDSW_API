using CentroTerapia.Domain.Entities;

namespace CentroTerapia.Domain.Ports.Out
{
    public interface IPacienteRepository : IRepository<Paciente>
    {
        Task<IEnumerable<Paciente>> GetByFamiliaIdAsync(int familiaId);
        Task<Paciente?> GetWithFamiliaAndCitasAsync(int id);
    }
}
