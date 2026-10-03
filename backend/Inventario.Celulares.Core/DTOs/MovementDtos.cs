using System.ComponentModel.DataAnnotations;
using Inventario.Celulares.Core.Enumeraciones;

namespace Inventario.Celulares.Core.DTOs;

/// <summary>
/// Datos para registrar un movimiento de inventario.
/// </summary>
public class CreateMovementRequest
{
    /// <summary>Equipo afectado por el movimiento.</summary>
    [Required(ErrorMessage = "El equipo es obligatorio")]
    public Guid DeviceId { get; set; }

    /// <summary>Tipo de movimiento.</summary>
    [EnumDataType(typeof(MovementType), ErrorMessage = "El tipo de movimiento no es valido")]
    public MovementType Type { get; set; }

    /// <summary>Ubicacion de origen.</summary>
    public Guid? FromLocationId { get; set; }

    /// <summary>Ubicacion de destino.</summary>
    public Guid? ToLocationId { get; set; }

    /// <summary>Fecha del movimiento; si se omite se usa el instante actual.</summary>
    public DateTime? OccurredAt { get; set; }

    /// <summary>Motivo del movimiento.</summary>
    [StringLength(300, ErrorMessage = "El motivo no puede superar 300 caracteres")]
    public string? Reason { get; set; }
}

/// <summary>
/// Vista de un movimiento con los nombres de las ubicaciones.
/// </summary>
public class MovementResponse
{
    /// <summary>Identificador unico del movimiento.</summary>
    public Guid Id { get; set; }

    /// <summary>Equipo afectado.</summary>
    public Guid DeviceId { get; set; }

    /// <summary>Marca y modelo del equipo.</summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>IMEI del equipo.</summary>
    public string Imei { get; set; } = string.Empty;

    /// <summary>Tipo de movimiento.</summary>
    public MovementType Type { get; set; }

    /// <summary>Nombre del tipo de movimiento.</summary>
    public string TypeName => Type.ToString();

    /// <summary>Ubicacion de origen.</summary>
    public string? FromLocationName { get; set; }

    /// <summary>Ubicacion de destino.</summary>
    public string? ToLocationName { get; set; }

    /// <summary>Fecha del movimiento.</summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>Motivo del movimiento.</summary>
    public string? Reason { get; set; }

    /// <summary>Usuario que registro el movimiento.</summary>
    public string? RegisteredBy { get; set; }
}
