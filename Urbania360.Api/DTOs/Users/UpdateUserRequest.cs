using System.ComponentModel.DataAnnotations;
using Urbania360.Domain.Enums;

namespace Urbania360.Api.DTOs.Users;

/// <summary>
/// Request para actualizar datos de usuario
/// </summary>
public class UpdateUserRequest
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder 50 caracteres")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(60, ErrorMessage = "El nombre no puede exceder 60 caracteres")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Los apellidos son requeridos")]
    [StringLength(60, ErrorMessage = "Los apellidos no pueden exceder 60 caracteres")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "El DNI es requerido")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener exactamente 8 caracteres")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe contener solo números")]
    public string Dni { get; set; } = null!;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El email debe tener un formato válido")]
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
    public string Email { get; set; } = null!;

    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Phone { get; set; }

    public Currency? DefaultCurrency { get; set; }
    public RateType? DefaultRateType { get; set; }
}
