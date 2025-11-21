using System.ComponentModel.DataAnnotations;
using Urbania360.Domain.Enums;

namespace Urbania360.Api.DTOs.Simulations;

/// <summary>
/// Request para crear una simulación de préstamo
/// </summary>
public class SimulationRequest
{
    [Required(ErrorMessage = "El ID del cliente es requerido")]
    public Guid ClientId { get; set; }

    public Guid? PropertyId { get; set; }

    public int? BankId { get; set; }

    [Required(ErrorMessage = "El monto del préstamo es requerido")]
    [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
    public decimal Principal { get; set; }

    [Required(ErrorMessage = "La moneda es requerida")]
    public Currency Currency { get; set; }

    [Required(ErrorMessage = "El tipo de tasa es requerido")]
    public RateType RateType { get; set; }

    [Range(0.0001, 1, ErrorMessage = "TEA debe estar entre 0.01% y 100%")]
    public decimal? TEA { get; set; }

    [Range(0.0001, 1, ErrorMessage = "TNA debe estar entre 0.01% y 100%")]
    public decimal? TNA { get; set; }

    [Range(1, 365, ErrorMessage = "La capitalización por año debe estar entre 1 y 365")]
    public int? CapitalizationPerYear { get; set; }

    [Required(ErrorMessage = "El plazo en meses es requerido")]
    [Range(1, 600, ErrorMessage = "El plazo debe estar entre 1 y 600 meses")]
    public int TermMonths { get; set; }

    [Required(ErrorMessage = "El tipo de gracia es requerido")]
    public GraceType GraceType { get; set; }

    [Range(0, 120, ErrorMessage = "Los meses de gracia deben estar entre 0 y 120")]
    public int GraceMonths { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es requerida")]
    public DateOnly StartDate { get; set; }

    public bool ApplyMiViviendaBonus { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto del bono debe ser mayor o igual a cero")]
    public decimal? BonusAmount { get; set; }

    [Required(ErrorMessage = "La tasa del seguro de vida mensual es requerida")]
    [Range(0, 0.01, ErrorMessage = "La tasa del seguro de vida debe estar entre 0% y 1%")]
    public decimal LifeInsuranceRateMonthly { get; set; }

    [Required(ErrorMessage = "La tasa del seguro de riesgo anual es requerida")]
    [Range(0, 0.1, ErrorMessage = "La tasa del seguro de riesgo debe estar entre 0% y 10%")]
    public decimal RiskInsuranceRateAnnual { get; set; }

    [Required(ErrorMessage = "Las comisiones mensuales son requeridas")]
    [Range(0, 10000, ErrorMessage = "Las comisiones mensuales deben estar entre 0 y 10000")]
    public decimal FeesMonthly { get; set; }
}
