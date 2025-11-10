using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Urbania360.Domain.Enums;

namespace Urbania360.Domain.Entities;

/// <summary>
/// Propiedad inmobiliaria
/// </summary>
[Table("Properties")]
public class Property
{
    /// <summary>
    /// Identificador único de la propiedad
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Código único de la propiedad (ej. P0001)
    /// </summary>
    [Required]
    [MaxLength(12)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// Título de la propiedad
    /// </summary>
    [Required]
    [MaxLength(120)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Dirección de la propiedad
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Address { get; set; } = null!;

    /// <summary>
    /// Distrito donde se ubica la propiedad
    /// </summary>
    [Required]
    [MaxLength(80)]
    public string District { get; set; } = null!;

    /// <summary>
    /// Provincia donde se ubica la propiedad
    /// </summary>
    [Required]
    [MaxLength(80)]
    public string Province { get; set; } = null!;

    /// <summary>
    /// Tipo de propiedad
    /// </summary>
    [Required]
    public PropertyType Type { get; set; }

    /// <summary>
    /// Área en metros cuadrados
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? AreaM2 { get; set; }

    /// <summary>
    /// Precio de la propiedad
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    /// <summary>
    /// Moneda del precio
    /// </summary>
    [Required]
    public Currency Currency { get; set; }

    /// <summary>
    /// Usuario que creó la propiedad
    /// </summary>
    [Required]
    public Guid CreatedByUserId { get; set; }

    /// <summary>
    /// Fecha de creación en UTC
    /// </summary>
    [Required]
    public DateTime CreatedAtUtc { get; set; }

    // Navigation properties
    [ForeignKey(nameof(CreatedByUserId))]
    public virtual User CreatedByUser { get; set; } = null!;
    
    public virtual ICollection<PropertyImage> PropertyImages { get; set; } = new List<PropertyImage>();
    public virtual ICollection<PropertyConsult> PropertyConsults { get; set; } = new List<PropertyConsult>();
    public virtual ICollection<LoanSimulation> LoanSimulations { get; set; } = new List<LoanSimulation>();
}