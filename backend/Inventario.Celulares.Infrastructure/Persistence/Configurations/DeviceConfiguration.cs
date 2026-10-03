using Inventario.Celulares.Core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Celulares.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuracion de mapeo de la entidad <see cref="Device"/>.
/// </summary>
public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("devices");
        builder.HasKey(device => device.Id);

        builder.Property(device => device.Brand)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(device => device.Model)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(device => device.Imei)
            .HasMaxLength(15)
            .IsRequired();

        builder.HasIndex(device => device.Imei)
            .IsUnique()
            .HasDatabaseName("ux_devices_imei");

        builder.Property(device => device.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(device => device.Observations)
            .HasMaxLength(500);

        builder.Property(device => device.PurchasePrice)
            .HasPrecision(12, 2);

        builder.Property(device => device.EntryDate)
            .IsRequired();

        builder.Property(device => device.CreatedAt)
            .IsRequired();

        builder.Property(device => device.UpdatedAt)
            .IsRequired();

        builder.HasIndex(device => device.Status)
            .HasDatabaseName("ix_devices_status");

        builder.HasIndex(device => device.Brand)
            .HasDatabaseName("ix_devices_brand");

        builder.HasOne(device => device.Location)
            .WithMany(location => location.Devices)
            .HasForeignKey(device => device.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
