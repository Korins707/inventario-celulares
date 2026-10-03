using Inventario.Celulares.Core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Celulares.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuracion de mapeo de la entidad <see cref="Movement"/>.
/// </summary>
public class MovementConfiguration : IEntityTypeConfiguration<Movement>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Movement> builder)
    {
        builder.ToTable("movements");
        builder.HasKey(movement => movement.Id);

        builder.Property(movement => movement.Type)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(movement => movement.Reason)
            .HasMaxLength(300);

        builder.Property(movement => movement.OccurredAt)
            .IsRequired();

        builder.Property(movement => movement.CreatedAt)
            .IsRequired();

        builder.HasIndex(movement => movement.DeviceId)
            .HasDatabaseName("ix_movements_device_id");

        builder.HasIndex(movement => movement.OccurredAt)
            .HasDatabaseName("ix_movements_occurred_at");

        builder.HasOne(movement => movement.Device)
            .WithMany(device => device.Movements)
            .HasForeignKey(movement => movement.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(movement => movement.FromLocation)
            .WithMany(location => location.OriginMovements)
            .HasForeignKey(movement => movement.FromLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(movement => movement.ToLocation)
            .WithMany(location => location.DestinationMovements)
            .HasForeignKey(movement => movement.ToLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
