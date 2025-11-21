using Urbania360.Domain.Enums;

namespace Urbania360.Api.DTOs.Users;

/// <summary>
/// Response con datos del usuario
/// </summary>
public class UserResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Dni { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public Role Role { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Currency? DefaultCurrency { get; set; }
    public RateType? DefaultRateType { get; set; }
}
