using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Inventario.Celulares.Core.DTOs;
using Inventario.Celulares.Core.Entidades;
using Inventario.Celulares.Core.Enumeraciones;
using Inventario.Celulares.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Inventario.Celulares.Tests;

/// <summary>
/// Pruebas de integracion sobre los endpoints HTTP de la API.
/// </summary>
public class DevicesApiTests : IClassFixture<ApiFactory>, IDisposable
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    /// <summary>
    /// Inicializa el cliente HTTP con la API en memoria.
    /// </summary>
    /// <param name="factory">Fabrica de la aplicacion de pruebas.</param>
    private static readonly JsonSerializerOptions SerializerOptions = CrearOpcionesJson();

    public DevicesApiTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Usa las mismas reglas de serializacion que el servidor: enums como texto.
    /// </summary>
    private static JsonSerializerOptions CrearOpcionesJson()
    {
        var opciones = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        opciones.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: true));
        return opciones;
    }

    /// <summary>
    /// Limpia los datos compartidos entre pruebas.
    /// </summary>
    public void Dispose()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
        context.Movements.RemoveRange(context.Movements);
        context.Devices.RemoveRange(context.Devices);
        context.SaveChanges();
        _client.Dispose();
    }

    private async Task<Location> ObtenerAlmacenAsync()
    {
        var locations = await _client.GetFromJsonAsync<List<Location>>("/locations", SerializerOptions);
        return locations!.First(location => location.Code == "ALM-01");
    }

    private async Task<Guid> CrearEquipoAsync(string imei = "490154203237518")
    {
        var almacen = await ObtenerAlmacenAsync();
        var respuesta = await _client.PostAsJsonAsync("/devices", new CreateDeviceRequest
        {
            Brand = "Samsung",
            Model = "Galaxy S22",
            Imei = imei,
            Status = DeviceStatus.Disponible,
            LocationId = almacen.Id,
            EntryDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            PurchasePrice = 1800m
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var creado = await respuesta.Content.ReadFromJsonAsync<DeviceResponse>(SerializerOptions);
        return creado!.Id;
    }

    [Fact]
    public async Task Health_DebeResponderOk()
    {
        var respuesta = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [Fact]
    public async Task Locations_DebeDevolverLasUbicacionesSembradas()
    {
        var ubicaciones = await _client.GetFromJsonAsync<List<Location>>("/locations", SerializerOptions);

        Assert.NotNull(ubicaciones);
        Assert.Equal(3, ubicaciones!.Count);
    }

    [Fact]
    public async Task PostDevice_ConDatosValidos_DebeDevolver201YLocation()
    {
        var almacen = await ObtenerAlmacenAsync();

        var respuesta = await _client.PostAsJsonAsync("/devices", new CreateDeviceRequest
        {
            Brand = "Xiaomi",
            Model = "Redmi Note 12",
            Imei = "356938035643809",
            LocationId = almacen.Id,
            EntryDate = new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc)
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        Assert.NotNull(respuesta.Headers.Location);
        var creado = await respuesta.Content.ReadFromJsonAsync<DeviceResponse>(SerializerOptions);
        Assert.Equal("Xiaomi", creado!.Brand);
        Assert.Equal("Almacen Central", creado.LocationName);
    }

    [Fact]
    public async Task PostDevice_SinMarca_DebeDevolver400ConErroresDeValidacion()
    {
        var almacen = await ObtenerAlmacenAsync();

        var respuesta = await _client.PostAsJsonAsync("/devices", new CreateDeviceRequest
        {
            Brand = string.Empty,
            Model = "Sin marca",
            Imei = "490154203237518",
            LocationId = almacen.Id,
            EntryDate = DateTime.UtcNow
        });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("marca", cuerpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PostDevice_ConImeiDuplicado_DebeDevolver409()
    {
        await CrearEquipoAsync();

        var almacen = await ObtenerAlmacenAsync();
        var respuesta = await _client.PostAsJsonAsync("/devices", new CreateDeviceRequest
        {
            Brand = "Otro",
            Model = "Duplicado",
            Imei = "490154203237518",
            LocationId = almacen.Id,
            EntryDate = DateTime.UtcNow
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task GetDevices_SinParametros_DebePaginarLosEquipos()
    {
        await CrearEquipoAsync();

        var pagina = await _client.GetFromJsonAsync<PagedResult<DeviceResponse>>("/devices?page=1&pageSize=10", SerializerOptions);

        Assert.NotNull(pagina);
        Assert.Equal(1, pagina!.TotalItems);
        Assert.Single(pagina.Items);
    }

    [Fact]
    public async Task GetDevices_ConSearchInvalido_DebeDevolver400()
    {
        var respuesta = await _client.GetAsync("/devices?page=0");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task GetDeviceById_ConIdExistente_DebeDevolverElEquipo()
    {
        var id = await CrearEquipoAsync();

        var equipo = await _client.GetFromJsonAsync<DeviceResponse>($"/devices/{id}", SerializerOptions);

        Assert.Equal("Galaxy S22", equipo!.Model);
        Assert.Equal("490154203237518", equipo.Imei);
    }

    [Fact]
    public async Task GetDeviceById_ConIdInexistente_DebeDevolver404()
    {
        var respuesta = await _client.GetAsync($"/devices/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task PutDevice_DebeActualizarElEquipo()
    {
        var id = await CrearEquipoAsync();

        var respuesta = await _client.PutAsJsonAsync($"/devices/{id}", new UpdateDeviceRequest
        {
            Model = "Galaxy S23",
            Observations = "Actualizado desde la prueba"
        });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var actualizado = await respuesta.Content.ReadFromJsonAsync<DeviceResponse>(SerializerOptions);
        Assert.Equal("Galaxy S23", actualizado!.Model);
    }

    [Fact]
    public async Task DeleteDevice_DebeDevolver204YEliminarElEquipo()
    {
        var id = await CrearEquipoAsync();

        var respuesta = await _client.DeleteAsync($"/devices/{id}");

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        var consulta = await _client.GetAsync($"/devices/{id}");
        Assert.Equal(HttpStatusCode.NotFound, consulta.StatusCode);
    }

    [Fact]
    public async Task PostMovement_Traslado_DebeRegistrarElMovimiento()
    {
        var id = await CrearEquipoAsync();
        var ubicaciones = await _client.GetFromJsonAsync<List<Location>>("/locations", SerializerOptions);
        var soporte = ubicaciones!.First(location => location.Code == "SOP-01");

        var respuesta = await _client.PostAsJsonAsync("/movements", new CreateMovementRequest
        {
            DeviceId = id,
            Type = MovementType.Traslado,
            ToLocationId = soporte.Id,
            Reason = "Cambio de display"
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var movimiento = await respuesta.Content.ReadFromJsonAsync<MovementResponse>(SerializerOptions);
        Assert.Equal(MovementType.Traslado, movimiento!.Type);
        Assert.Equal("Sala de Soporte", movimiento.ToLocationName);
    }

    [Fact]
    public async Task GetMovements_ConDeviceId_DebeDevolverElHistorial()
    {
        var id = await CrearEquipoAsync();
        var ubicaciones = await _client.GetFromJsonAsync<List<Location>>("/locations", SerializerOptions);
        var soporte = ubicaciones!.First(location => location.Code == "SOP-01");

        await _client.PostAsJsonAsync("/movements", new CreateMovementRequest
        {
            DeviceId = id,
            Type = MovementType.Traslado,
            ToLocationId = soporte.Id
        });

        var historial = await _client.GetFromJsonAsync<List<MovementResponse>>($"/movements?deviceId={id}", SerializerOptions);

        Assert.NotNull(historial);
        Assert.Equal(2, historial!.Count);
        Assert.Contains(historial, item => item.Type == MovementType.Ingreso);
    }

    [Fact]
    public async Task GetMovements_ConEquipoInexistente_DebeDevolverListaVacia()
    {
        var historial = await _client.GetFromJsonAsync<List<MovementResponse>>($"/movements?deviceId={Guid.NewGuid()}", SerializerOptions);

        Assert.Empty(historial!);
    }

    [Fact]
    public async Task GetStockReport_DebeDevolverLosTotalesDelInventario()
    {
        await CrearEquipoAsync();

        var reporte = await _client.GetFromJsonAsync<StockReportResponse>("/reports/stock", SerializerOptions);

        Assert.NotNull(reporte);
        Assert.True(reporte!.TotalDevices >= 1);
        Assert.NotEmpty(reporte.ByStatus);
        Assert.NotEmpty(reporte.ByLocation);
    }

    [Fact]
    public async Task PostDevice_ConEstadoComoTexto_DebeAceptarseYDevolverElNombreDelEstado()
    {
        var almacen = await ObtenerAlmacenAsync();
        var imei = "352099001761481";

        var respuesta = await _client.PostAsJsonAsync("/devices", new
        {
            brand = "Xiaomi",
            model = "Redmi Note 12",
            imei,
            status = "Disponible",
            locationId = almacen.Id,
            entryDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var creado = await respuesta.Content.ReadFromJsonAsync<DeviceResponse>(SerializerOptions);
        Assert.Equal("Disponible", creado!.StatusName);
    }

    [Fact]
    public async Task PostMovement_ConTipoComoTexto_DebeAceptarse()
    {
        var id = await CrearEquipoAsync();
        var ubicaciones = await _client.GetFromJsonAsync<List<Location>>("/locations", SerializerOptions);
        var soporte = ubicaciones!.First(location => location.Code == "SOP-01");

        var respuesta = await _client.PostAsJsonAsync("/movements", new
        {
            deviceId = id,
            type = "Traslado",
            toLocationId = soporte.Id,
            reason = "Movimiento con enum en texto"
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var movimiento = await respuesta.Content.ReadFromJsonAsync<MovementResponse>(SerializerOptions);
        Assert.Equal("Traslado", movimiento!.TypeName);
    }

    [Fact]
    public async Task Swagger_DebeServirLaDocumentacionDeLaApi()
    {
        var respuesta = await _client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("/devices", cuerpo, StringComparison.Ordinal);
        Assert.Contains("/movements", cuerpo, StringComparison.Ordinal);
        Assert.Contains("/reports/stock", cuerpo, StringComparison.Ordinal);
    }
}
