using Inventario.Celulares.Core.DTOs;
using Inventario.Celulares.Core.Interfaces;
using Inventario.Celulares.Api.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Celulares.Api.Controllers;

/// <summary>
/// Endpoints de movimientos de inventario.
/// </summary>
/// <summary>Ruta base del recurso de movements.</summary>
[Route("movements")]
public class MovementsController : ApiControllerBase
{

    private readonly IInventoryService _inventoryService;

    /// <summary>
    /// Inicializa el controlador con el servicio de inventario.
    /// </summary>
    /// <param name="inventoryService">Servicio de inventario.</param>
    public MovementsController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>
    /// Registra un movimiento de inventario para un equipo.
    /// </summary>
    /// <param name="request">Datos del movimiento.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Movimiento registrado.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(MovementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovementResponse>> Create(
        [FromBody] CreateMovementRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _inventoryService.CreateMovementAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return FromResult(result);
        }

        return CreatedAtAction(nameof(History), new { deviceId = result.Data!.DeviceId }, result.Data);
    }

    /// <summary>
    /// Obtiene el historial de movimientos, filtrable por equipo.
    /// </summary>
    /// <param name="deviceId">Equipo a filtrar; si se omite devuelve todos.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Lista de movimientos.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MovementResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MovementResponse>>> History(
        [FromQuery] Guid? deviceId,
        CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetMovementsAsync(deviceId, cancellationToken);
        return FromResult(result);
    }
}
