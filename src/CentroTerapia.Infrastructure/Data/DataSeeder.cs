using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CentroTerapia.Domain.Ports.Out;
using CentroTerapia.Domain.Entities;

namespace CentroTerapia.Infrastructure.Data
{
    public class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;

            var config = provider.GetRequiredService<IConfiguration>();
            var logger = provider.GetService<ILogger<DataSeeder>>();

            var uow = provider.GetRequiredService<IUnitOfWork>();

            var adminEmail = config["Seed:AdminEmail"] ?? "admin@centroterapia.local";
            var adminPassword = config["Seed:AdminPassword"] ?? "ChangeMe123!";

            try
            {
                var exists = await uow.Users.ExistsByEmailAsync(adminEmail);
                if (!exists)
                {
                    var admin = new User
                    {
                        Correo = adminEmail,
                        HashContrasena = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                        Nombres = "Administrador",
                        Apellidos = "Sistema",
                        Rol = "Admin",
                        FechaCreacion = DateTime.UtcNow,
                        Activo = true
                    };

                    await uow.Users.CreateAsync(admin);
                    await uow.SaveChangesAsync();
                    logger?.LogInformation("Admin user created: {Email}", adminEmail);
                }
                else
                {
                    logger?.LogInformation("Admin user already exists: {Email}", adminEmail);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error while seeding admin user.");
                throw;
            }
        }
    }
}
