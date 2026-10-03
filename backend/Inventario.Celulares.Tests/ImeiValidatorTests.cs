using Inventario.Celulares.Core.Common;

namespace Inventario.Celulares.Tests;

/// <summary>
/// Pruebas del validador de IMEI con digito de control Luhn.
/// </summary>
public class ImeiValidatorTests
{
    /// <summary>IMEI de ejemplo valido, con digito verificador correcto.</summary>
    private const string ImeiValido = "490154203237518";

    [Theory]
    [InlineData(ImeiValido, true)]
    [InlineData("356938035643809", true)]
    [InlineData("490154203237519", false)]
    [InlineData("49015420323751", false)]
    [InlineData("4901542032375180", false)]
    [InlineData("49015420323751A", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void IsValid_DebeRespetarFormatoYVerificador(string? imei, bool esperado)
    {
        Assert.Equal(esperado, ImeiValidator.IsValid(imei));
    }

    [Theory]
    [InlineData("490154203237518", "490154203237518")]
    [InlineData("49-01542 0323-7518", "490154203237518")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Normalize_DebeDejarSoloDigitos(string? entrada, string esperado)
    {
        Assert.Equal(esperado, ImeiValidator.Normalize(entrada));
    }

    [Fact]
    public void Normalize_LuegoDeIsValid_DebeAceptarElImeiFormateado()
    {
        var normalizado = ImeiValidator.Normalize("49 01542-0323.7518");

        Assert.True(ImeiValidator.IsValid(normalizado));
    }
}
