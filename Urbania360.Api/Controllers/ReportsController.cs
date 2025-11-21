using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Urbania360.Infrastructure.Data;

namespace Urbania360.Api.Controllers;

/// <summary>
/// Controller para reportes del sistema
/// </summary>
[ApiController]
[Route("api/v1/reports")]
[Authorize]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly UrbaniaDbContext _context;

    public ReportsController(UrbaniaDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtener resumen general del sistema
    /// </summary>
    /// <returns>Estadísticas generales y últimas actividades</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetSummary()
    {
        // Estadísticas generales
        var registeredClients = await _context.Clients.CountAsync();
        var totalUsers = await _context.Users.Where(u => u.IsActive).CountAsync();

        // Últimas 5 actividades
        var lastActivities = await _context.ActivityLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(5)
            .Select(a => new
            {
                id = a.Id,
                action = a.Action,
                entity = a.Entity,
                entityId = a.EntityId,
                userName = a.User.FullName,
                createdAt = a.CreatedAtUtc
            })
            .ToListAsync();

        var summary = new
        {
            statistics = new
            {
                registeredClients,
                totalUsers,
                totalProperties = await _context.Properties.CountAsync(),
                totalSimulations = await _context.LoanSimulations.CountAsync()
            },
            lastActivities
        };

        return Ok(summary);
    }

    /// <summary>
    /// Obtener propiedades más consultadas
    /// </summary>
    /// <param name="limit">Número máximo de resultados (default: 10)</param>
    /// <returns>Lista de propiedades más consultadas</returns>
    [HttpGet("most-consulted-properties")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMostConsultedProperties([FromQuery] int limit = 10)
    {
        limit = Math.Min(limit, 50); // Máximo 50 elementos

        var mostConsulted = await _context.PropertyConsults
            .Include(pc => pc.Property)
            .Where(pc => pc.CreatedAtUtc >= DateTime.UtcNow.AddDays(-30)) // Últimos 30 días
            .GroupBy(pc => new { pc.PropertyId, pc.Property.Title, pc.Property.Code, pc.Property.Price, pc.Property.Currency })
            .Select(g => new
            {
                propertyId = g.Key.PropertyId,
                code = g.Key.Code,
                title = g.Key.Title,
                price = g.Key.Price,
                currency = g.Key.Currency.ToString(),
                consultCount = g.Count()
            })
            .OrderByDescending(x => x.consultCount)
            .Take(limit)
            .ToListAsync();

        return Ok(new { data = mostConsulted });
    }

    /// <summary>
    /// Obtener estadísticas de simulaciones por mes
    /// </summary>
    /// <param name="months">Número de meses hacia atrás (default: 6)</param>
    /// <returns>Estadísticas mensuales de simulaciones</returns>
    [HttpGet("simulations-by-month")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetSimulationsByMonth([FromQuery] int months = 6)
    {
        months = Math.Min(months, 24); // Máximo 2 años
        var startDate = DateTime.UtcNow.AddMonths(-months);

        var simulationStats = await _context.LoanSimulations
            .Where(s => s.CreatedAtUtc >= startDate)
            .GroupBy(s => new { Year = s.CreatedAtUtc.Year, Month = s.CreatedAtUtc.Month })
            .Select(g => new
            {
                year = g.Key.Year,
                month = g.Key.Month,
                count = g.Count(),
                totalAmount = g.Sum(s => s.Principal),
                averageAmount = g.Average(s => s.Principal)
            })
            .OrderBy(x => x.year)
            .ThenBy(x => x.month)
            .ToListAsync();

        return Ok(new { data = simulationStats });
    }

    /// <summary>
    /// Obtener participación de bancos en simulaciones (últimos 3 meses)
    /// </summary>
    /// <returns>Estadísticas de selección de entidades financieras</returns>
    [HttpGet("entity-selection")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetEntitySelection()
    {
        var startDate = DateTime.UtcNow.AddMonths(-3);

        var totalSimulations = await _context.LoanSimulations
            .Where(s => s.CreatedAtUtc >= startDate && s.BankId.HasValue)
            .CountAsync();

        if (totalSimulations == 0)
        {
            return Ok(new { data = new List<object>() });
        }

        var bankStats = await _context.LoanSimulations
            .Where(s => s.CreatedAtUtc >= startDate && s.BankId.HasValue)
            .Include(s => s.Bank)
            .GroupBy(s => new { s.BankId, s.Bank!.Name })
            .Select(g => new
            {
                bankName = g.Key.Name,
                count = g.Count(),
                percentage = Math.Round((decimal)g.Count() / totalSimulations * 100, 2)
            })
            .OrderByDescending(x => x.count)
            .ToListAsync();

        return Ok(new { data = bankStats });
    }

    /// <summary>
    /// Obtener consultas de propiedades por mes (últimos 12 meses)
    /// </summary>
    /// <returns>Estadísticas mensuales de consultas de propiedades</returns>
    [HttpGet("property-consults-by-month")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPropertyConsultsByMonth()
    {
        var startDate = DateTime.UtcNow.AddMonths(-12);

        var consultStats = await _context.PropertyConsults
            .Where(pc => pc.CreatedAtUtc >= startDate)
            .GroupBy(pc => new { Year = pc.CreatedAtUtc.Year, Month = pc.CreatedAtUtc.Month })
            .Select(g => new
            {
                year = g.Key.Year,
                month = g.Key.Month,
                count = g.Count()
            })
            .OrderBy(x => x.year)
            .ThenBy(x => x.month)
            .ToListAsync();

        return Ok(new { data = consultStats });
    }
}