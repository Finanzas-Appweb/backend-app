using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Urbania360.Domain.Entities;

/// <summary>
/// Cliente del sistema
/// </summary>
[Table("Clients")]
public class Client
{
    /// <summary>
    /// Identificador único del cliente
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    [Required]
    [MaxLength(60)]
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Apellido del cliente
    /// </summary>
    [Required]
    [MaxLength(60)]
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Email del cliente
    /// </summary>
    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// Ingresos anuales del cliente
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal AnnualIncome { get; set; }

    /// <summary>
    /// Usuario que creó el cliente
    /// </summary>
    [Required]
    public Guid CreatedByUserId { get; set; }

    /// <summary>
    /// Fecha de creación en UTC
    /// </summary>
    [Required]
    public DateTime CreatedAtUtc { get; set; }

    // Navigation properties
    [ForeignKey(nameof(CreatedByUserId))]
    public virtual User CreatedByUser { get; set; } = null!;
    
    public virtual ICollection<LoanSimulation> LoanSimulations { get; set; } = new List<LoanSimulation>();
}