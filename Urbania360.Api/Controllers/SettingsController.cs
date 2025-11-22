using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Urbania360.Api.DTOs.Banks;
using Urbania360.Api.DTOs.Users;
using Urbania360.Domain.Entities;
using Urbania360.Domain.Enums;
using Urbania360.Infrastructure.Data;

namespace Urbania360.Api.Controllers;

/// <summary>
/// Controller para configuraciones y perfil de usuario
/// </summary>
[ApiController]
[Route("api/v1/settings")]
[Authorize]
[Produces("application/json")]
public class SettingsController : ControllerBase
{
    private readonly UrbaniaDbContext _context;
    private readonly IMapper _mapper;

    public SettingsController(UrbaniaDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtener perfil del usuario actual
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponse>> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var user = await _context.Users
            .Include(u => u.UserPreference)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }

        var response = _mapper.Map<UserResponse>(user);
        return Ok(response);
    }

    /// <summary>
    /// Actualizar preferencias del usuario actual
    /// </summary>
    [HttpPut("preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var preference = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (preference == null)
        {
            // Crear preferencias si no existen
            preference = new UserPreference
            {
                UserId = userId,
                DefaultCurrency = request.DefaultCurrency ?? Currency.PEN,
                DefaultRateType = request.DefaultRateType ?? RateType.TEA
            };
            _context.UserPreferences.Add(preference);
        }
        else
        {
            // Actualizar preferencias existentes
            if (request.DefaultCurrency.HasValue)
            {
                preference.DefaultCurrency = request.DefaultCurrency.Value;
            }
            if (request.DefaultRateType.HasValue)
            {
                preference.DefaultRateType = request.DefaultRateType.Value;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Preferencias actualizadas correctamente" });
    }

    /// <summary>
    /// Obtener lista de entidades financieras
    /// </summary>
    [HttpGet("financial-entities")]
    [ProducesResponseType(typeof(List<BankResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BankResponse>>> GetFinancialEntities()
    {
        var banks = await _context.Banks
            .OrderBy(b => b.Name)
            .ToListAsync();

        var response = _mapper.Map<List<BankResponse>>(banks);
        return Ok(response);
    }

    /// <summary>
    /// Crear una nueva entidad financiera (solo Admin)
    /// </summary>
    [HttpPost("financial-entities")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BankResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BankResponse>> CreateFinancialEntity([FromBody] BankRequest request)
    {
        // Verificar si el nombre ya existe
        if (await _context.Banks.AnyAsync(b => b.Name == request.Name))
        {
            return Conflict(new { message = "El nombre del banco ya existe" });
        }

        var bank = new Bank
        {
            Name = request.Name,
            AnnualRateTea = request.AnnualRateTea,
            EffectiveFrom = request.EffectiveFrom ?? DateTime.UtcNow
        };

        _context.Banks.Add(bank);

        // Registrar actividad
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid userId))
        {
            var activityLog = new ActivityLog
            {
                UserId = userId,
                Action = "Create",
                Entity = "Bank",
                EntityId = bank.Id.ToString(),
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(activityLog);
        }

        await _context.SaveChangesAsync();

        var response = _mapper.Map<BankResponse>(bank);
        return CreatedAtAction(nameof(GetFinancialEntities), null, response);
    }

    /// <summary>
    /// Actualizar una entidad financiera (solo Admin)
    /// </summary>
    [HttpPut("financial-entities/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BankResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BankResponse>> UpdateFinancialEntity(int id, [FromBody] BankRequest request)
    {
        var bank = await _context.Banks.FindAsync(id);

        if (bank == null)
        {
            return NotFound(new { message = "Banco no encontrado" });
        }

        // Verificar si el nombre ya existe en otro banco
        if (await _context.Banks.AnyAsync(b => b.Name == request.Name && b.Id != id))
        {
            return Conflict(new { message = "El nombre del banco ya existe" });
        }

        bank.Name = request.Name;
        bank.AnnualRateTea = request.AnnualRateTea;
        if (request.EffectiveFrom.HasValue)
        {
            bank.EffectiveFrom = request.EffectiveFrom.Value;
        }

        // Registrar actividad
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid userId))
        {
            var activityLog = new ActivityLog
            {
                UserId = userId,
                Action = "Update",
                Entity = "Bank",
                EntityId = bank.Id.ToString(),
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(activityLog);
        }

        await _context.SaveChangesAsync();

        var response = _mapper.Map<BankResponse>(bank);
        return Ok(response);
    }

    /// <summary>
    /// Eliminar una entidad financiera (solo Admin)
    /// </summary>
    [HttpDelete("financial-entities/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> DeleteFinancialEntity(int id)
    {
        var bank = await _context.Banks
            .Include(b => b.LoanSimulations)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bank == null)
        {
            return NotFound(new { message = "Banco no encontrado" });
        }

        // Verificar si tiene simulaciones asociadas
        if (bank.LoanSimulations.Any())
        {
            return Conflict(new { message = "No se puede eliminar un banco con simulaciones asociadas" });
        }

        _context.Banks.Remove(bank);

        // Registrar actividad
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid userId))
        {
            var activityLog = new ActivityLog
            {
                UserId = userId,
                Action = "Delete",
                Entity = "Bank",
                EntityId = bank.Id.ToString(),
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(activityLog);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}

/// <summary>
/// Request para actualizar preferencias
/// </summary>
public class UpdatePreferencesRequest
{
    public Currency? DefaultCurrency { get; set; }
    public RateType? DefaultRateType { get; set; }
}
