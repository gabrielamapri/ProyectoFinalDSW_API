
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CentroTerapia.Domain.Entities;

namespace CentroTerapia.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration: IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Correo)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.HashContrasena)
                .IsRequired();

            builder.Property(u => u.Nombres)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Apellidos)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Rol)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.FechaCreacion)
                .IsRequired();

            builder.Property(u => u.Activo)
                .IsRequired();

            builder.HasIndex(u => u.Correo).IsUnique();
        }
    }
}
