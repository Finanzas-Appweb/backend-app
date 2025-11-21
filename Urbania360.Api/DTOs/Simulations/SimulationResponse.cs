using Urbania360.Domain.Enums;

namespace Urbania360.Api.DTOs.Simulations;

/// <summary>
/// Response con datos de una simulación
/// </summary>
public class SimulationResponse
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = null!;
    public Guid? PropertyId { get; set; }
    public string? PropertyTitle { get; set; }
    public int? BankId { get; set; }
    public string? BankName { get; set; }
    
    // Parámetros de entrada
    public decimal Principal { get; set; }
    public Currency Currency { get; set; }
    public RateType RateType { get; set; }
    public decimal? TEA { get; set; }
    public decimal? TNA { get; set; }
    public int? CapitalizationPerYear { get; set; }
    public int TermMonths { get; set; }
    public GraceType GraceType { get; set; }
    public int GraceMonths { get; set; }
    public DateOnly StartDate { get; set; }
    public bool ApplyMiViviendaBonus { get; set; }
    public decimal? BonusAmount { get; set; }
    public decimal LifeInsuranceRateMonthly { get; set; }
    public decimal RiskInsuranceRateAnnual { get; set; }
    public decimal FeesMonthly { get; set; }
    
    // Resultados calculados
    public decimal TEM { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal TCEA { get; set; }
    public decimal VAN { get; set; }
    public decimal TIR { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal TotalCost { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    
    // Tabla de amortización
    public List<AmortizationItemResponse> AmortizationSchedule { get; set; } = new();
}

/// <summary>
/// Response resumido para listado de simulaciones
/// </summary>
public class SimulationSummaryResponse
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = null!;
    public string? PropertyTitle { get; set; }
    public decimal Principal { get; set; }
    public Currency Currency { get; set; }
    public int TermMonths { get; set; }
    public decimal TEM { get; set; }
    public decimal MonthlyPayment { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
