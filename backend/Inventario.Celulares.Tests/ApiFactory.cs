using Inventario.Celulares.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Inventario.Celulares.Tests;

/// <summary>
/// Fabrica que levanta la API en memoria con SQLite para pruebas de integracion.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<InventarioDbContext>>();
            services.RemoveAll<DbContextOptions>();

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<InventarioDbContext>(options => options.UseSqlite(_connection));
        });
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}
