using Urbania360.Domain.Entities;
using Urbania360.Domain.Enums;

namespace Urbania360.Infrastructure.Services;

/// <summary>
/// Entrada para cálculo de simulación hipotecaria
/// </summary>
public class SimulationInput
{
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
}

/// <summary>
/// Resultado del cálculo de simulación hipotecaria
/// </summary>
public class SimulationResult
{
    public decimal TEM { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal TCEA { get; set; }
    public decimal VAN { get; set; }
    public decimal TIR { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal TotalCost { get; set; }
}

/// <summary>
/// Interfaz para el servicio de cálculo hipotecario
/// </summary>
public interface IMortgageCalculatorService
{
    (SimulationResult Result, List<AmortizationItem> Schedule) Calculate(SimulationInput input);
}

/// <summary>
/// Servicio de cálculo hipotecario según método francés
/// </summary>
public class MortgageCalculatorService : IMortgageCalculatorService
{
    public (SimulationResult Result, List<AmortizationItem> Schedule) Calculate(SimulationInput input)
    {
        // 1. Conversión de tasas a TEM
        decimal tem = CalculateTEM(input.RateType, input.TEA, input.TNA, input.CapitalizationPerYear);

        // 2. Aplicar bono Mi Vivienda si corresponde
        decimal principal = input.Principal;
        if (input.ApplyMiViviendaBonus && input.BonusAmount.HasValue)
        {
            principal -= input.BonusAmount.Value;
        }

        // 3. Generar tabla de amortización
        var schedule = GenerateAmortizationSchedule(
            principal,
            tem,
            input.TermMonths,
            input.GraceType,
            input.GraceMonths,
            input.StartDate,
            input.LifeInsuranceRateMonthly,
            input.RiskInsuranceRateAnnual,
            input.FeesMonthly,
            input.Principal // Valor asegurado original
        );

        // 4. Calcular indicadores financieros
        var result = CalculateFinancialIndicators(
            schedule,
            principal,
            tem,
            input.TermMonths,
            input.GraceMonths
        );

        return (result, schedule);
    }

    private decimal CalculateTEM(RateType rateType, decimal? tea, decimal? tna, int? capitalizationPerYear)
    {
        if (rateType == RateType.TEA && tea.HasValue)
        {
            // TEM = (1 + TEA)^(1/12) - 1
            return (decimal)(Math.Pow((double)(1 + tea.Value), 1.0 / 12.0) - 1);
        }
        else if (rateType == RateType.TNA && tna.HasValue && capitalizationPerYear.HasValue)
        {
            // Primero calcular TEA desde TNA
            // TEA = (1 + TNA/n)^n - 1, donde n = periodos de capitalización por año
            decimal teaFromTna = (decimal)(Math.Pow((double)(1 + tna.Value / capitalizationPerYear.Value), capitalizationPerYear.Value) - 1);
            // Luego TEM
            return (decimal)(Math.Pow((double)(1 + teaFromTna), 1.0 / 12.0) - 1);
        }

        throw new InvalidOperationException("Debe proporcionar TEA o TNA con capitalización");
    }

    private List<AmortizationItem> GenerateAmortizationSchedule(
        decimal principal,
        decimal tem,
        int termMonths,
        GraceType graceType,
        int graceMonths,
        DateOnly startDate,
        decimal lifeInsuranceRateMonthly,
        decimal riskInsuranceRateAnnual,
        decimal feesMonthly,
        decimal originalPrincipal)
    {
        var schedule = new List<AmortizationItem>();
        decimal balance = principal;
        int period = 1;
        DateOnly currentDate = startDate;

        // Periodo de gracia
        for (int i = 0; i < graceMonths; i++)
        {
            currentDate = startDate.AddMonths(period - 1);
            decimal interest = balance * tem;
            decimal lifeIns = balance * lifeInsuranceRateMonthly;
            decimal riskIns = originalPrincipal * (riskInsuranceRateAnnual / 12);
            decimal fees = feesMonthly;

            AmortizationItem item;

            if (graceType == GraceType.Total)
            {
                // Gracia total: no se paga nada, intereses se capitalizan
                balance = balance * (1 + tem);
                item = new AmortizationItem
                {
                    Period = period,
                    DueDate = currentDate,
                    OpeningBalance = balance / (1 + tem), // Balance antes de capitalizar
                    Interest = interest,
                    Principal = 0,
                    Installment = 0,
                    LifeInsurance = 0,
                    RiskInsurance = 0,
                    Fees = 0,
                    ClosingBalance = balance
                };
            }
            else if (graceType == GraceType.Partial)
            {
                // Gracia parcial: se pagan intereses + seguros + comisiones
                decimal payment = interest + lifeIns + riskIns + fees;
                item = new AmortizationItem
                {
                    Period = period,
                    DueDate = currentDate,
                    OpeningBalance = balance,
                    Interest = interest,
                    Principal = 0,
                    Installment = payment,
                    LifeInsurance = lifeIns,
                    RiskInsurance = riskIns,
                    Fees = fees,
                    ClosingBalance = balance
                };
            }
            else
            {
                // Sin gracia
                break;
            }

            schedule.Add(item);
            period++;
        }

        // Periodo de amortización (método francés)
        int remainingMonths = termMonths - graceMonths;
        if (remainingMonths > 0)
        {
            // Calcular cuota fija (método francés)
            // Cuota = Principal * [i / (1 - (1 + i)^(-n))]
            decimal monthlyPrincipalPayment = CalculateFrenchPayment(balance, tem, remainingMonths);

            for (int i = 0; i < remainingMonths; i++)
            {
                currentDate = startDate.AddMonths(period - 1);
                decimal openingBalance = balance;
                decimal interest = balance * tem;
                decimal principalPayment = monthlyPrincipalPayment - interest;
                
                // Ajustar último periodo para cerrar en cero
                if (i == remainingMonths - 1)
                {
                    principalPayment = balance;
                    monthlyPrincipalPayment = principalPayment + interest;
                }

                balance -= principalPayment;
                if (balance < 0) balance = 0;

                decimal lifeIns = openingBalance * lifeInsuranceRateMonthly;
                decimal riskIns = originalPrincipal * (riskInsuranceRateAnnual / 12);
                decimal fees = feesMonthly;
                decimal totalInstallment = monthlyPrincipalPayment + lifeIns + riskIns + fees;

                var item = new AmortizationItem
                {
                    Period = period,
                    DueDate = currentDate,
                    OpeningBalance = openingBalance,
                    Interest = interest,
                    Principal = principalPayment,
                    Installment = totalInstallment,
                    LifeInsurance = lifeIns,
                    RiskInsurance = riskIns,
                    Fees = fees,
                    ClosingBalance = balance
                };

                schedule.Add(item);
                period++;
            }
        }

        return schedule;
    }

    private decimal CalculateFrenchPayment(decimal principal, decimal tem, int months)
    {
        if (tem == 0) return principal / months;
        
        // Cuota = Principal * [i / (1 - (1 + i)^(-n))]
        double rate = (double)tem;
        double n = months;
        double denominator = 1 - Math.Pow(1 + rate, -n);
        return (decimal)((double)principal * (rate / denominator));
    }

    private SimulationResult CalculateFinancialIndicators(
        List<AmortizationItem> schedule,
        decimal principal,
        decimal tem,
        int termMonths,
        int graceMonths)
    {
        decimal totalInterest = schedule.Sum(x => x.Interest);
        decimal totalLifeInsurance = schedule.Sum(x => x.LifeInsurance);
        decimal totalRiskInsurance = schedule.Sum(x => x.RiskInsurance);
        decimal totalFees = schedule.Sum(x => x.Fees);
        decimal totalCost = principal + totalInterest + totalLifeInsurance + totalRiskInsurance + totalFees;

        // Cuota mensual representativa (promedio de periodos de amortización, sin gracia)
        var amortizationPeriods = schedule.Skip(graceMonths).ToList();
        decimal monthlyPayment = amortizationPeriods.Any() 
            ? amortizationPeriods.Average(x => x.Installment) 
            : 0;

        // VAN (Valor Actual Neto) usando TEM como tasa de descuento
        decimal van = -principal;
        for (int i = 0; i < schedule.Count; i++)
        {
            decimal cashFlow = schedule[i].Installment;
            van += cashFlow / (decimal)Math.Pow((double)(1 + tem), i + 1);
        }

        // TIR (Tasa Interna de Retorno) - búsqueda iterativa
        decimal tir = CalculateTIR(principal, schedule);

        // TCEA (Tasa de Costo Efectivo Anual) - incluye todos los costos
        // TCEA se calcula como la tasa que hace que el VAN de todos los flujos sea cero
        decimal tcea = CalculateTCEA(principal, schedule);

        return new SimulationResult
        {
            TEM = tem,
            MonthlyPayment = monthlyPayment,
            TCEA = tcea,
            VAN = van,
            TIR = tir,
            TotalInterest = totalInterest,
            TotalCost = totalCost
        };
    }

    private decimal CalculateTIR(decimal principal, List<AmortizationItem> schedule)
    {
        // TIR mensual: tasa que hace VAN = 0
        // Usamos búsqueda iterativa (Newton-Raphson simplificado)
        decimal rate = 0.01m; // Estimación inicial 1% mensual
        int maxIterations = 100;
        decimal tolerance = 0.0001m;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            decimal npv = -principal;
            decimal dnpv = 0;

            for (int i = 0; i < schedule.Count; i++)
            {
                decimal cashFlow = schedule[i].Installment;
                int period = i + 1;
                decimal factor = (decimal)Math.Pow((double)(1 + rate), period);
                npv += cashFlow / factor;
                dnpv -= cashFlow * period / (factor * (1 + rate));
            }

            if (Math.Abs(npv) < tolerance)
            {
                break;
            }

            rate = rate - npv / dnpv;
            
            // Validar que la tasa sea razonable
            if (rate < -0.5m) rate = -0.5m;
            if (rate > 1m) rate = 1m;
        }

        // Anualizar: TIR_anual = (1 + TIR_mensual)^12 - 1
        decimal tirAnual = (decimal)(Math.Pow((double)(1 + rate), 12) - 1);
        return tirAnual;
    }

    private decimal CalculateTCEA(decimal principal, List<AmortizationItem> schedule)
    {
        // TCEA es similar a TIR pero considera todos los costos
        // Ya calculamos TIR con todos los costos incluidos en Installment
        // Por lo tanto, TCEA = TIR en este caso
        return CalculateTIR(principal, schedule);
    }
}
