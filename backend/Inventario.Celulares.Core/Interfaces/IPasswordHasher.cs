namespace Inventario.Celulares.Core.Interfaces;

/// <summary>
/// Contrato para el hash y verificacion de contrasenas.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Genera el hash de una contrasena.</summary>
    /// <param name="password">Contrasena en texto plano.</param>
    /// <param name="hash">Hash resultante con sal incluida.</param>
    void CreateHash(string password, out string hash);

    /// <summary>Verifica una contrasena contra su hash.</summary>
    /// <param name="password">Contrasena en texto plano.</param>
    /// <param name="hash">Hash almacenado.</param>
    /// <returns><c>true</c> cuando la contrasena coincide.</returns>
    bool Verify(string password, string hash);
}
