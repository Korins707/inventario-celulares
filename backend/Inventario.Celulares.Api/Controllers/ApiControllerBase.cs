using System.Net.Mime;
using Inventario.Celulares.Api.Middleware;
using Inventario.Celulares.Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Celulares.Api.Controllers;

/// <summary>
/// Base de los controladores, con traduccion de resultados a respuestas HTTP.
/// </summary>
[ApiController]
[Produces(MediaTypeNames.Application.Json)]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Traduce un resultado de dominio a una respuesta HTTP.
    /// </summary>
    /// <typeparam name="T">Tipo del valor transportado.</typeparam>
    /// <param name="result">Resultado de la operacion.</param>
    /// <returns>Respuesta HTTP correspondiente.</returns>
    protected ActionResult<T> FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        var statusCode = result.ErrorCode switch
        {
            "EQUIPO_NO_ENCONTRADO" => StatusCodes.Status404NotFound,
            "IMEI_DUPLICADO" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return StatusCode(statusCode, new ErrorResponse(result.ErrorCode ?? "ERROR", result.ErrorMessage ?? string.Empty));
    }

    /// <summary>
    /// Traduce un resultado sin valor a una respuesta HTTP.
    /// </summary>
    /// <param name="result">Resultado de la operacion.</param>
    /// <returns>Respuesta HTTP correspondiente.</returns>
    protected ActionResult FromResultNoContent(Result<bool> result)
    {
        if (result.IsSuccess)
        {
            return NoContent();
        }

        var statusCode = result.ErrorCode == "EQUIPO_NO_ENCONTRADO"
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status400BadRequest;

        return StatusCode(statusCode, new ErrorResponse(result.ErrorCode ?? "ERROR", result.ErrorMessage ?? string.Empty));
    }
}
