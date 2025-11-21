using FluentValidation;
using Urbania360.Api.DTOs.Auth;

namespace Urbania360.Api.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es requerido")
            .MaximumLength(50).WithMessage("El nombre de usuario no puede exceder 50 caracteres");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(60).WithMessage("El nombre no puede exceder 60 caracteres");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Los apellidos son requeridos")
            .MaximumLength(60).WithMessage("Los apellidos no pueden exceder 60 caracteres");

        RuleFor(x => x.Dni)
            .NotEmpty().WithMessage("El DNI es requerido")
            .Length(8).WithMessage("El DNI debe tener exactamente 8 caracteres")
            .Matches(@"^\d{8}$").WithMessage("El DNI debe contener solo números");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("El email debe tener un formato válido")
            .MaximumLength(100).WithMessage("El email no puede exceder 100 caracteres");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
            .MaximumLength(100).WithMessage("La contraseña no puede exceder 100 caracteres");

        RuleFor(x => x.Role)
            .InclusiveBetween(1, 3).WithMessage("El rol debe ser 1 (Admin), 2 (Agent) o 3 (Client)");
    }
}