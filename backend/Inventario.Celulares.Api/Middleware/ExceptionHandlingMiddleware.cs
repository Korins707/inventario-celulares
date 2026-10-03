using System.Net.Mime;
using System.Text.Json;
using Inventario.Celulares.Core.Common;

namespace Inventario.Celulares.Api.Middleware;

/// <summary>
/// Middleware que traduce excepciones no controladas a respuestas JSON.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Inicializa el middleware.
    /// </summary>
    /// <param name="next">Siguiente middleware de la canalizacion.</param>
    /// <param name="logger">Registrador de diagnostico.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta la canalizacion y captura las excepciones.
    /// </summary>
    /// <param name="context">Contexto de la solicitud.</param>
    /// <returns>Tarea asincrona de la canalizacion.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (DomainException exception)
        {
            _logger.LogWarning(exception, "Error de negocio controlado: {Code}", exception.Code);
            await WriteErrorAsync(context, exception.StatusCode, exception.Code, exception.Message).ConfigureAwait(false);
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(exception, "Argumento invalido en la solicitud");
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, "ARGUMENTO_INVALIDO", exception.Message)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error no controlado al procesar la solicitud");
            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "ERROR_INTERNO",
                "Ocurrio un error inesperado al procesar la solicitud.")
                .ConfigureAwait(false);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string code, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var payload = new ErrorResponse(code, message);
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, SerializerOptions)).ConfigureAwait(false);
    }
}

/// <summary>
/// Estructura de la respuesta de error de la API.
/// </summary>
/// <param name="Code">Codigo de error estable.</param>
/// <param name="Message">Mensaje legible para el usuario.</param>
public record ErrorResponse(string Code, string Message);
