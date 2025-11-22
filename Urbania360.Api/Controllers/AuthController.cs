using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Urbania360.Api.DTOs.Auth;
using Urbania360.Domain.Entities;
using Urbania360.Domain.Enums;
using Urbania360.Infrastructure.Data;
using Urbania360.Infrastructure.Services;
using BCrypt.Net;

namespace Urbania360.Api.Controllers;

/// <summary>
/// Controller para autenticación de usuarios
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UrbaniaDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMapper _mapper;

    public AuthController(UrbaniaDbContext context, IJwtTokenService jwtTokenService, IMapper mapper)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _mapper = mapper;
    }

    /// <summary>
    /// Registrar un nuevo usuario
    /// </summary>
    /// <param name="request">Datos del usuario a registrar</param>
    /// <returns>Respuesta de autenticación con token</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        // Verificar si el username ya existe
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return Conflict(new { message = "El nombre de usuario ya está registrado" });
        }

        // Verificar si el email ya existe
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return Conflict(new { message = "El email ya está registrado" });
        }

        // Crear nuevo usuario con rol User por defecto
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Dni = request.Dni,
            FullName = $"{request.FirstName} {request.LastName}",
            Email = request.Email,
            Phone = request.Phone,
            Role = Role.User, // Todos los usuarios registrados obtienen rol User por defecto
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);

        // Crear preferencias por defecto
        var preference = new UserPreference
        {
            UserId = user.Id,
            DefaultCurrency = Currency.PEN,
            DefaultRateType = RateType.TEA
        };

        _context.UserPreferences.Add(preference);

        // Crear log de actividad
        var activityLog = new ActivityLog
        {
            UserId = user.Id,
            Action = "Register",
            Entity = "User",
            EntityId = user.Id.ToString(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);

        await _context.SaveChangesAsync();

        // Generar token
        var token = _jwtTokenService.GenerateToken(user);
        var userInfo = _mapper.Map<UserInfo>(user);

        var response = new AuthResponse
        {
            Token = token,
            User = userInfo
        };

        return CreatedAtAction(nameof(Register), null, response);
    }

    /// <summary>
    /// Iniciar sesión
    /// </summary>
    /// <param name="request">Credenciales de login</param>
    /// <returns>Respuesta de autenticación con token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        // Buscar usuario por email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        if (user == null)
        {
            return Unauthorized(new { message = "Credenciales inválidas" });
        }

        // Verificar contraseña
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Credenciales inválidas" });
        }

        // Crear log de actividad
        var activityLog = new ActivityLog
        {
            UserId = user.Id,
            Action = "Login",
            Entity = "User",
            EntityId = user.Id.ToString(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync();

        // Generar token
        var token = _jwtTokenService.GenerateToken(user);
        var userInfo = _mapper.Map<UserInfo>(user);

        var response = new AuthResponse
        {
            Token = token,
            User = userInfo
        };

        return Ok(response);
    }
}