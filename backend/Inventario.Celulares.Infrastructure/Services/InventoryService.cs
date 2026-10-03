using Inventario.Celulares.Core.Common;
using Inventario.Celulares.Core.DTOs;
using Inventario.Celulares.Core.Entidades;
using Inventario.Celulares.Core.Enumeraciones;
using Inventario.Celulares.Core.Interfaces;
using Inventario.Celulares.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Celulares.Infrastructure.Services;

/// <summary>
/// Servicio de inventario implementado sobre EF Core.
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly InventarioDbContext _context;

    /// <summary>
    /// Inicializa el servicio con el contexto de datos.
    /// </summary>
    /// <param name="context">Contexto de datos.</param>
    public InventoryService(InventarioDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Result<DeviceResponse>> CreateDeviceAsync(
        CreateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var imei = ImeiValidator.Normalize(request.Imei);
        if (!ImeiValidator.IsValid(imei))
        {
            return Result<DeviceResponse>.Failure(
                "IMEI_INVALIDO",
                "El IMEI debe tener 15 digitos y un digito verificador valido.");
        }

        var duplicated = await _context.Devices
            .AnyAsync(device => device.Imei == imei, cancellationToken)
            .ConfigureAwait(false);

        if (duplicated)
        {
            return Result<DeviceResponse>.Failure(
                "IMEI_DUPLICADO",
                $"Ya existe un equipo registrado con el IMEI {imei}.");
        }

        var locationExists = await _context.Locations
            .AnyAsync(location => location.Id == request.LocationId, cancellationToken)
            .ConfigureAwait(false);

        if (!locationExists)
        {
            return Result<DeviceResponse>.Failure(
                "UBICACION_INVALIDA",
                "La ubicacion indicada no existe.");
        }

        var device = new Device
        {
            Brand = request.Brand.Trim(),
            Model = request.Model.Trim(),
            Imei = imei,
            Status = request.Status,
            LocationId = request.LocationId,
            EntryDate = request.EntryDate,
            Observations = request.Observations,
            PurchasePrice = request.PurchasePrice
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await ApplyInitialMovementAsync(device, cancellationToken).ConfigureAwait(false);

        return Result<DeviceResponse>.Success(await MapAsync(device, cancellationToken).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<DeviceResponse>>> GetDevicesAsync(
        DeviceQuery query,
        CancellationToken cancellationToken)
    {
        var queryable = _context.Devices.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            queryable = queryable.Where(device =>
                device.Imei.ToLower().Contains(term)
                || device.Brand.ToLower().Contains(term)
                || device.Model.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(query.Brand))
        {
            var brand = query.Brand.Trim().ToLower();
            queryable = queryable.Where(device => device.Brand.ToLower() == brand);
        }

        if (!string.IsNullOrWhiteSpace(query.Model))
        {
            var model = query.Model.Trim().ToLower();
            queryable = queryable.Where(device => device.Model.ToLower() == model);
        }

        if (query.Status.HasValue)
        {
            queryable = queryable.Where(device => device.Status == query.Status.Value);
        }

        if (query.LocationId.HasValue)
        {
            queryable = queryable.Where(device => device.LocationId == query.LocationId.Value);
        }

        var total = await queryable.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await queryable
            .OrderBy(device => device.Brand).ThenBy(device => device.Model)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Include(device => device.Location)
            .Select(device => new DeviceResponse
            {
                Id = device.Id,
                Brand = device.Brand,
                Model = device.Model,
                Imei = device.Imei,
                Status = device.Status,
                LocationId = device.LocationId,
                LocationName = device.Location!.Name,
                EntryDate = device.EntryDate,
                Observations = device.Observations,
                PurchasePrice = device.PurchasePrice,
                CreatedAt = device.CreatedAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<PagedResult<DeviceResponse>>.Success(new PagedResult<DeviceResponse>
        {
            Items = items,
            TotalItems = total,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    /// <inheritdoc />
    public async Task<Result<DeviceResponse>> GetDeviceByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .AsNoTracking()
            .Include(item => item.Location)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (device is null)
        {
            return Result<DeviceResponse>.Failure("EQUIPO_NO_ENCONTRADO", "El equipo no existe.");
        }

        return Result<DeviceResponse>.Success(ToResponse(device));
    }

    /// <inheritdoc />
    public async Task<Result<DeviceResponse>> UpdateDeviceAsync(
        Guid id,
        UpdateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(item => item.Location)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (device is null)
        {
            return Result<DeviceResponse>.Failure("EQUIPO_NO_ENCONTRADO", "El equipo no existe.");
        }

        if (!string.IsNullOrWhiteSpace(request.Imei))
        {
            var imei = ImeiValidator.Normalize(request.Imei);
            if (!ImeiValidator.IsValid(imei))
            {
                return Result<DeviceResponse>.Failure(
                    "IMEI_INVALIDO",
                    "El IMEI debe tener 15 digitos y un digito verificador valido.");
            }

            if (imei != device.Imei)
            {
                var duplicated = await _context.Devices
                    .AnyAsync(item => item.Imei == imei && item.Id != id, cancellationToken)
                    .ConfigureAwait(false);

                if (duplicated)
                {
                    return Result<DeviceResponse>.Failure(
                        "IMEI_DUPLICADO",
                        $"Ya existe un equipo registrado con el IMEI {imei}.");
                }

                device.Imei = imei;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Brand))
        {
            device.Brand = request.Brand.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Model))
        {
            device.Model = request.Model.Trim();
        }

        if (request.Status.HasValue)
        {
            device.Status = request.Status.Value;
        }

        if (request.LocationId.HasValue && request.LocationId.Value != device.LocationId)
        {
            var locationExists = await _context.Locations
                .AnyAsync(location => location.Id == request.LocationId.Value, cancellationToken)
                .ConfigureAwait(false);

            if (!locationExists)
            {
                return Result<DeviceResponse>.Failure(
                    "UBICACION_INVALIDA",
                    "La ubicacion indicada no existe.");
            }

            device.LocationId = request.LocationId.Value;
        }

        if (request.Observations is not null)
        {
            device.Observations = request.Observations;
        }

        if (request.PurchasePrice.HasValue)
        {
            device.PurchasePrice = request.PurchasePrice;
        }

        device.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<DeviceResponse>.Success(ToResponse(device));
    }

    /// <inheritdoc />
    public async Task<Result<bool>> DeleteDeviceAsync(Guid id, CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (device is null)
        {
            return Result<bool>.Failure("EQUIPO_NO_ENCONTRADO", "El equipo no existe.");
        }

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<bool>.Success(true);
    }

    /// <inheritdoc />
    public async Task<Result<MovementResponse>> CreateMovementAsync(
        CreateMovementRequest request,
        CancellationToken cancellationToken)
    {
        var device = await _context.Devices
            .Include(item => item.Location)
            .FirstOrDefaultAsync(item => item.Id == request.DeviceId, cancellationToken)
            .ConfigureAwait(false);

        if (device is null)
        {
            return Result<MovementResponse>.Failure("EQUIPO_NO_ENCONTRADO", "El equipo no existe.");
        }

        if (request.Type == MovementType.Traslado && !request.ToLocationId.HasValue)
        {
            return Result<MovementResponse>.Failure(
                "DESTINO_REQUERIDO",
                "Un traslado necesita una ubicacion de destino.");
        }

        if (request.Type == MovementType.Baja && device.Status == DeviceStatus.DeBaja)
        {
            return Result<MovementResponse>.Failure(
                "EQUIPO_YA_DADO_DE_BAJA",
                "El equipo ya se encuentra dado de baja.");
        }

        if (request.FromLocationId.HasValue)
        {
            var originExists = await _context.Locations
                .AnyAsync(location => location.Id == request.FromLocationId.Value, cancellationToken)
                .ConfigureAwait(false);

            if (!originExists)
            {
                return Result<MovementResponse>.Failure("UBICACION_INVALIDA", "La ubicacion de origen no existe.");
            }
        }

        if (request.ToLocationId.HasValue)
        {
            var destinationExists = await _context.Locations
                .AnyAsync(location => location.Id == request.ToLocationId.Value, cancellationToken)
                .ConfigureAwait(false);

            if (!destinationExists)
            {
                return Result<MovementResponse>.Failure("UBICACION_INVALIDA", "La ubicacion de destino no existe.");
            }
        }

        var movement = new Movement
        {
            DeviceId = device.Id,
            Type = request.Type,
            FromLocationId = request.FromLocationId ?? device.LocationId,
            ToLocationId = request.ToLocationId,
            OccurredAt = request.OccurredAt ?? DateTime.UtcNow,
            Reason = request.Reason
        };

        switch (request.Type)
        {
            case MovementType.Traslado:
                device.LocationId = request.ToLocationId!.Value;
                device.UpdatedAt = DateTime.UtcNow;
                break;
            case MovementType.Salida:
                device.Status = DeviceStatus.Asignado;
                device.UpdatedAt = DateTime.UtcNow;
                break;
            case MovementType.Baja:
                device.Status = DeviceStatus.DeBaja;
                device.UpdatedAt = DateTime.UtcNow;
                break;
            case MovementType.Ingreso:
            default:
                break;
        }

        _context.Movements.Add(movement);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<MovementResponse>.Success(await MapMovementAsync(movement, cancellationToken).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<MovementResponse>>> GetMovementsAsync(
        Guid? deviceId,
        CancellationToken cancellationToken)
    {
        var queryable = _context.Movements.AsNoTracking().AsQueryable();

        if (deviceId.HasValue)
        {
            queryable = queryable.Where(movement => movement.DeviceId == deviceId.Value);
        }

        var movements = await queryable
            .OrderByDescending(movement => movement.OccurredAt)
            .Include(movement => movement.Device)
            .Include(movement => movement.FromLocation)
            .Include(movement => movement.ToLocation)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = movements.Select(movement => new MovementResponse
        {
            Id = movement.Id,
            DeviceId = movement.DeviceId,
            DeviceName = movement.Device is null
                ? "Equipo eliminado"
                : $"{movement.Device.Brand} {movement.Device.Model}",
            Imei = movement.Device?.Imei ?? string.Empty,
            Type = movement.Type,
            FromLocationName = movement.FromLocation?.Name,
            ToLocationName = movement.ToLocation?.Name,
            OccurredAt = movement.OccurredAt,
            Reason = movement.Reason
        }).ToList();

        return Result<IReadOnlyList<MovementResponse>>.Success(result);
    }

    /// <inheritdoc />
    public async Task<Result<StockReportResponse>> GetStockReportAsync(CancellationToken cancellationToken)
    {
        var devices = await _context.Devices
            .AsNoTracking()
            .Include(device => device.Location)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var movements = await _context.Movements
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var report = new StockReportResponse
        {
            TotalDevices = devices.Count,
            TotalValue = devices.Sum(device => device.PurchasePrice ?? 0m),
            ByStatus = devices
                .GroupBy(device => device.Status.ToString())
                .Select(group => new GroupCountResponse { Label = group.Key, Count = group.Count() })
                .OrderByDescending(group => group.Count)
                .ToList(),
            ByLocation = devices
                .GroupBy(device => device.Location?.Name ?? "Sin ubicacion")
                .Select(group => new GroupCountResponse { Label = group.Key, Count = group.Count() })
                .OrderByDescending(group => group.Count)
                .ToList(),
            ByBrand = devices
                .GroupBy(device => device.Brand)
                .Select(group => new GroupCountResponse { Label = group.Key, Count = group.Count() })
                .OrderByDescending(group => group.Count)
                .ToList(),
            ByMovementType = movements
                .GroupBy(movement => movement.Type.ToString())
                .Select(group => new GroupCountResponse { Label = group.Key, Count = group.Count() })
                .OrderByDescending(group => group.Count)
                .ToList()
        };

        return Result<StockReportResponse>.Success(report);
    }

    private async Task ApplyInitialMovementAsync(Device device, CancellationToken cancellationToken)
    {
        var movement = new Movement
        {
            DeviceId = device.Id,
            Type = MovementType.Ingreso,
            ToLocationId = device.LocationId,
            OccurredAt = device.EntryDate,
            Reason = "Ingreso inicial al inventario"
        };

        _context.Movements.Add(movement);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<DeviceResponse> MapAsync(Device device, CancellationToken cancellationToken)
    {
        var locationName = await _context.Locations
            .Where(location => location.Id == device.LocationId)
            .Select(location => location.Name)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        device.Location ??= new Location { Id = device.LocationId, Name = locationName ?? string.Empty };

        return ToResponse(device);
    }

    private async Task<MovementResponse> MapMovementAsync(Movement movement, CancellationToken cancellationToken)
    {
        var details = await _context.Devices
            .Where(device => device.Id == movement.DeviceId)
            .Select(device => new { device.Brand, device.Model, device.Imei })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var originName = movement.FromLocationId.HasValue
            ? await LocationNameAsync(movement.FromLocationId.Value, cancellationToken).ConfigureAwait(false)
            : null;

        var destinationName = movement.ToLocationId.HasValue
            ? await LocationNameAsync(movement.ToLocationId.Value, cancellationToken).ConfigureAwait(false)
            : null;

        return new MovementResponse
        {
            Id = movement.Id,
            DeviceId = movement.DeviceId,
            DeviceName = details is null ? "Equipo eliminado" : $"{details.Brand} {details.Model}",
            Imei = details?.Imei ?? string.Empty,
            Type = movement.Type,
            FromLocationName = originName,
            ToLocationName = destinationName,
            OccurredAt = movement.OccurredAt,
            Reason = movement.Reason
        };
    }

    private Task<string?> LocationNameAsync(Guid locationId, CancellationToken cancellationToken) =>
        _context.Locations
            .Where(location => location.Id == locationId)
            .Select(location => location.Name)
            .FirstOrDefaultAsync(cancellationToken);

    private static DeviceResponse ToResponse(Device device) => new()
    {
        Id = device.Id,
        Brand = device.Brand,
        Model = device.Model,
        Imei = device.Imei,
        Status = device.Status,
        LocationId = device.LocationId,
        LocationName = device.Location?.Name ?? string.Empty,
        EntryDate = device.EntryDate,
        Observations = device.Observations,
        PurchasePrice = device.PurchasePrice,
        CreatedAt = device.CreatedAt
    };
}
