namespace Urbania360.Api.DTOs.Simulations;

/// <summary>
/// Response con datos de un ítem de amortización
/// </summary>
public class AmortizationItemResponse
{
    /// <summary>
    /// ID del item (igual al periodo, reiniciado por simulación)
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Número de periodo/cuota (1, 2, 3, ...)
    /// </summary>
    public int Period { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Interest { get; set; }
    public decimal Principal { get; set; }
    public decimal Installment { get; set; }
    public decimal LifeInsurance { get; set; }
    public decimal RiskInsurance { get; set; }
    public decimal Fees { get; set; }
    public decimal ClosingBalance { get; set; }
}
