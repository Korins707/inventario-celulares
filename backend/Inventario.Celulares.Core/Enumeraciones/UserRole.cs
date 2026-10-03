namespace Inventario.Celulares.Core.Enumeraciones;

/// <summary>
/// Rol asignado a un usuario del sistema.
/// </summary>
public enum UserRole
{
    /// <summary>Operador con permisos sobre su propio inventario.</summary>
    Operador = 1,

    /// <summary>Administrador con permisos globales.</summary>
    Administrador = 2
}
