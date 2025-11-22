using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Urbania360.Domain.Enums;

namespace Urbania360.Domain.Entities;

/// <summary>
/// Simulación de préstamo hipotecario
/// </summary>
[Table("LoanSimulations")]
public class LoanSimulation
{
    /// <summary>
    /// Identificador único de la simulación
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Cliente para quien se hace la simulación
    /// </summary>
    [Required]    
    public Guid ClientId { get; set; }

    /// <summary>
    /// Propiedad asociada (opcional)
    /// </summary>
    public Guid? PropertyId { get; set; }

    /// <summary>
    /// Banco asociado (opcional)
    /// </summary>
    public int? BankId { get; set; }

    /// <summary>
    /// Monto principal del préstamo
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Principal { get; set; }

    /// <summary>
    /// Moneda del préstamo
    /// </summary>
    [Required]
    public Currency Currency { get; set; }

    /// <summary>
    /// Tipo de tasa de interés
    /// </summary>
    [Required]
    public RateType RateType { get; set; }

    /// <summary>
    /// Tasa Efectiva Anual
    /// </summary>
    [Column(TypeName = "decimal(6,4)")]
    public decimal? TEA { get; set; }

    /// <summary>
    /// Tasa Nominal Anual
    /// </summary>
    [Column(TypeName = "decimal(6,4)")]
    public decimal? TNA { get; set; }

    /// <summary>
    /// Capitalizaciones por año
    /// </summary>
    public int? CapitalizationPerYear { get; set; }

    /// <summary>
    /// Plazo en meses
    /// </summary>
    [Required]
    public int TermMonths { get; set; }

    /// <summary>
    /// Tipo de período de gracia
    /// </summary>
    [Required]
    public GraceType GraceType { get; set; }

    /// <summary>
    /// Meses de gracia
    /// </summary>
    [Required]
    public int GraceMonths { get; set; } = 0;

    /// <summary>
    /// Fecha de inicio del préstamo
    /// </summary>
    [Required]
    [Column(TypeName = "date")]
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Aplicar bono Mi Vivienda
    /// </summary>
    [Required]
    public bool ApplyMiViviendaBonus { get; set; } = false;

    /// <summary>
    /// Monto del bono
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? BonusAmount { get; set; }

    /// <summary>
    /// Tasa mensual del seguro de vida
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(6,4)")]
    public decimal LifeInsuranceRateMonthly { get; set; }

    /// <summary>
    /// Tasa anual del seguro de riesgo
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(6,4)")]
    public decimal RiskInsuranceRateAnnual { get; set; }

    /// <summary>
    /// Comisiones mensuales
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal FeesMonthly { get; set; }

    // Resultados cacheados para UI
    /// <summary>
    /// Tasa Efectiva Mensual calculada
    /// </summary>
    [Column(TypeName = "decimal(6,4)")]
    public decimal? TEM { get; set; }

    /// <summary>
    /// Cuota mensual calculada
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MonthlyPayment { get; set; }

    /// <summary>
    /// Tasa de Costo Efectivo Anual
    /// </summary>
    [Column(TypeName = "decimal(6,4)")]
    public decimal? TCEA { get; set; }

    /// <summary>
    /// Valor Actual Neto
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? VAN { get; set; }

    /// <summary>
    /// Tasa Interna de Retorno
    /// </summary>
    [Column(TypeName = "decimal(6,4)")]
    public decimal? TIR { get; set; }

    /// <summary>
    /// Total de intereses pagados
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalInterest { get; set; }

    /// <summary>
    /// Costo total del préstamo
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalCost { get; set; }

    /// <summary>
    /// Usuario que creó la simulación  
    /// </summary>
    [Required]
    public Guid CreatedByUserId { get; set; }

    /// <summary>
    /// Fecha de creación en UTC
    /// </summary>
    [Required]
    public DateTime CreatedAtUtc { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ClientId))]
    public virtual Client Client { get; set; } = null!;

    [ForeignKey(nameof(PropertyId))]
    public virtual Property? Property { get; set; }

    [ForeignKey(nameof(BankId))]
    public virtual Bank? Bank { get; set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public virtual User CreatedByUser { get; set; } = null!;

    public virtual ICollection<AmortizationItem> AmortizationItems { get; set; } = new List<AmortizationItem>();
}