using System.Linq;
using Inventario.Celulares.Core.Entidades;
using Inventario.Celulares.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Inventario.Celulares.Api.Persistence;

/// <summary>
/// Prepara la base de datos al arrancar la aplicacion.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Aplica las migraciones pendientes y siembra las ubicaciones base.
    /// </summary>
    /// <param name="services">Proveedor de servicios de la aplicacion.</param>
    /// <returns>Tarea asincrona de inicializacion.</returns>
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(SeedData).FullName!);
        var context = provider.GetRequiredService<InventarioDbContext>();

        if (!await context.Database.CanConnectAsync().ConfigureAwait(false))
        {
            logger.LogWarning("La base de datos no esta disponible; se omite la inicializacion.");
            return;
        }

        var pending = (await context.Database.GetPendingMigrationsAsync().ConfigureAwait(false)).ToList();
        if (pending.Count > 0)
        {
            logger.LogInformation("Aplicando {Cantidad} migracion(es) pendiente(s).", pending.Count);
            await context.Database.MigrateAsync().ConfigureAwait(false);
        }

        await SeedLocationsAsync(context, logger).ConfigureAwait(false);
    }

    /// <summary>
    /// Inserta las ubicaciones base si la tabla aun no tiene registros.
    /// </summary>
    /// <param name="context">Contexto de datos ya configurado.</param>
    /// <param name="logger">Registrador de diagnostico.</param>
    /// <returns>Tarea asincrona de siembra.</returns>
    public static async Task SeedLocationsAsync(InventarioDbContext context, ILogger logger)
    {
        if (await context.Locations.AnyAsync().ConfigureAwait(false))
        {
            return;
        }

        context.Locations.AddRange(
            new Location { Name = "Almacen Central", Code = "ALM-01", Description = "Deposito principal" },
            new Location { Name = "Sala de Soporte", Code = "SOP-01", Description = "Equipos en mantenimiento" },
            new Location { Name = "Oficina de TI", Code = "TI-01", Description = "Equipos asignados a personal" });

        await context.SaveChangesAsync().ConfigureAwait(false);
        logger.LogInformation("Ubicaciones iniciales cargadas.");
    }
}
