using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Urbania360.Api.DTOs.Banks;
using Urbania360.Domain.Entities;
using Urbania360.Infrastructure.Data;

namespace Urbania360.Api.Controllers;

/// <summary>
/// Controller para gestión de bancos
/// </summary>
[ApiController]
[Route("api/v1/banks")]
[Authorize]
[Produces("application/json")]
public class BanksController : ControllerBase
{
    private readonly UrbaniaDbContext _context;
    private readonly IMapper _mapper;

    public BanksController(UrbaniaDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtener lista de bancos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<BankResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BankResponse>>> GetBanks()
    {
        var banks = await _context.Banks
            .OrderBy(b => b.Name)
            .ToListAsync();

        var response = _mapper.Map<List<BankResponse>>(banks);
        return Ok(response);
    }

    /// <summary>
    /// Obtener un banco por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BankResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BankResponse>> GetBank(int id)
    {
        var bank = await _context.Banks.FindAsync(id);

        if (bank == null)
        {
            return NotFound(new { message = "Banco no encontrado" });
        }

        var response = _mapper.Map<BankResponse>(bank);
        return Ok(response);
    }

    /// <summary>
    /// Crear un nuevo banco
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BankResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BankResponse>> CreateBank([FromBody] BankRequest request)
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
        return CreatedAtAction(nameof(GetBank), new { id = bank.Id }, response);
    }

    /// <summary>
    /// Actualizar un banco
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BankResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BankResponse>> UpdateBank(int id, [FromBody] BankRequest request)
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
    /// Eliminar un banco
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> DeleteBank(int id)
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
