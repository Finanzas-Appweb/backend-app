using System.ComponentModel.DataAnnotations;

namespace Urbania360.Api.DTOs.Auth;

/// <summary>
/// Request para registro de usuario
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Nombre de usuario único
    /// </summary>
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder 50 caracteres")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// Nombre del usuario
    /// </summary>
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(60, ErrorMessage = "El nombre no puede exceder 60 caracteres")]
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Apellidos del usuario
    /// </summary>
    [Required(ErrorMessage = "Los apellidos son requeridos")]
    [StringLength(60, ErrorMessage = "Los apellidos no pueden exceder 60 caracteres")]
    public string LastName { get; set; } = null!;

    /// <summary>
    /// DNI del usuario (8 caracteres numéricos)
    /// </summary>
    [Required(ErrorMessage = "El DNI es requerido")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener exactamente 8 caracteres")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe contener solo números")]
    public string Dni { get; set; } = null!;

    /// <summary>
    /// Email del usuario
    /// </summary>
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El email debe tener un formato válido")]
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Teléfono del usuario
    /// </summary>
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Phone { get; set; }

    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string Password { get; set; } = null!;
}