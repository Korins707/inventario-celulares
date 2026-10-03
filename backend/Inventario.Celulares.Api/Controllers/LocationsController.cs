using Inventario.Celulares.Core.Entidades;
using Inventario.Celulares.Infrastructure.Persistence;
using Inventario.Celulares.Api.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Celulares.Api.Controllers;

/// <summary>
/// Endpoints de consulta de ubicaciones.
/// </summary>
/// <summary>Ruta base del recurso de locations.</summary>
[Route("locations")]
public class LocationsController : ApiControllerBase
{

    private readonly InventarioDbContext _context;

    /// <summary>
    /// Inicializa el controlador con el contexto de datos.
    /// </summary>
    /// <param name="context">Contexto de datos.</param>
    public LocationsController(InventarioDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista las ubicaciones registradas.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    /// <returns>Lista de ubicaciones.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Location>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Location>>> List(CancellationToken cancellationToken)
    {
        var locations = await _context.Locations
            .AsNoTracking()
            .OrderBy(location => location.Name)
            .Select(location => new Location
            {
                Id = location.Id,
                Name = location.Name,
                Code = location.Code,
                Description = location.Description
            })
            .ToListAsync(cancellationToken);

        return Ok(locations);
    }
}
