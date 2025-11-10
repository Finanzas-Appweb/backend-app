using System.ComponentModel.DataAnnotations;

namespace Urbania360.Api.DTOs.Clients;

/// <summary>
/// Request para actualizar cliente
/// </summary>
public class ClientUpdateRequest
{
    /// <summary>
    /// Nombre del cliente
    /// </summary>
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(60, ErrorMessage = "El nombre no puede exceder 60 caracteres")]
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Apellido del cliente
    /// </summary>
    [Required(ErrorMessage = "El apellido es requerido")]
    [StringLength(60, ErrorMessage = "El apellido no puede exceder 60 caracteres")]
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Email del cliente
    /// </summary>
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El email debe tener un formato válido")]
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Phone { get; set; }

    /// <summary>
    /// Ingresos anuales del cliente
    /// </summary>
    [Required(ErrorMessage = "Los ingresos anuales son requeridos")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Los ingresos anuales deben ser mayor a 0")]
    public decimal AnnualIncome { get; set; }
}