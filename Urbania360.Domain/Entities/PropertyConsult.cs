using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Urbania360.Domain.Entities;

/// <summary>
/// Consulta de propiedad - para estadísticas de propiedades más consultadas
/// </summary>
[Table("PropertyConsults")]
public class PropertyConsult
{
    /// <summary>
    /// Identificador único (BIGINT IDENTITY)
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Propiedad consultada
    /// </summary>
    [Required]
    public Guid PropertyId { get; set; }

    /// <summary>
    /// Usuario que realizó la consulta (opcional)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Fecha de la consulta en UTC
    /// </summary>
    [Required]
    public DateTime CreatedAtUtc { get; set; }

    // Navigation properties
    [ForeignKey(nameof(PropertyId))]
    public virtual Property Property { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}