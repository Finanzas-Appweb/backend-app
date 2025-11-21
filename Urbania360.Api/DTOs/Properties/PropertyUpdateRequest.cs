using System.ComponentModel.DataAnnotations;
using Urbania360.Domain.Enums;

namespace Urbania360.Api.DTOs.Properties;

/// <summary>
/// Request para actualizar una propiedad
/// </summary>
public class PropertyUpdateRequest
{
    [Required(ErrorMessage = "El código es requerido")]
    [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "El título es requerido")]
    [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "La dirección es requerida")]
    [StringLength(300, ErrorMessage = "La dirección no puede exceder 300 caracteres")]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "El distrito es requerido")]
    [StringLength(100, ErrorMessage = "El distrito no puede exceder 100 caracteres")]
    public string District { get; set; } = null!;

    [Required(ErrorMessage = "La provincia es requerida")]
    [StringLength(100, ErrorMessage = "La provincia no puede exceder 100 caracteres")]
    public string Province { get; set; } = null!;

    [Required(ErrorMessage = "El tipo de propiedad es requerido")]
    public PropertyType Type { get; set; }

    [Required(ErrorMessage = "El área es requerida")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El área debe ser mayor a cero")]
    public decimal AreaM2 { get; set; }

    [Required(ErrorMessage = "El precio es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "La moneda es requerida")]
    public Currency Currency { get; set; }

    public List<string> ImagesUrl { get; set; } = new();
}
