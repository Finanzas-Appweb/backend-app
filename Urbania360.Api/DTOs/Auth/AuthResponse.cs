namespace Urbania360.Api.DTOs.Auth;

/// <summary>
/// Response de autenticación exitosa
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT Token
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// Información del usuario autenticado
    /// </summary>
    public UserInfo User { get; set; } = null!;
}

/// <summary>
/// Información básica del usuario autenticado
/// </summary>
public class UserInfo
{
    /// <summary>
    /// ID del usuario
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre completo
    /// </summary>
    public string FullName { get; set; } = null!;

    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Rol del usuario
    /// </summary>
    public string Role { get; set; } = null!;

    /// <summary>
    /// Teléfono
    /// </summary>
    public string? Phone { get; set; }
}