using System.ComponentModel.DataAnnotations;

namespace Urbania360.Api.DTOs.Auth;

/// <summary>
/// Request para registro de usuario
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    [Required(ErrorMessage = "El nombre completo es requerido")]
    [StringLength(120, ErrorMessage = "El nombre completo no puede exceder 120 caracteres")]
    public string FullName { get; set; } = null!;

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

    /// <summary>
    /// Rol del usuario (1=Admin, 2=Agent, 3=Client)
    /// </summary>
    [Required(ErrorMessage = "El rol es requerido")]
    [Range(1, 3, ErrorMessage = "El rol debe ser 1 (Admin), 2 (Agent) o 3 (Client)")]
    public int Role { get; set; }
}