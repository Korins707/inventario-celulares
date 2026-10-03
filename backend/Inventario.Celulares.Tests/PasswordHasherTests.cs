using Inventario.Celulares.Infrastructure.Security;

namespace Inventario.Celulares.Tests;

/// <summary>
/// Pruebas del hash de contrasenas PBKDF2.
/// </summary>
public class PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void CreateHash_GeneraHashDistintoDeLaContrasena()
    {
        _hasher.CreateHash("claveSegura123", out var hash);

        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.DoesNotContain("claveSegura123", hash, StringComparison.Ordinal);
    }

    [Fact]
    public void Verify_ContrasenaCorrecta_DebeDevolverTrue()
    {
        _hasher.CreateHash("claveSegura123", out var hash);

        Assert.True(_hasher.Verify("claveSegura123", hash));
    }

    [Theory]
    [InlineData("claveIncorrecta")]
    [InlineData("")]
    public void Verify_ContrasenaIncorrecta_DebeDevolverFalse(string intento)
    {
        _hasher.CreateHash("claveSegura123", out var hash);

        Assert.False(_hasher.Verify(intento, hash));
    }

    [Fact]
    public void Verify_ConHashMalformado_DebeDevolverFalseYNoLanzar()
    {
        Assert.False(_hasher.Verify("clave", "no-es-un-hash-valido"));
        Assert.False(_hasher.Verify("clave", "###.###"));
    }

    [Fact]
    public void Verify_LaMismaContrasenaGeneraHashesDistintosPorSal()
    {
        _hasher.CreateHash("mismaClave", out var primerHash);
        _hasher.CreateHash("mismaClave", out var segundoHash);

        Assert.NotEqual(primerHash, segundoHash);
        Assert.True(_hasher.Verify("mismaClave", primerHash));
        Assert.True(_hasher.Verify("mismaClave", segundoHash));
    }
}
