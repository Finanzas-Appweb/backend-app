using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Urbania360.Domain.Entities;

/// <summary>
/// Log de actividades del sistema (alta volumetría)
/// </summary>
[Table("ActivityLogs")]
public class ActivityLog
{
    /// <summary>
    /// Identificador único del log (BIGINT IDENTITY)
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Usuario que realizó la acción
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Acción realizada
    /// </summary>
    [Required]
    [MaxLength(120)]
    public string Action { get; set; } = null!;

    /// <summary>
    /// Entidad afectada
    /// </summary>
    [Required]
    [MaxLength(60)]
    public string Entity { get; set; } = null!;

    /// <summary>
    /// ID de la entidad afectada
    /// </summary>
    [MaxLength(50)]
    public string? EntityId { get; set; }

    /// <summary>
    /// Fecha de creación en UTC
    /// </summary>
    [Required]
    public DateTime CreatedAtUtc { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}