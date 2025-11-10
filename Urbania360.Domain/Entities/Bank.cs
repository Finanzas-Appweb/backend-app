using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Urbania360.Domain.Entities;

/// <summary>
/// Catálogo de bancos
/// </summary>
[Table("Banks")]
public class Bank
{
    /// <summary>
    /// Identificador único del banco (INT IDENTITY)
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Nombre del banco (único)
    /// </summary>
    [Required]
    [MaxLength(80)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Descripción del banco
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>
    /// Tasa anual TEA del banco
    /// </summary>
    [Column(TypeName = "decimal(6,4)")]
    public decimal? AnnualRateTea { get; set; }

    /// <summary>
    /// Fecha desde la cual es efectiva la tasa
    /// </summary>
    public DateTime? EffectiveFrom { get; set; }

    // Navigation properties
    public virtual ICollection<LoanSimulation> LoanSimulations { get; set; } = new List<LoanSimulation>();
}