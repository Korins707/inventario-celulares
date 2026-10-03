using Inventario.Celulares.Core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventario.Celulares.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuracion de mapeo de la entidad <see cref="Location"/>.
/// </summary>
public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        builder.HasKey(location => location.Id);

        builder.Property(location => location.Name)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(location => location.Code)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(location => location.Description)
            .HasMaxLength(200);

        builder.HasIndex(location => location.Code)
            .IsUnique()
            .HasDatabaseName("ux_locations_code");
    }
}
