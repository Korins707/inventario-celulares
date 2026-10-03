using Inventario.Celulares.Core.DTOs;
using Inventario.Celulares.Core.Interfaces;
using Inventario.Celulares.Api.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Celulares.Api.Controllers;

/// <summary>
/// Endpoints de reportes del inventario.
/// </summary>
/// <summary>Ruta base del recurso de reports.</summary>
[Route("reports")]
public class ReportsController : ApiControllerBase
{

    private readonly IInventoryService _inventoryService;

    /// <summary>
    /// Inicializa el controlador con el servicio de inventario.
    /// </summary>
    /// <param name="inventoryService">Servicio de inventario.</param>
    public ReportsController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>
    /// Genera el reporte de stock actual agrupado por estado, ubicacion y marca.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Reporte de stock.</returns>
    [HttpGet("stock")]
    [ProducesResponseType(typeof(StockReportResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StockReportResponse>> Stock(CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetStockReportAsync(cancellationToken);
        return FromResult(result);
    }
}
