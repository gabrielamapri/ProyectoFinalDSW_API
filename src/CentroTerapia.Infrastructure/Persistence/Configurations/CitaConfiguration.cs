using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CentroTerapia.Domain.Entities;

namespace CentroTerapia.Infrastructure.Persistence.Configurations
{
    public class CitaConfiguration : IEntityTypeConfiguration<Cita>
    {
        public void Configure(EntityTypeBuilder<Cita> builder)
        {
            builder.ToTable("Citas");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Fecha)
                .IsRequired();

            builder.Property(a => a.Motivo)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.Estado)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(a => a.Notas)
                .HasMaxLength(1000);

            builder.Property(a => a.FechaCreacion)
                .IsRequired();

            builder.HasIndex(a => a.PacienteId);
            builder.HasIndex(a => a.Fecha);
            builder.HasIndex(a => a.Estado);

        }
    }
}

