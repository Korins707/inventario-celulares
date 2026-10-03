using System.Text.Json.Serialization;
using Inventario.Celulares.Api.Middleware;
using Inventario.Celulares.Api.Persistence;
using Inventario.Celulares.Core.Interfaces;
using Inventario.Celulares.Infrastructure.Persistence;
using Inventario.Celulares.Infrastructure.Security;
using Inventario.Celulares.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Aceptar enums como texto ("Disponible") ademas del valor numerico.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: true));
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<InventarioDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "No se configuro la cadena de conexion 'DefaultConnection'. "
            + "Defina ConnectionStrings__DefaultConnection en las variables de entorno.");
    }

    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

builder.Services.AddCors(options => options.AddPolicy("FrontendPolicy", policy =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:5173" };
    policy.WithOrigins(origins)
        .AllowAnyHeader()
        .AllowAnyMethod();
}));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("FrontendPolicy");
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("HealthCheck")
    .WithTags("Health");

await SeedData.InitializeAsync(app.Services);

app.Run();

/// <summary>
/// Punto de entrada de la API, expuesto para las pruebas de integracion.
/// </summary>
public partial class Program
{
}
