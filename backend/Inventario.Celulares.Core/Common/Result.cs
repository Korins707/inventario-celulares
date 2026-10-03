namespace Inventario.Celulares.Core.Common;

/// <summary>
/// Resultado estandar de una operacion de negocio.
/// </summary>
/// <typeparam name="T">Tipo del valor transportado.</typeparam>
public class Result<T>
{
    /// <summary>Indica si la operacion fue exitosa.</summary>
    public bool IsSuccess { get; }

    /// <summary>Valor transportado cuando la operacion fue exitosa.</summary>
    public T? Data { get; }

    /// <summary>Codigo de error cuando la operacion fallo.</summary>
    public string? ErrorCode { get; }

    /// <summary>Mensaje de error cuando la operacion fallo.</summary>
    public string? ErrorMessage { get; }

    private Result(bool isSuccess, T? data, string? errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    /// <summary>Crea un resultado exitoso.</summary>
    /// <param name="data">Valor a transportar.</param>
    /// <returns>Resultado exitoso.</returns>
    public static Result<T> Success(T data) => new(true, data, null, null);

    /// <summary>Crea un resultado fallido.</summary>
    /// <param name="code">Codigo de error.</param>
    /// <param name="message">Mensaje de error.</param>
    /// <returns>Resultado con error.</returns>
    public static Result<T> Failure(string code, string message) => new(false, default, code, message);
}
