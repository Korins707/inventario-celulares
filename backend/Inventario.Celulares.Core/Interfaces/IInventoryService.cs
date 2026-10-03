using Inventario.Celulares.Core.Common;
using Inventario.Celulares.Core.DTOs;
using Inventario.Celulares.Core.Enumeraciones;

namespace Inventario.Celulares.Core.Interfaces;

/// <summary>
/// Servicio de aplicacion para la gestion del inventario de equipos.
/// </summary>
public interface IInventoryService
{
    /// <summary>Registra un nuevo equipo.</summary>
    /// <param name="request">Datos del equipo.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Equipo creado.</returns>
    Task<Result<DeviceResponse>> CreateDeviceAsync(CreateDeviceRequest request, CancellationToken cancellationToken);

    /// <summary>Lista equipos aplicando filtros y paginacion.</summary>
    /// <param name="query">Criterios de busqueda.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Pagina de equipos.</returns>
    Task<Result<PagedResult<DeviceResponse>>> GetDevicesAsync(DeviceQuery query, CancellationToken cancellationToken);

    /// <summary>Obtiene un equipo por su identificador.</summary>
    /// <param name="id">Identificador del equipo.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Equipo encontrado.</returns>
    Task<Result<DeviceResponse>> GetDeviceByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Actualiza los datos de un equipo.</summary>
    /// <param name="id">Identificador del equipo.</param>
    /// <param name="request">Campos a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Equipo actualizado.</returns>
    Task<Result<DeviceResponse>> UpdateDeviceAsync(Guid id, UpdateDeviceRequest request, CancellationToken cancellationToken);

    /// <summary>Elimina logicamente un equipo.</summary>
    /// <param name="id">Identificador del equipo.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Resultado de la operacion.</returns>
    Task<Result<bool>> DeleteDeviceAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Registra un movimiento de inventario.</summary>
    /// <param name="request">Datos del movimiento.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Movimiento creado.</returns>
    Task<Result<MovementResponse>> CreateMovementAsync(CreateMovementRequest request, CancellationToken cancellationToken);

    /// <summary>Obtiene el historial de movimientos, opcionalmente de un equipo.</summary>
    /// <param name="deviceId">Equipo a filtrar, o nulo para todos.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Lista de movimientos.</returns>
    Task<Result<IReadOnlyList<MovementResponse>>> GetMovementsAsync(Guid? deviceId, CancellationToken cancellationToken);

    /// <summary>Genera el reporte de stock actual.</summary>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Reporte con totales y agrupaciones.</returns>
    Task<Result<StockReportResponse>> GetStockReportAsync(CancellationToken cancellationToken);
}
