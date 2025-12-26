using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CentroTerapia.Domain.Ports.Out;
using CentroTerapia.Infrastructure.Persistence.Context;
using CentroTerapia.Infrastructure.Persistence.Repositories;

namespace CentroTerapia.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var port = Environment.GetEnvironmentVariable("DB_PORT") ;
        var database = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var connectionString = $"Server={host};Port={port};Database={database};User={user};Password={password};";        
        Console.WriteLine($"Connection String: {connectionString}");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 0))
            )
        );

        // Repositorios para CentroTerapia
        services.AddScoped<ICitaRepository, CitaRepository>();
        services.AddScoped<IPacienteRepository, PacienteRepository>();
        services.AddScoped<ITerapeutaRepository, TerapeutaRepository>();
        services.AddScoped<IFamiliaRepository, FamiliaRepository>();
        services.AddScoped<ITipoSesionRepository, TipoSesionRepository>();
        services.AddScoped<INotaSesionRepository, NotaSesionRepository>();
        services.AddScoped<IFranjaRepository, FranjaRepository>();
        services.AddScoped<IFranjaExcepcionRepository, FranjaExcepcionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
                                                            