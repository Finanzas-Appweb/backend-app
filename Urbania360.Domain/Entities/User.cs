using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Urbania360.Domain.Enums;

namespace Urbania360.Domain.Entities;

/// <summary>
/// Usuario del sistema (Admin, Agent, Client)
/// </summary>
[Table("Users")]
public class User
{
    /// <summary>
    /// Identificador único del usuario
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre de usuario único
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = null!;

    /// <summary>
    /// Nombre del usuario
    /// </summary>
    [Required]
    [MaxLength(60)]
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Apellidos del usuario
    /// </summary>
    [Required]
    [MaxLength(60)]
    public string LastName { get; set; } = null!;

    /// <summary>
    /// DNI del usuario (8 caracteres numéricos)
    /// </summary>
    [Required]
    [MaxLength(8)]
    [MinLength(8)]
    public string Dni { get; set; } = null!;

    /// <summary>
    /// Nombre completo del usuario (calculado)
    /// </summary>
    [Required]
    [MaxLength(120)]
    public string FullName { get; set; } = null!;

    /// <summary>
    /// Email único del usuario
    /// </summary>
    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Número de teléfono
    /// </summary>
    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// Rol del usuario en el sistema
    /// </summary>
    [Required]
    public Role Role { get; set; }

    /// <summary>
    /// Hash de la contraseña (BCrypt)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// Fecha de creación en UTC
    /// </summary>
    [Required]
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Indica si el usuario está activo
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Client> CreatedClients { get; set; } = new List<Client>();
    public virtual ICollection<Property> CreatedProperties { get; set; } = new List<Property>();
    public virtual ICollection<LoanSimulation> CreatedSimulations { get; set; } = new List<LoanSimulation>();
    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    public virtual UserPreference? UserPreference { get; set; }
}