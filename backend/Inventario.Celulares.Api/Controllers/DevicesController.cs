using Inventario.Celulares.Core.DTOs;
using Inventario.Celulares.Core.Interfaces;
using Inventario.Celulares.Api.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Celulares.Api.Controllers;

/// <summary>
/// Endpoints de registro y gestion de equipos celulares.
/// </summary>
/// <summary>Ruta base del recurso de devices.</summary>
[Route("devices")]
public class DevicesController : ApiControllerBase
{

    private readonly IInventoryService _inventoryService;

    /// <summary>
    /// Inicializa el controlador con el servicio de inventario.
    /// </summary>
    /// <param name="inventoryService">Servicio de inventario.</param>
    public DevicesController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>
    /// Registra un nuevo equipo celular.
    /// </summary>
    /// <param name="request">Datos del equipo a registrar.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Equipo creado.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DeviceResponse>> Create(
        [FromBody] CreateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _inventoryService.CreateDeviceAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return FromResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Lista equipos celulares con filtros y paginacion.
    /// </summary>
    /// <param name="query">Criterios de busqueda.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Pagina de equipos.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<DeviceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DeviceResponse>>> List(
        [FromQuery] DeviceQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetDevicesAsync(query, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Obtiene el detalle de un equipo.
    /// </summary>
    /// <param name="id">Identificador del equipo.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Detalle del equipo.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetDeviceByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Actualiza la informacion de un equipo.
    /// </summary>
    /// <param name="id">Identificador del equipo.</param>
    /// <param name="request">Campos a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Equipo actualizado.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceResponse>> Update(
        Guid id,
        [FromBody] UpdateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _inventoryService.UpdateDeviceAsync(id, request, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Elimina un equipo del inventario.
    /// </summary>
    /// <param name="id">Identificador del equipo.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Respuesta sin contenido cuando la operacion es exitosa.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.DeleteDeviceAsync(id, cancellationToken);
        return FromResultNoContent(result);
    }
}
