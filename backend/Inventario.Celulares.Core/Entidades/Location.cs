using Inventario.Celulares.Core.Enumeraciones;

namespace Inventario.Celulares.Core.Entidades;

/// <summary>
/// Ubicacion fisica o logica donde se almacenan los equipos.
/// </summary>
public class Location
{
    /// <summary>Identificador unico de la ubicacion.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Nombre descriptivo de la ubicacion.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Codigo corto de la ubicacion.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Descripcion opcional.</summary>
    public string? Description { get; set; }

    /// <summary>Equipos actualmente ubicados en este punto.</summary>
    public ICollection<Device> Devices { get; set; } = new List<Device>();

    /// <summary>Movimientos en los que la ubicacion es el origen.</summary>
    public ICollection<Movement> OriginMovements { get; set; } = new List<Movement>();

    /// <summary>Movimientos en los que la ubicacion es el destino.</summary>
    public ICollection<Movement> DestinationMovements { get; set; } = new List<Movement>();
}
