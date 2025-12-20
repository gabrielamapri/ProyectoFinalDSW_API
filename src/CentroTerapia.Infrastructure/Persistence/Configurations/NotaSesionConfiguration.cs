using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CentroTerapia.Domain.Entities;

namespace CentroTerapia.Infrastructure.Persistence.Configurations
{
    public class NotaSesionConfiguration : IEntityTypeConfiguration<NotaSesion>
    {
        public void Configure(EntityTypeBuilder<NotaSesion> builder)
        {
            builder.ToTable("NotaSesion");
            builder.HasKey(n => n.Id);

            builder.Property(n => n.CitaId)
                .IsRequired();

            builder.Property(n => n.TerapeutaId)
                .IsRequired();

            builder.Property(n => n.Notas)
                .HasColumnType("longtext");

            builder.Property(n => n.FechaCreacion)
                .IsRequired();

            builder.HasOne(n => n.Cita)
                .WithMany()
                .HasForeignKey(n => n.CitaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(n => n.Terapeuta)
                .WithMany()
                .HasForeignKey(n => n.TerapeutaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
