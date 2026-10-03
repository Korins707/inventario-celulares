using Inventario.Celulares.Core.DTOs;
using Inventario.Celulares.Core.Enumeraciones;
using Inventario.Celulares.Infrastructure.Persistence;
using Inventario.Celulares.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Celulares.Tests;

/// <summary>
/// Pruebas unitarias del servicio de inventario sobre SQLite en memoria.
/// </summary>
public class InventoryServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly InventarioDbContext _context;
    private readonly InventoryService _service;

    /// <summary>
    /// Prepara una base limpia para cada prueba.
    /// </summary>
    public InventoryServiceTests()
    {
        _connection = TestDatabase.CreateConnection();
        var options = new DbContextOptionsBuilder<InventarioDbContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new InventarioDbContext(options);
        _service = new InventoryService(_context);
    }

    /// <summary>
    /// Libera la conexion de la base en memoria.
    /// </summary>
    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    private static CreateDeviceRequest NewDevice(string imei = "490154203237518") => new()
    {
        Brand = "Samsung",
        Model = "Galaxy S22",
        Imei = imei,
        Status = DeviceStatus.Disponible,
        LocationId = SeedIds.Almacen,
        EntryDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    [Fact]
    public async Task CreateDeviceAsync_ConDatosValidos_DebeRegistrarElEquipo()
    {
        var result = await _service.CreateDeviceAsync(NewDevice(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Samsung", result.Data!.Brand);
        Assert.Equal("Almacen Central", result.Data.LocationName);
        var guardado = await _context.Devices.CountAsync();
        Assert.Equal(1, guardado);
    }

    [Fact]
    public async Task CreateDeviceAsync_GeneraMovimientoDeIngresoAutomatico()
    {
        await _service.CreateDeviceAsync(NewDevice(), CancellationToken.None);

        var movimientos = await _context.Movements.ToListAsync();
        Assert.Single(movimientos);
        Assert.Equal(MovementType.Ingreso, movimientos[0].Type);
    }

    [Fact]
    public async Task CreateDeviceAsync_ConImeiInvalido_DebeFallar()
    {
        var result = await _service.CreateDeviceAsync(NewDevice("123456789012345"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("IMEI_INVALIDO", result.ErrorCode);
        Assert.Equal(0, await _context.Devices.CountAsync());
    }

    [Fact]
    public async Task CreateDeviceAsync_ConImeiDuplicado_DebeFallar()
    {
        await _service.CreateDeviceAsync(NewDevice(), CancellationToken.None);

        var result = await _service.CreateDeviceAsync(NewDevice(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("IMEI_DUPLICADO", result.ErrorCode);
        Assert.Equal(1, await _context.Devices.CountAsync());
    }

    [Fact]
    public async Task CreateDeviceAsync_ConUbicacionInexistente_DebeFallar()
    {
        var request = NewDevice();
        request.LocationId = Guid.NewGuid();

        var result = await _service.CreateDeviceAsync(request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("UBICACION_INVALIDA", result.ErrorCode);
    }

    [Fact]
    public async Task GetDevicesAsync_FiltraPorTextoLibre()
    {
        await _service.CreateDeviceAsync(NewDevice(), CancellationToken.None);
        await _service.CreateDeviceAsync(new CreateDeviceRequest
        {
            Brand = "Apple",
            Model = "iPhone 14",
            Imei = "356938035643809",
            LocationId = SeedIds.Almacen,
            EntryDate = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc)
        }, CancellationToken.None);

        var resultado = await _service.GetDevicesAsync(new DeviceQuery { Search = "Apple" }, CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(1, resultado.Data!.TotalItems);
        Assert.Equal("Apple", resultado.Data.Items[0].Brand);
    }

    [Fact]
    public async Task GetDevicesAsync_AplicaPaginacion()
    {
        await _service.CreateDeviceAsync(new CreateDeviceRequest
        {
            Brand = "Xiaomi", Model = "Redmi Note 12", Imei = "490154203237518",
            LocationId = SeedIds.Almacen, EntryDate = DateTime.UtcNow
        }, CancellationToken.None);
        await _service.CreateDeviceAsync(new CreateDeviceRequest
        {
            Brand = "Motorola", Model = "Edge 40", Imei = "356938035643809",
            LocationId = SeedIds.Almacen, EntryDate = DateTime.UtcNow
        }, CancellationToken.None);

        var primera = await _service.GetDevicesAsync(new DeviceQuery { Page = 1, PageSize = 1 }, CancellationToken.None);

        Assert.Equal(2, primera.Data!.TotalItems);
        Assert.Single(primera.Data.Items);
        Assert.Equal(2, primera.Data.TotalPages);
    }

    [Fact]
    public async Task GetDeviceByIdAsync_ConIdInexistente_DebeDevolverNotFound()
    {
        var result = await _service.GetDeviceByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("EQUIPO_NO_ENCONTRADO", result.ErrorCode);
    }

    [Fact]
    public async Task UpdateDeviceAsync_ActualizaCamposPermitidos()
    {
        var creado = await _service.CreateDeviceAsync(NewDevice(), CancellationToken.None);

        var result = await _service.UpdateDeviceAsync(
            creado.Data!.Id,
            new UpdateDeviceRequest { Model = "Galaxy S23 Ultra", Observations = "Equipo reacondicionado" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Galaxy S23 Ultra", result.Data!.Model);
        Assert.Equal("Equipo reacondicionado", result.Data.Observations);
        Assert.Equal("Samsung", result.Data.Brand);
    }

    [Fact]
    public async Task UpdateDeviceAsync_ConImeiDeOtroEquipo_DebeFallar()
    {
        var primero = await _service.CreateDeviceAsync(NewDevice(), CancellationToken.None);
        var segundo = await _service.CreateDeviceAsync(new CreateDeviceRequest
        {
            Brand = "Apple", Model = "iPhone 14", Imei = "356938035643809",
            LocationId = SeedIds.Almacen, EntryDate = DateTime.UtcNow
        }, CancellationToken.None);

        var result = await _service.UpdateDeviceAsync(
            segundo.Data!.Id,
            new UpdateDeviceRequest { Imei = primero.Data!.Imei },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("IMEI_DUPLICADO", result.ErrorCode);
    }

    [Fact]
    public async Task DeleteDeviceAsync_EliminaElEquipoYSuHistorial()
    {
        var creado = await _service.CreateDeviceAsync(NewDevice(), CancellationToken.None);

        var result = await _service.DeleteDeviceAsync(creado.Data!.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, await _context.Devices.CountAsync());
        Assert.Equal(0, await _context.Movements.CountAsync());
    }

    [Fact]
    public async Task CreateMovementAsync_Traslado_ActualizaLaUbicacion()
    {
        var creado = await _service.CreateDeviceAsync(NewDevice(), CancellationToken.None);

        var result = await _service.CreateMovementAsync(new CreateMovementRequest
        {
            DeviceId = creado.Data!.Id,
            Type = MovementType.Traslado,
            ToLocationId = SeedIds.Soporte,
            Reason = "Equipo con pantalla danada"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var equipo = await _context.Devices.AsNoTracking().FirstAsync();
        Assert.Equal(SeedIds.Soporte, equipo.LocationId);
    }

    [Fact]
    public async Task CreateMovementAsync_TrasladoSinDestino_DebeFallar()
    {
        var creado = await _service.CreateDeviceAsync(NewDevice(), CancellationToken.None);

        var result = await _service.CreateMovementAsync(new CreateMovementRequest
        {
            DeviceId = creado.Data!.Id,
            Type = MovementType.Traslado
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("DESTINO_REQUERIDO", result.ErrorCode);
    }

    [Fact]
    public async Task CreateMovementAsync_BajaMarcaElEquipoComoDadoDeBaja()
    {
        var creado = await _service.CreateDeviceAsync(NewDevice(), CancellationToken.None);

        var result = await _service.CreateMovementAsync(new CreateMovementRequest
        {
            DeviceId = creado.Data!.Id,
            Type = MovementType.Baja,
            Reason = "Equipo quemado"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var equipo = await _context.Devices.AsNoTracking().FirstAsync();
        Assert.Equal(DeviceStatus.DeBaja, equipo.Status);
    }

    [Fact]
    public async Task CreateMovementAsync_DobleBaja_DebeFallar()
    {
        var creado = await _service.CreateDeviceAsync(NewDevice(), CancellationToken.None);
        await _service.CreateMovementAsync(new CreateMovementRequest
        {
            DeviceId = creado.Data!.Id,
            Type = MovementType.Baja
        }, CancellationToken.None);

        var segundo = await _service.CreateMovementAsync(new CreateMovementRequest
        {
            DeviceId = creado.Data!.Id,
            Type = MovementType.Baja
        }, CancellationToken.None);

        Assert.False(segundo.IsSuccess);
        Assert.Equal("EQUIPO_YA_DADO_DE_BAJA", segundo.ErrorCode);
    }

    [Fact]
    public async Task GetMovementsAsync_FiltraPorEquipo()
    {
        var creado = await _service.CreateDeviceAsync(NewDevice(), CancellationToken.None);
        await _service.CreateMovementAsync(new CreateMovementRequest
        {
            DeviceId = creado.Data!.Id,
            Type = MovementType.Traslado,
            ToLocationId = SeedIds.Soporte
        }, CancellationToken.None);

        var historial = await _service.GetMovementsAsync(creado.Data!.Id, CancellationToken.None);

        Assert.True(historial.IsSuccess);
        Assert.Equal(2, historial.Data!.Count);
        Assert.Equal(MovementType.Traslado, historial.Data[0].Type);
        Assert.Equal("Samsung Galaxy S22", historial.Data[0].DeviceName);
    }

    [Fact]
    public async Task GetStockReportAsync_AgrupaPorEstadoUbicacionYMarca()
    {
        await _service.CreateDeviceAsync(NewDevice(), CancellationToken.None);
        await _service.CreateDeviceAsync(new CreateDeviceRequest
        {
            Brand = "Samsung", Model = "Galaxy A54", Imei = "356938035643809",
            LocationId = SeedIds.Soporte, EntryDate = DateTime.UtcNow, PurchasePrice = 1500m
        }, CancellationToken.None);

        var reporte = await _service.GetStockReportAsync(CancellationToken.None);

        Assert.True(reporte.IsSuccess);
        Assert.Equal(2, reporte.Data!.TotalDevices);
        Assert.Equal(1500m, reporte.Data.TotalValue);
        Assert.Equal(2, reporte.Data.ByStatus.Sum(group => group.Count));
        Assert.Contains(reporte.Data.ByBrand, group => group.Label == "Samsung");
        Assert.Contains(reporte.Data.ByLocation, group => group.Label == "Sala de Soporte");
    }
}
