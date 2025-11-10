using System.ComponentModel.DataAnnotations;

namespace Urbania360.Api.DTOs.Auth;

/// <summary>
/// Request para login de usuario
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Email del usuario
    /// </summary>
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El email debe tener un formato válido")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    [Required(ErrorMessage = "La contraseña es requerida")]
    public string Password { get; set; } = null!;
}