using Inventario.Celulares.Core.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Celulares.Infrastructure.Persistence;

/// <summary>
/// Contexto de datos de la aplicacion de inventario.
/// </summary>
public class InventarioDbContext : DbContext
{
    /// <summary>
    /// Inicializa el contexto con las opciones configuradas.
    /// </summary>
    /// <param name="options">Opciones de EF Core.</param>
    public InventarioDbContext(DbContextOptions<InventarioDbContext> options)
        : base(options)
    {
    }

    /// <summary>Equipos celulares del inventario.</summary>
    public DbSet<Device> Devices => Set<Device>();

    /// <summary>Movimientos de inventario.</summary>
    public DbSet<Movement> Movements => Set<Movement>();

    /// <summary>Ubicaciones fisicas o logicas.</summary>
    public DbSet<Location> Locations => Set<Location>();

    /// <summary>Usuarios del sistema.</summary>
    public DbSet<User> Users => Set<User>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventarioDbContext).Assembly);
    }
}
