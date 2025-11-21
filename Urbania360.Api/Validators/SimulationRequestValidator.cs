using FluentValidation;
using Urbania360.Api.DTOs.Simulations;
using Urbania360.Domain.Enums;

namespace Urbania360.Api.Validators;

/// <summary>
/// Validador para SimulationRequest
/// </summary>
public class SimulationRequestValidator : AbstractValidator<SimulationRequest>
{
    public SimulationRequestValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("El ID del cliente es requerido");

        RuleFor(x => x.Principal)
            .GreaterThan(0).WithMessage("El monto del préstamo debe ser mayor a cero");

        RuleFor(x => x.Currency)
            .IsInEnum().WithMessage("La moneda no es válida");

        RuleFor(x => x.RateType)
            .IsInEnum().WithMessage("El tipo de tasa no es válido");

        RuleFor(x => x.TEA)
            .NotNull().When(x => x.RateType == RateType.TEA)
            .WithMessage("TEA es requerida cuando el tipo de tasa es TEA")
            .GreaterThan(0).When(x => x.TEA.HasValue)
            .WithMessage("TEA debe ser mayor a cero");

        RuleFor(x => x.TNA)
            .Null().When(x => x.RateType == RateType.TEA)
            .WithMessage("TNA debe ser nula cuando el tipo de tasa es TEA");

        RuleFor(x => x.TNA)
            .NotNull().When(x => x.RateType == RateType.TNA)
            .WithMessage("TNA es requerida cuando el tipo de tasa es TNA")
            .GreaterThan(0).When(x => x.TNA.HasValue)
            .WithMessage("TNA debe ser mayor a cero");

        RuleFor(x => x.TEA)
            .Null().When(x => x.RateType == RateType.TNA)
            .WithMessage("TEA debe ser nula cuando el tipo de tasa es TNA");

        RuleFor(x => x.CapitalizationPerYear)
            .NotNull().When(x => x.RateType == RateType.TNA)
            .WithMessage("La capitalización por año es requerida cuando el tipo de tasa es TNA")
            .GreaterThan(0).When(x => x.CapitalizationPerYear.HasValue)
            .WithMessage("La capitalización por año debe ser mayor a cero");

        RuleFor(x => x.TermMonths)
            .GreaterThan(0).WithMessage("El plazo en meses debe ser mayor a cero")
            .LessThanOrEqualTo(600).WithMessage("El plazo en meses no puede exceder 600");

        RuleFor(x => x.GraceType)
            .IsInEnum().WithMessage("El tipo de gracia no es válido");

        RuleFor(x => x.GraceMonths)
            .GreaterThanOrEqualTo(0).WithMessage("Los meses de gracia deben ser mayor o igual a cero")
            .LessThan(x => x.TermMonths).WithMessage("Los meses de gracia deben ser menores al plazo total");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("La fecha de inicio es requerida");

        RuleFor(x => x.BonusAmount)
            .NotNull().When(x => x.ApplyMiViviendaBonus)
            .WithMessage("El monto del bono es requerido cuando se aplica el bono Mi Vivienda")
            .GreaterThan(0).When(x => x.BonusAmount.HasValue)
            .WithMessage("El monto del bono debe ser mayor a cero")
            .LessThan(x => x.Principal).When(x => x.BonusAmount.HasValue)
            .WithMessage("El monto del bono no puede ser mayor al préstamo principal");

        RuleFor(x => x.LifeInsuranceRateMonthly)
            .GreaterThanOrEqualTo(0).WithMessage("La tasa del seguro de vida debe ser mayor o igual a cero");

        RuleFor(x => x.RiskInsuranceRateAnnual)
            .GreaterThanOrEqualTo(0).WithMessage("La tasa del seguro de riesgo debe ser mayor o igual a cero");

        RuleFor(x => x.FeesMonthly)
            .GreaterThanOrEqualTo(0).WithMessage("Las comisiones mensuales deben ser mayor o igual a cero");
    }
}
