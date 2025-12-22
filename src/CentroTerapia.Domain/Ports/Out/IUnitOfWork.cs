
namespace CentroTerapia.Domain.Ports.Out
{
    public interface IUnitOfWork : IDisposable
    {
        ICitaRepository Citas { get; }
        IPacienteRepository Pacientes { get; }
        ITerapeutaRepository Terapeutas { get; }
        IUserRepository Users { get; }
        IFamiliaRepository Familias { get; }
        ITipoSesionRepository TiposSesion { get; }
        INotaSesionRepository NotasSesion { get; }
        IFranjaRepository Franjas { get; }
        IFranjaExcepcionRepository Excepciones { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}

