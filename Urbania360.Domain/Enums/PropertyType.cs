namespace Urbania360.Domain.Enums;

/// <summary>
/// Tipos de propiedad inmobiliaria
/// </summary>
public enum PropertyType
{
    /// <summary>
    /// Casa independiente
    /// </summary>
    Casa = 1,

    /// <summary>
    /// Departamento en edificio
    /// </summary>
    Departamento = 2,

    /// <summary>
    /// Terreno sin construcción
    /// </summary>
    Terreno = 3,

    /// <summary>
    /// Oficina comercial
    /// </summary>
    Oficina = 4,

    /// <summary>
    /// Otros tipos de propiedad
    /// </summary>
    Otro = 99
}