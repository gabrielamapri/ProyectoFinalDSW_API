using Microsoft.Extensions.DependencyInjection;
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Application.Services;
using CentroTerapia.Application.Mappings;
using FluentValidation;
using CentroTerapia.Application.Validators;

namespace CentroTerapia.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile));

            // Servicios de Citas, Pacientes y Terapeutas
            services.AddScoped<ICitaService, CitaService>();
            services.AddScoped<IPacienteService, PacienteService>();
            services.AddScoped<ITerapeutaService, TerapeutaService>();
            services.AddScoped<IFamiliaService, FamiliaService>();
            services.AddScoped<ITipoSesionService, TipoSesionService>();
            services.AddScoped<INotaSesionService, NotaSesionService>();
            services.AddScoped<IFranjaService, FranjaService>();
            services.AddScoped<IAuthService, AuthService>();

            // Keep appointment validators
            services.AddValidatorsFromAssemblyContaining<CreateCitaDtoValidator>();


            return services;
        }
    }
}
