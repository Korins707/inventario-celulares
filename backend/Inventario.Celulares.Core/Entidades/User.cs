using Inventario.Celulares.Core.Enumeraciones;

namespace Inventario.Celulares.Core.Entidades;

/// <summary>
/// Usuario con acceso al sistema de inventario.
/// </summary>
public class User
{
    /// <summary>Identificador unico del usuario.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Nombre de usuario unico.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Hash de la contrasena (nunca se almacena en texto plano).</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Nombre completo del usuario.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Rol del usuario.</summary>
    public UserRole Role { get; set; } = UserRole.Operador;

    /// <summary>Indica si el usuario puede autenticarse.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Fecha de creacion del usuario.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
