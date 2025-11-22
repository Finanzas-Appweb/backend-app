using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Urbania360.Api.DTOs.Simulations;
using Urbania360.Domain.Entities;
using Urbania360.Domain.Enums;
using Urbania360.Infrastructure.Data;
using Urbania360.Infrastructure.Services;

namespace Urbania360.Api.Controllers;

/// <summary>
/// Controller para simulaciones de préstamos hipotecarios
/// </summary>
[ApiController]
[Route("api/v1/simulations")]
[Authorize] // Cualquier usuario autenticado puede acceder
[Produces("application/json")]
public class SimulationsController : ControllerBase
{
    private readonly UrbaniaDbContext _context;
    private readonly IMortgageCalculatorService _calculatorService;
    private readonly IMapper _mapper;

    public SimulationsController(
        UrbaniaDbContext context,
        IMortgageCalculatorService calculatorService,
        IMapper mapper)
    {
        _context = context;
        _calculatorService = calculatorService;
        _mapper = mapper;
    }

    /// <summary>
    /// Crear una nueva simulación de préstamo
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SimulationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SimulationResponse>> CreateSimulation([FromBody] SimulationRequest request)
    {
        // Verificar que el cliente existe
        var client = await _context.Clients.FindAsync(request.ClientId);
        if (client == null)
        {
            return NotFound(new { message = "Cliente no encontrado" });
        }

        // Verificar propiedad si se especificó
        Property? property = null;
        if (request.PropertyId.HasValue)
        {
            property = await _context.Properties.FindAsync(request.PropertyId.Value);
            if (property == null)
            {
                return NotFound(new { message = "Propiedad no encontrada" });
            }
        }

        // Verificar banco si se especificó
        Bank? bank = null;
        if (request.BankId.HasValue)
        {
            bank = await _context.Banks.FindAsync(request.BankId.Value);
            if (bank == null)
            {
                return NotFound(new { message = "Banco no encontrado" });
            }
        }

        // Normalizar bonusAmount: si no se aplica el bono MiVivienda, forzar a 0
        var bonusAmount = request.ApplyMiViviendaBonus ? request.BonusAmount : 0;

        // Preparar entrada para el calculador
        var input = new SimulationInput
        {
            Principal = request.Principal,
            Currency = request.Currency,
            RateType = request.RateType,
            TEA = request.TEA,
            TNA = request.TNA,
            CapitalizationPerYear = request.CapitalizationPerYear,
            TermMonths = request.TermMonths,
            GraceType = request.GraceType,
            GraceMonths = request.GraceMonths,
            StartDate = request.StartDate,
            ApplyMiViviendaBonus = request.ApplyMiViviendaBonus,
            BonusAmount = bonusAmount, // Usar bonusAmount normalizado
            LifeInsuranceRateMonthly = request.LifeInsuranceRateMonthly,
            RiskInsuranceRateAnnual = request.RiskInsuranceRateAnnual,
            FeesMonthly = request.FeesMonthly
        };

        // Calcular simulación
        var (result, schedule) = _calculatorService.Calculate(input);

        // Obtener usuario actual
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        // Crear entidad LoanSimulation
        var simulation = new LoanSimulation
        {
            Id = Guid.NewGuid(),
            ClientId = request.ClientId,
            PropertyId = request.PropertyId,
            BankId = request.BankId,
            Principal = request.Principal,
            Currency = request.Currency,
            RateType = request.RateType,
            TEA = request.TEA,
            TNA = request.TNA,
            CapitalizationPerYear = request.CapitalizationPerYear,
            TermMonths = request.TermMonths,
            GraceType = request.GraceType,
            GraceMonths = request.GraceMonths,
            StartDate = request.StartDate,
            ApplyMiViviendaBonus = request.ApplyMiViviendaBonus,
            BonusAmount = bonusAmount, // Usar bonusAmount normalizado
            LifeInsuranceRateMonthly = request.LifeInsuranceRateMonthly,
            RiskInsuranceRateAnnual = request.RiskInsuranceRateAnnual,
            FeesMonthly = request.FeesMonthly,
            TEM = result.TEM,
            MonthlyPayment = result.MonthlyPayment,
            TCEA = result.TCEA,
            VAN = result.VAN,
            TIR = result.TIR,
            TotalInterest = result.TotalInterest,
            TotalCost = result.TotalCost,
            CreatedByUserId = userId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.LoanSimulations.Add(simulation);

        // Agregar items de amortización
        foreach (var item in schedule)
        {
            item.SimulationId = simulation.Id;
            _context.AmortizationItems.Add(item);
        }

        // Registrar actividad
        var activityLog = new ActivityLog
        {
            UserId = userId,
            Action = "Create",
            Entity = "LoanSimulation",
            EntityId = simulation.Id.ToString(),
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.ActivityLogs.Add(activityLog);

        await _context.SaveChangesAsync();

        // Cargar relaciones para respuesta
        await _context.Entry(simulation)
            .Reference(s => s.Client)
            .LoadAsync();
        
        if (simulation.PropertyId.HasValue)
        {
            await _context.Entry(simulation)
                .Reference(s => s.Property)
                .LoadAsync();
        }

        if (simulation.BankId.HasValue)
        {
            await _context.Entry(simulation)
                .Reference(s => s.Bank)
                .LoadAsync();
        }

        var response = _mapper.Map<SimulationResponse>(simulation);
        response.AmortizationSchedule = _mapper.Map<List<AmortizationItemResponse>>(schedule);

        return CreatedAtAction(nameof(GetSimulation), new { id = simulation.Id }, response);
    }

    /// <summary>
    /// Obtener lista de simulaciones con filtros opcionales
    /// Admin y Agent ven todas las simulaciones, User solo ve las propias
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetSimulations(
        [FromQuery] Guid? clientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        // Obtener usuario actual
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        // Obtener rol del usuario
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        var userRole = Enum.TryParse<Role>(roleClaim, out var role) ? role : Role.User;

        var query = _context.LoanSimulations
            .Include(s => s.Client)
            .Include(s => s.Property)
            .AsQueryable();

        // Si el usuario es User (no Admin ni Agent), solo puede ver simulaciones de sus propios clientes
        if (userRole == Role.User)
        {
            // Filtrar por clientes creados por el usuario actual
            query = query.Where(s => s.Client.CreatedByUserId == userId);
        }

        if (clientId.HasValue)
        {
            query = query.Where(s => s.ClientId == clientId.Value);
        }

        var total = await query.CountAsync();

        var simulations = await query
            .OrderByDescending(s => s.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = _mapper.Map<List<SimulationSummaryResponse>>(simulations);

        return Ok(new
        {
            data = response,
            pagination = new
            {
                page,
                pageSize,
                total,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            }
        });
    }

    /// <summary>
    /// Obtener una simulación por ID con su tabla de amortización
    /// Admin y Agent pueden ver cualquier simulación, User solo las propias
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SimulationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SimulationResponse>> GetSimulation(Guid id)
    {
        var simulation = await _context.LoanSimulations
            .Include(s => s.Client)
            .Include(s => s.Property)
            .Include(s => s.Bank)
            .Include(s => s.AmortizationItems)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (simulation == null)
        {
            return NotFound(new { message = "Simulación no encontrada" });
        }

        // Obtener usuario actual
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        // Obtener rol del usuario
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        var userRole = Enum.TryParse<Role>(roleClaim, out var role) ? role : Role.User;

        // Si el usuario es User (no Admin ni Agent), solo puede ver simulaciones de sus propios clientes
        if (userRole == Role.User && simulation.Client.CreatedByUserId != userId)
        {
            return Forbid(); // 403 Forbidden
        }

        var response = _mapper.Map<SimulationResponse>(simulation);
        return Ok(response);
    }

    /// <summary>
    /// Eliminar una simulación y su cronograma de amortización
    /// Admin y Agent pueden eliminar cualquier simulación, User solo las de sus propios clientes
    /// </summary>
    /// <param name="id">ID de la simulación</param>
    /// <returns>Confirmación de eliminación</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteSimulation(Guid id)
    {
        // Buscar la simulación con el cliente asociado
        var simulation = await _context.LoanSimulations
            .Include(s => s.Client)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (simulation == null)
        {
            return NotFound(new { message = "Simulación no encontrada" });
        }

        // Obtener usuario y rol actual
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        // Obtener rol del usuario
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        var userRole = Enum.TryParse<Role>(roleClaim, out var role) ? role : Role.User;

        // Validar autorización
        if (userRole == Role.User)
        {
            // User solo puede eliminar simulaciones de clientes creados por él
            var client = simulation.Client;
            if (client.CreatedByUserId != userId)
            {
                return Forbid(); // 403 Forbidden
            }
        }
        // Admin y Agent pueden eliminar cualquier simulación (sin restricción)

        // Eliminar la simulación (AmortizationItems se eliminan en cascada por DeleteBehavior.Cascade)
        _context.LoanSimulations.Remove(simulation);

        // Registrar actividad
        var activityLog = new ActivityLog
        {
            UserId = userId,
            Action = "Delete",
            Entity = "LoanSimulation",
            EntityId = simulation.Id.ToString(),
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.ActivityLogs.Add(activityLog);

        await _context.SaveChangesAsync();

        return NoContent(); // 204 No Content
    }
}
