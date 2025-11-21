using FluentValidation;
using Urbania360.Api.DTOs.Properties;

namespace Urbania360.Api.Validators;

/// <summary>
/// Validador para PropertyCreateRequest
/// </summary>
public class PropertyCreateRequestValidator : AbstractValidator<PropertyCreateRequest>
{
    public PropertyCreateRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es requerido")
            .MaximumLength(20).WithMessage("El código no puede exceder 20 caracteres");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido")
            .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("La dirección es requerida")
            .MaximumLength(300).WithMessage("La dirección no puede exceder 300 caracteres");

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("El distrito es requerido")
            .MaximumLength(100).WithMessage("El distrito no puede exceder 100 caracteres");

        RuleFor(x => x.Province)
            .NotEmpty().WithMessage("La provincia es requerida")
            .MaximumLength(100).WithMessage("La provincia no puede exceder 100 caracteres");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("El tipo de propiedad no es válido");

        RuleFor(x => x.AreaM2)
            .GreaterThan(0).WithMessage("El área debe ser mayor a cero");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a cero");

        RuleFor(x => x.Currency)
            .IsInEnum().WithMessage("La moneda no es válida");
    }
}
