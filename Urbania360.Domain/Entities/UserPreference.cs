using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Urbania360.Domain.Enums;

namespace Urbania360.Domain.Entities;

/// <summary>
/// Preferencias de usuario
/// </summary>
[Table("UserPreferences")]
public class UserPreference
{
    /// <summary>
    /// Identificador único (IDENTITY)
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Usuario al que pertenecen las preferencias (unique)
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Moneda por defecto (PEN por defecto)
    /// </summary>
    [Required]
    public Currency DefaultCurrency { get; set; } = Currency.PEN;

    /// <summary>
    /// Tipo de tasa por defecto (TEA por defecto)
    /// </summary>
    [Required]
    public RateType DefaultRateType { get; set; } = RateType.TEA;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}