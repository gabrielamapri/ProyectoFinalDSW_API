using CentroTerapia.Domain.Entities;
using CentroTerapia.Domain.Ports.Out;
using CentroTerapia.Infrastructure.Persistence.Context;

namespace CentroTerapia.Infrastructure.Persistence.Repositories
{
    public class TipoSesionRepository : Repository<TipoSesion>, ITipoSesionRepository
    {
        public TipoSesionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
