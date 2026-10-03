using Inventario.Celulares.Core.Entidades;
using Inventario.Celulares.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Celulares.Tests;

/// <summary>
/// Crea una base SQLite en memoria compartida para ejecutar pruebas.
/// </summary>
public static class TestDatabase
{
    /// <summary>
    /// Abre una conexion SQLite en memoria y aplica el esquema.
    /// </summary>
    /// <returns>Conexion abierta, el llamador debe descartarla.</returns>
    public static SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<InventarioDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new InventarioDbContext(options);
        context.Database.EnsureCreated();

        Seed(context);
        return connection;
    }

    private static void Seed(InventarioDbContext context)
    {
        var almacen = new Location { Id = SeedIds.Almacen, Name = "Almacen Central", Code = "ALM-01" };
        var soporte = new Location { Id = SeedIds.Soporte, Name = "Sala de Soporte", Code = "SOP-01" };

        context.Locations.AddRange(almacen, soporte);
        context.SaveChanges();
    }
}

/// <summary>
/// Identificadores deterministas usados por las pruebas.
/// </summary>
public static class SeedIds
{
    /// <summary>Ubicacion de almacen.</summary>
    public static readonly Guid Almacen = new("11111111-1111-1111-1111-111111111111");

    /// <summary>Ubicacion de soporte.</summary>
    public static readonly Guid Soporte = new("22222222-2222-2222-2222-222222222222");

    /// <summary>Ubicacion de la oficina de TI.</summary>
    public static readonly Guid Ti = new("33333333-3333-3333-3333-333333333333");
}
