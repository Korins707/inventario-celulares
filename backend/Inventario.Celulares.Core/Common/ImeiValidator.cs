namespace Inventario.Celulares.Core.Common;

/// <summary>
/// Validador del codigo IMEI de 15 digitos mediante el algoritmo de Luhn.
/// </summary>
public static class ImeiValidator
{
    /// <summary>Longitud esperada de un IMEI.</summary>
    public const int ImeiLength = 15;

    /// <summary>
    /// Indica si el valor cumple longitud, formato y digito verificador.
    /// </summary>
    /// <param name="imei">Texto a validar.</param>
    /// <returns><c>true</c> cuando el IMEI es valido.</returns>
    public static bool IsValid(string? imei)
    {
        if (string.IsNullOrWhiteSpace(imei) || imei.Length != ImeiLength)
        {
            return false;
        }

        var digits = new int[ImeiLength];
        for (var index = 0; index < ImeiLength; index++)
        {
            var character = imei[index];
            if (!char.IsDigit(character))
            {
                return false;
            }

            digits[index] = character - '0';
        }

        // Los ultimos dos digitos son el codigo de fabricante + verificador.
        for (var index = 0; index < 14; index++)
        {
            if (index % 2 == 1)
            {
                digits[index] *= 2;
                if (digits[index] > 9)
                {
                    digits[index] -= 9;
                }
            }
        }

        var sum = 0;
        foreach (var digit in digits)
        {
            sum += digit;
        }

        return sum % 10 == 0;
    }

    /// <summary>
    /// Normaliza un IMEI quitando espacios y guiones.
    /// </summary>
    /// <param name="imei">Texto de entrada.</param>
    /// <returns>Solo los digitos del texto, o cadena vacia.</returns>
    public static string Normalize(string? imei)
    {
        if (string.IsNullOrWhiteSpace(imei))
        {
            return string.Empty;
        }

        return new string(imei.Where(char.IsDigit).ToArray());
    }
}
