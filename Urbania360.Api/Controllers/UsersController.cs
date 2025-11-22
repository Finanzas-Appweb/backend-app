using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Urbania360.Api.DTOs.Users;
using Urbania360.Infrastructure.Data;

namespace Urbania360.Api.Controllers;

/// <summary>
/// Controller para gestión de usuarios
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly UrbaniaDbContext _context;
    private readonly IMapper _mapper;

    public UsersController(UrbaniaDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtener un usuario por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetUser(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.UserPreference)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }

        var response = _mapper.Map<UserResponse>(user);
        return Ok(response);
    }

    /// <summary>
    /// Obtener lista de usuarios
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.UserPreference)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var response = _mapper.Map<List<UserResponse>>(users);
        return Ok(response);
    }

    /// <summary>
    /// Actualizar un usuario
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        // Verificar que el usuario actual solo pueda editar su propio perfil (excepto Admin)
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");
        
        if (string.IsNullOrEmpty(currentUserIdClaim) || !Guid.TryParse(currentUserIdClaim, out Guid currentUserId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        if (!isAdmin && currentUserId != id)
        {
            return Forbid();
        }

        var user = await _context.Users
            .Include(u => u.UserPreference)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }

        // Verificar si el username ya existe en otro usuario
        if (await _context.Users.AnyAsync(u => u.Username == request.Username && u.Id != id))
        {
            return Conflict(new { message = "El nombre de usuario ya está en uso" });
        }

        // Verificar si el email ya existe en otro usuario
        if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
        {
            return Conflict(new { message = "El email ya está en uso" });
        }

        // Actualizar campos del usuario
        user.Username = request.Username;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Dni = request.Dni;
        user.FullName = $"{request.FirstName} {request.LastName}";
        user.Email = request.Email;
        user.Phone = request.Phone;

        // Actualizar preferencias
        if (user.UserPreference != null)
        {
            if (request.DefaultCurrency.HasValue)
            {
                user.UserPreference.DefaultCurrency = request.DefaultCurrency.Value;
            }
            if (request.DefaultRateType.HasValue)
            {
                user.UserPreference.DefaultRateType = request.DefaultRateType.Value;
            }
        }
        else if (request.DefaultCurrency.HasValue || request.DefaultRateType.HasValue)
        {
            // Crear preferencias si no existen
            user.UserPreference = new Domain.Entities.UserPreference
            {
                UserId = user.Id,
                DefaultCurrency = request.DefaultCurrency ?? Domain.Enums.Currency.PEN,
                DefaultRateType = request.DefaultRateType ?? Domain.Enums.RateType.TEA
            };
            _context.UserPreferences.Add(user.UserPreference);
        }

        // Registrar actividad
        var activityLog = new Domain.Entities.ActivityLog
        {
            UserId = currentUserId,
            Action = "Update",
            Entity = "User",
            EntityId = user.Id.ToString(),
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.ActivityLogs.Add(activityLog);

        await _context.SaveChangesAsync();

        var response = _mapper.Map<UserResponse>(user);
        return Ok(response);
    }
}
