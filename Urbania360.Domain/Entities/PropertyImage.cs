using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Urbania360.Domain.Entities;

/// <summary>
/// Imagen de propiedad
/// </summary>
[Table("PropertyImages")]
public class PropertyImage
{
    /// <summary>
    /// Identificador único (BIGINT IDENTITY)
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Propiedad a la que pertenece la imagen
    /// </summary>
    [Required]
    public Guid PropertyId { get; set; }

    /// <summary>
    /// URL de la imagen
    /// </summary>
    [Required]
    [MaxLength(300)]
    public string Url { get; set; } = null!;

    // Navigation properties
    [ForeignKey(nameof(PropertyId))]
    public virtual Property Property { get; set; } = null!;
}