namespace Urbania360.Api.DTOs.Clients;

/// <summary>
/// Response de cliente
/// </summary>
public class ClientResponse
{
    /// <summary>
    /// ID del cliente
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Apellido del cliente
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Nombre completo del cliente
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Email del cliente
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Ingresos anuales del cliente
    /// </summary>
    public decimal AnnualIncome { get; set; }

    /// <summary>
    /// Usuario que creó el cliente
    /// </summary>
    public string CreatedByUserName { get; set; } = null!;

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }
}