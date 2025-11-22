using System.ComponentModel.DataAnnotations;

namespace Urbania360.Api.DTOs.Banks;

/// <summary>
/// Request para crear o actualizar un banco
/// </summary>
public class BankRequest
{
    [Required(ErrorMessage = "El nombre del banco es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "La tasa anual TEA es requerida")]
    [Range(0.0001, 1, ErrorMessage = "La tasa TEA debe estar entre 0.01% y 100%")]
    public decimal AnnualRateTea { get; set; }

    public DateTime? EffectiveFrom { get; set; }
}

/// <summary>
/// Response con datos de un banco
/// </summary>
public class BankResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal AnnualRateTea { get; set; }
    public DateTime? EffectiveFrom { get; set; }
}
