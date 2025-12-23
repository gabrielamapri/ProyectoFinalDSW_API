using Microsoft.EntityFrameworkCore;
using CentroTerapia.Domain.Entities;
using CentroTerapia.Infrastructure.Persistence.Configurations;

namespace CentroTerapia.Infrastructure.Persistence.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cita> Citas { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<NotaSesion> NotasSesion { get; set; }
        public DbSet<Especialidad> Especialidades { get; set; }
        public DbSet<Terapeuta> Terapeutas { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Familia> Familias { get; set; }
        public DbSet<TipoSesion> TiposSesion { get; set; }
        public DbSet<FranjaDisponibilidad> FranjasDisponibilidad { get; set; }
        public DbSet<FranjaExcepcion> FranjaExcepciones { get; set; }
        public DbSet<Terapia> Terapias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones para CentroTerapia
            modelBuilder.ApplyConfiguration(new CitaConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new NotaSesionConfiguration());
            modelBuilder.ApplyConfiguration(new TerapeutaConfiguration());

            // Apply default conventions for new entities
            modelBuilder.Entity<Especialidad>(eb =>
            {
                eb.Property(e => e.Nombre).IsRequired();
            });

            modelBuilder.Entity<Terapeuta>(tb =>
            {
                tb.HasOne(t => t.Especialidad)
                  .WithMany(e => e.Terapeutas)
                  .HasForeignKey(t => t.EspecialidadId)
                  .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Paciente>(pb =>
            {
                pb.Property(p => p.Sexo).HasConversion<int>();
            });

            // FranjaExcepcion: unique per (FranjaId, Fecha)
            modelBuilder.Entity<FranjaExcepcion>(eb =>
            {
                eb.HasOne(e => e.Franja)
                  .WithMany()
                  .HasForeignKey(e => e.FranjaId)
                  .OnDelete(DeleteBehavior.Cascade);

                eb.HasIndex(e => new { e.FranjaId, e.Fecha }).IsUnique();
            });
        }
    }
}

