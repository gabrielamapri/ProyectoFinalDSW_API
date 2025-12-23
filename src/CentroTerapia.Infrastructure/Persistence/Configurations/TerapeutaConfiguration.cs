using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CentroTerapia.Domain.Entities;

namespace CentroTerapia.Infrastructure.Persistence.Configurations
{
    public class TerapeutaConfiguration : IEntityTypeConfiguration<Terapeuta>
    {
        public void Configure(EntityTypeBuilder<Terapeuta> builder)
        {
            builder.ToTable("Terapeutas");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Nombres)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.Apellidos)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.DNI)
                .HasMaxLength(20);

            builder.Property(t => t.Correo)
                .HasMaxLength(100);

            builder.Property(t => t.Presentacion)
                .HasMaxLength(500);

            builder.Property(t => t.Telefono)
                .HasMaxLength(20);

            builder.Property(t => t.Direccion)
                .HasMaxLength(200);

            builder.HasIndex(t => t.DNI);
            builder.HasIndex(t => t.Correo);
        }
    }
}
