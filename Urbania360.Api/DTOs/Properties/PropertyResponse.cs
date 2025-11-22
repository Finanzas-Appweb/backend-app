using Urbania360.Domain.Enums;

namespace Urbania360.Api.DTOs.Properties;

/// <summary>
/// Response con datos de una propiedad
/// </summary>
public class PropertyResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string District { get; set; } = null!;
    public string Province { get; set; } = null!;
    public PropertyType Type { get; set; }
    public decimal AreaM2 { get; set; }
    public decimal Price { get; set; }
    public Currency Currency { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public List<PropertyImageResponse> Images { get; set; } = new();
    public int ConsultsCount { get; set; }
}

/// <summary>
/// Response resumido para listado de propiedades
/// </summary>
public class PropertySummaryResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string District { get; set; } = null!;
    public PropertyType Type { get; set; }
    public decimal Price { get; set; }
    public Currency Currency { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int ConsultsCount { get; set; }
}

/// <summary>
/// Response con datos de una imagen de propiedad
/// </summary>
public class PropertyImageResponse
{
    public long Id { get; set; }
    public string Url { get; set; } = null!;
}
