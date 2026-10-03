using Inventario.Celulares.Core.Enumeraciones;

namespace Inventario.Celulares.Core.Entidades;

/// <summary>
/// Movimiento de inventario asociado a un equipo.
/// </summary>
public class Movement
{
    /// <summary>Identificador unico del movimiento.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Identificador del equipo movers.</summary>
    public Guid DeviceId { get; set; }

    /// <summary>Equipo afectado por el movimiento.</summary>
    public Device? Device { get; set; }

    /// <summary>Tipo de movimiento registrado.</summary>
    public MovementType Type { get; set; }

    /// <summary>Ubicacion de origen, nula cuando el equipo ingresa al inventario.</summary>
    public Guid? FromLocationId { get; set; }

    /// <summary>Ubicacion de origen.</summary>
    public Location? FromLocation { get; set; }

    /// <summary>Ubicacion de destino, nula en bajas y salidas sin destino.</summary>
    public Guid? ToLocationId { get; set; }

    /// <summary>Ubicacion de destino.</summary>
    public Location? ToLocation { get; set; }

    /// <summary>Fecha en que se ejecuto el movimiento.</summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>Motivo o detalle del movimiento.</summary>
    public string? Reason { get; set; }

    /// <summary>Usuario que registro el movimiento.</summary>
    public Guid? RegisteredById { get; set; }

    /// <summary>Fecha de creacion del registro.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
