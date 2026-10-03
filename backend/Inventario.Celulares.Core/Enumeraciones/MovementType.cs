namespace Inventario.Celulares.Core.Enumeraciones;

/// <summary>
/// Tipo de movimiento de inventario registrado para un equipo.
/// </summary>
public enum MovementType
{
    /// <summary>Ingreso del equipo al inventario.</summary>
    Ingreso = 1,

    /// <summary>Salida del equipo del inventario.</summary>
    Salida = 2,

    /// <summary>Traslado del equipo entre ubicaciones.</summary>
    Traslado = 3,

    /// <summary>Baja definitiva del equipo.</summary>
    Baja = 4
}
