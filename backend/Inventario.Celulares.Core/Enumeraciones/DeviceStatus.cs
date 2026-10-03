namespace Inventario.Celulares.Core.Enumeraciones;

/// <summary>
/// Estado actual de un equipo celular dentro del inventario.
/// </summary>
public enum DeviceStatus
{
    /// <summary>Equipo en almacén, listo para ser asignado.</summary>
    Disponible = 1,

    /// <summary>Equipo entregado a un usuario o area.</summary>
    Asignado = 2,

    /// <summary>Equipo en reparación o mantenimiento.</summary>
    EnMantenimiento = 3,

    /// <summary>Equipo dado de baja, fuera del inventario operativo.</summary>
    DeBaja = 4
}
