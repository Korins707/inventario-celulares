using Inventario.Celulares.Core.Enumeraciones;

namespace Inventario.Celulares.Core.Entidades;

/// <summary>
/// Equipo celular registrado en el inventario.
/// </summary>
public class Device
{
    /// <summary>Identificador unico del equipo.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Marca del fabricante (Samsung, Apple, Xiaomi...).</summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>Modelo comercial del equipo.</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Codigo IMEI de 15 digitos, unico en todo el inventario.</summary>
    public string Imei { get; set; } = string.Empty;

    /// <summary>Estado actual del equipo.</summary>
    public DeviceStatus Status { get; set; } = DeviceStatus.Disponible;

    /// <summary>Identificador de la ubicacion actual.</summary>
    public Guid LocationId { get; set; }

    /// <summary>Ubicacion actual del equipo.</summary>
    public Location? Location { get; set; }

    /// <summary>Fecha de ingreso del equipo al inventario.</summary>
    public DateTime EntryDate { get; set; }

    /// <summary>Observaciones libres sobre el equipo.</summary>
    public string? Observations { get; set; }

    /// <summary>Precio de compra referencial, en soles.</summary>
    public decimal? PurchasePrice { get; set; }

    /// <summary>Fecha de creacion del registro.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Ultima fecha de actualizacion del registro.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Historial de movimientos del equipo.</summary>
    public ICollection<Movement> Movements { get; set; } = new List<Movement>();
}
