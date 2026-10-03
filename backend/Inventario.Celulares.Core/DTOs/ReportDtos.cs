namespace Inventario.Celulares.Core.DTOs;

/// <summary>
/// Totales generales del inventario.
/// </summary>
public class StockReportResponse
{
    /// <summary>Cantidad total de equipos registrados.</summary>
    public int TotalDevices { get; set; }

    /// <summary>Valor total del inventario en soles.</summary>
    public decimal TotalValue { get; set; }

    /// <summary>Equipos por estado.</summary>
    public IReadOnlyList<GroupCountResponse> ByStatus { get; set; } = Array.Empty<GroupCountResponse>();

    /// <summary>Equipos por ubicacion.</summary>
    public IReadOnlyList<GroupCountResponse> ByLocation { get; set; } = Array.Empty<GroupCountResponse>();

    /// <summary>Equipos por marca.</summary>
    public IReadOnlyList<GroupCountResponse> ByBrand { get; set; } = Array.Empty<GroupCountResponse>();

    /// <summary>Movimientos por tipo.</summary>
    public IReadOnlyList<GroupCountResponse> ByMovementType { get; set; } = Array.Empty<GroupCountResponse>();
}

/// <summary>
/// Cantidad agrupada por una dimension del reporte.
/// </summary>
public class GroupCountResponse
{
    /// <summary>Nombre de la agrupacion (estado, ubicacion o marca).</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Cantidad de elementos en el grupo.</summary>
    public int Count { get; set; }
}
