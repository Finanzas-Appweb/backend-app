using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Urbania360.Domain.Entities;

/// <summary>
/// Item de la tabla de amortización
/// </summary>
[Table("AmortizationItems")]
public class AmortizationItem
{
    /// <summary>
    /// Identificador único (BIGINT IDENTITY)
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Simulación a la que pertenece
    /// </summary>
    [Required]
    public Guid SimulationId { get; set; }

    /// <summary>
    /// Número de período
    /// </summary>
    [Required]
    public int Period { get; set; }

    /// <summary>
    /// Fecha de vencimiento de la cuota
    /// </summary>
    [Required]
    [Column(TypeName = "date")]
    public DateOnly DueDate { get; set; }

    /// <summary>
    /// Saldo inicial del período
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal OpeningBalance { get; set; }

    /// <summary>
    /// Interés del período
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Interest { get; set; }

    /// <summary>
    /// Amortización del capital
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Principal { get; set; }

    /// <summary>
    /// Cuota total (Principal + Interés + Seguros + Comisiones)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Installment { get; set; }

    /// <summary>
    /// Seguro de vida
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal LifeInsurance { get; set; }

    /// <summary>
    /// Seguro de riesgo
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal RiskInsurance { get; set; }

    /// <summary>
    /// Comisiones y gastos
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Fees { get; set; }

    /// <summary>
    /// Saldo final del período
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ClosingBalance { get; set; }

    // Navigation properties
    [ForeignKey(nameof(SimulationId))]
    public virtual LoanSimulation Simulation { get; set; } = null!;
}