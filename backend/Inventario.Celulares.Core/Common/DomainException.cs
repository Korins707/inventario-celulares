namespace Inventario.Celulares.Core.Common;

/// <summary>
/// Excepcion de negocio controlada, con codigo HTTP asociado.
/// </summary>
public class DomainException : Exception
{
    /// <summary>Codigo de error estable para el cliente.</summary>
    public string Code { get; }

    /// <summary>Codigo HTTP que debe devolver la API.</summary>
    public int StatusCode { get; }

    /// <summary>
    /// Inicializa una excepcion de dominio.
    /// </summary>
    /// <param name="code">Codigo de error estable.</param>
    /// <param name="message">Mensaje legible para el usuario.</param>
    /// <param name="statusCode">Codigo HTTP asociado.</param>
    public DomainException(string code, string message, int statusCode = 400)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
