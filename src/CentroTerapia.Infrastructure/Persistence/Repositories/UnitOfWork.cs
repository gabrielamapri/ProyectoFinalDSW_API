using Microsoft.EntityFrameworkCore.Storage;
using CentroTerapia.Domain.Ports.Out;
using CentroTerapia.Infrastructure.Persistence.Context;

namespace CentroTerapia.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public ICitaRepository Citas { get; }
        public IPacienteRepository Pacientes { get; }
        public ITerapeutaRepository Terapeutas { get; }
        public IUserRepository Users { get; }
        public IFamiliaRepository Familias { get; }
        public ITipoSesionRepository TiposSesion { get; }
        public INotaSesionRepository NotasSesion { get; }
    public IFranjaRepository Franjas { get; }

        public UnitOfWork(ApplicationDbContext context,
                          ICitaRepository citaRepository,
                          IPacienteRepository pacienteRepository,
                          ITerapeutaRepository terapeutaRepository,
                          IUserRepository userRepository,
                          IFamiliaRepository familiaRepository,
                          ITipoSesionRepository tipoSesionRepository,
                          INotaSesionRepository notaSesionRepository,
                          IFranjaRepository franjaRepository
                          )
        {
            _context = context;
            Citas = citaRepository;
            Pacientes = pacienteRepository;
            Terapeutas = terapeutaRepository;
            Users = userRepository;
            Familias = familiaRepository;
            TiposSesion = tipoSesionRepository;
            NotasSesion = notaSesionRepository;
            Franjas = franjaRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }
        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

