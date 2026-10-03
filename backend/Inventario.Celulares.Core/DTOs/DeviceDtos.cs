using System.ComponentModel.DataAnnotations;
using Inventario.Celulares.Core.Enumeraciones;

namespace Inventario.Celulares.Core.DTOs;

/// <summary>
/// Datos necesarios para registrar un equipo celular.
/// </summary>
public class CreateDeviceRequest
{
    /// <summary>Marca del fabricante.</summary>
    [Required(ErrorMessage = "La marca es obligatoria")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "La marca debe tener entre 2 y 50 caracteres")]
    public string Brand { get; set; } = string.Empty;

    /// <summary>Modelo comercial.</summary>
    [Required(ErrorMessage = "El modelo es obligatorio")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "El modelo es obligatorio")]
    public string Model { get; set; } = string.Empty;

    /// <summary>IMEI de 15 digitos con digito verificador valido.</summary>
    [Required(ErrorMessage = "El IMEI es obligatorio")]
    [StringLength(15, MinimumLength = 15, ErrorMessage = "El IMEI debe tener exactamente 15 digitos")]
    [RegularExpression(@"^[0-9]{15}$", ErrorMessage = "El IMEI solo admite digitos")]
    public string Imei { get; set; } = string.Empty;

    /// <summary>Estado inicial del equipo.</summary>
    [EnumDataType(typeof(DeviceStatus), ErrorMessage = "El estado del equipo no es valido")]
    public DeviceStatus Status { get; set; } = DeviceStatus.Disponible;

    /// <summary>Ubicacion donde se almacena el equipo.</summary>
    public Guid LocationId { get; set; }

    /// <summary>Fecha de ingreso al inventario.</summary>
    [Required(ErrorMessage = "La fecha de ingreso es obligatoria")]
    public DateTime EntryDate { get; set; }

    /// <summary>Observaciones libres.</summary>
    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar 500 caracteres")]
    public string? Observations { get; set; }

    /// <summary>Precio de compra referencial.</summary>
    [Range(0, 1000000, ErrorMessage = "El precio debe estar entre 0 y 1,000,000")]
    public decimal? PurchasePrice { get; set; }
}

/// <summary>
/// Datos que pueden actualizarse en un equipo.
/// </summary>
public class UpdateDeviceRequest
{
    /// <summary>Marca del fabricante.</summary>
    [StringLength(50, MinimumLength = 2, ErrorMessage = "La marca debe tener entre 2 y 50 caracteres")]
    public string? Brand { get; set; }

    /// <summary>Modelo comercial.</summary>
    [StringLength(80, MinimumLength = 1, ErrorMessage = "El modelo es obligatorio")]
    public string? Model { get; set; }

    /// <summary>IMEI del equipo.</summary>
    [StringLength(15, MinimumLength = 15, ErrorMessage = "El IMEI debe tener exactamente 15 digitos")]
    [RegularExpression(@"^[0-9]{15}$", ErrorMessage = "El IMEI solo admite digitos")]
    public string? Imei { get; set; }

    /// <summary>Estado del equipo.</summary>
    [EnumDataType(typeof(DeviceStatus), ErrorMessage = "El estado del equipo no es valido")]
    public DeviceStatus? Status { get; set; }

    /// <summary>Ubicacion actual.</summary>
    public Guid? LocationId { get; set; }

    /// <summary>Observaciones libres.</summary>
    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar 500 caracteres")]
    public string? Observations { get; set; }

    /// <summary>Precio de compra referencial.</summary>
    [Range(0, 1000000, ErrorMessage = "El precio debe estar entre 0 y 1,000,000")]
    public decimal? PurchasePrice { get; set; }
}

/// <summary>
/// Criterios de busqueda y filtrado de equipos.
/// </summary>
public class DeviceQuery
{
    /// <summary>Busqueda libre por IMEI, marca o modelo.</summary>
    [StringLength(60, ErrorMessage = "El termino de busqueda es demasiado largo")]
    public string? Search { get; set; }

    /// <summary>Filtro por marca.</summary>
    [StringLength(50, ErrorMessage = "La marca es demasiado larga")]
    public string? Brand { get; set; }

    /// <summary>Filtro por modelo.</summary>
    [StringLength(80, ErrorMessage = "El modelo es demasiado largo")]
    public string? Model { get; set; }

    /// <summary>Filtro por estado.</summary>
    [EnumDataType(typeof(DeviceStatus), ErrorMessage = "El estado del equipo no es valido")]
    public DeviceStatus? Status { get; set; }

    /// <summary>Filtro por ubicacion.</summary>
    public Guid? LocationId { get; set; }

    /// <summary>Numero de pagina, comenzando en 1.</summary>
    [Range(1, 100000, ErrorMessage = "La pagina debe ser mayor o igual a 1")]
    public int Page { get; set; } = 1;

    /// <summary>Cantidad de elementos por pagina.</summary>
    [Range(1, 100, ErrorMessage = "El tamano de pagina debe estar entre 1 y 100")]
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Vista de un equipo con el nombre de su ubicacion.
/// </summary>
public class DeviceResponse
{
    /// <summary>Identificador unico del equipo.</summary>
    public Guid Id { get; set; }

    /// <summary>Marca del fabricante.</summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>Modelo comercial.</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Codigo IMEI.</summary>
    public string Imei { get; set; } = string.Empty;

    /// <summary>Estado actual.</summary>
    public DeviceStatus Status { get; set; }

    /// <summary>Nombre del estado, para consumo del frontend.</summary>
    public string StatusName => Status.ToString();

    /// <summary>Identificador de la ubicacion.</summary>
    public Guid LocationId { get; set; }

    /// <summary>Nombre de la ubicacion.</summary>
    public string LocationName { get; set; } = string.Empty;

    /// <summary>Fecha de ingreso.</summary>
    public DateTime EntryDate { get; set; }

    /// <summary>Observaciones libres.</summary>
    public string? Observations { get; set; }

    /// <summary>Precio de compra referencial.</summary>
    public decimal? PurchasePrice { get; set; }

    /// <summary>Fecha de creacion del registro.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Pagina de resultados con metadatos de paginacion.
/// </summary>
/// <typeparam name="T">Tipo de cada elemento.</typeparam>
public class PagedResult<T>
{
    /// <summary>Elementos de la pagina actual.</summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>Cantidad total de coincidencias.</summary>
    public int TotalItems { get; set; }

    /// <summary>Pagina actual.</summary>
    public int Page { get; set; }

    /// <summary>Tamano de pagina.</summary>
    public int PageSize { get; set; }

    /// <summary>Cantidad total de paginas.</summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}
