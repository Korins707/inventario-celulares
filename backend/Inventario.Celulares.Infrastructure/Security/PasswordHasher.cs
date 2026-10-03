using Inventario.Celulares.Core.Interfaces;

namespace Inventario.Celulares.Infrastructure.Security;

/// <summary>
/// Implementacion del hash de contrasenas basado en PBKDF2-HMAC-SHA256.
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    /// <inheritdoc />
    public void CreateHash(string password, out string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(SaltSize);
        var key = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            password, salt, Iterations, System.Security.Cryptography.HashAlgorithmName.SHA256, KeySize);

        hash = FormatHash(salt, key);
    }

    /// <inheritdoc />
    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        var parts = hash.Split('.');
        if (parts.Length != 2)
        {
            return false;
        }

        byte[] salt;
        byte[] key;
        try
        {
            salt = Convert.FromBase64String(parts[0]);
            key = Convert.FromBase64String(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        var derived = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            password, salt, Iterations, System.Security.Cryptography.HashAlgorithmName.SHA256, key.Length);

        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(derived, key);
    }

    private static string FormatHash(byte[] salt, byte[] key) =>
        string.Join('.', Convert.ToBase64String(salt), Convert.ToBase64String(key));
}
