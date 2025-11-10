namespace Urbania360.Domain.Enums;

/// <summary>
/// Tipo de período de gracia
/// </summary>
public enum GraceType
{
    /// <summary>
    /// Sin período de gracia
    /// </summary>
    None = 0,

    /// <summary>
    /// Gracia parcial (solo intereses)
    /// </summary>
    Partial = 1,

    /// <summary>
    /// Gracia total (capital + intereses)
    /// </summary>
    Total = 2
}