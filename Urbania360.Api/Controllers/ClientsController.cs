using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Urbania360.Api.DTOs.Clients;
using Urbania360.Domain.Entities;
using Urbania360.Infrastructure.Data;

namespace Urbania360.Api.Controllers;

/// <summary>
/// Controller para gestión de clientes
/// </summary>
[ApiController]
[Route("api/v1/clients")]
[Authorize]
[Produces("application/json")]
public class ClientsController : ControllerBase
{
    private readonly UrbaniaDbContext _context;
    private readonly IMapper _mapper;

    public ClientsController(UrbaniaDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtener lista paginada de clientes
    /// </summary>
    /// <param name="search">Texto de búsqueda</param>
    /// <param name="page">Número de página (default: 1)</param>
    /// <param name="pageSize">Tamaño de página (default: 10)</param>
    /// <returns>Lista paginada de clientes</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetClients(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Min(pageSize, 100); // Máximo 100 elementos por página
        page = Math.Max(page, 1); // Mínimo página 1

        var query = _context.Clients
            .Include(c => c.CreatedByUser)
            .AsQueryable();

        // Aplicar filtro de búsqueda
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(search) ||
                c.LastName.ToLower().Contains(search) ||
                c.Email.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var clients = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var clientResponses = _mapper.Map<List<ClientResponse>>(clients);

        var result = new
        {
            data = clientResponses,
            pagination = new
            {
                currentPage = page,
                pageSize,
                totalCount,
                totalPages,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            }
        };

        return Ok(result);
    }

    /// <summary>
    /// Obtener un cliente por ID
    /// </summary>
    /// <param name="id">ID del cliente</param>
    /// <returns>Datos del cliente</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientResponse>> GetClient(Guid id)
    {
        var client = await _context.Clients
            .Include(c => c.CreatedByUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            return NotFound(new { message = "Cliente no encontrado" });
        }

        var response = _mapper.Map<ClientResponse>(client);
        return Ok(response);
    }

    /// <summary>
    /// Crear un nuevo cliente
    /// </summary>
    /// <param name="request">Datos del cliente</param>
    /// <returns>Cliente creado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientResponse>> CreateClient([FromBody] ClientCreateRequest request)
    {
        var currentUserId = GetCurrentUserId();

        var client = _mapper.Map<Client>(request);
        client.Id = Guid.NewGuid();
        client.CreatedByUserId = currentUserId;
        client.CreatedAtUtc = DateTime.UtcNow;

        _context.Clients.Add(client);

        // Crear log de actividad
        var activityLog = new ActivityLog
        {
            UserId = currentUserId,
            Action = "Create",
            Entity = "Client",
            EntityId = client.Id.ToString(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);

        await _context.SaveChangesAsync();

        // Cargar el cliente con la relación para la respuesta
        await _context.Entry(client)
            .Reference(c => c.CreatedByUser)
            .LoadAsync();

        var response = _mapper.Map<ClientResponse>(client);
        return CreatedAtAction(nameof(GetClient), new { id = client.Id }, response);
    }

    /// <summary>
    /// Actualizar un cliente existente
    /// </summary>
    /// <param name="id">ID del cliente</param>
    /// <param name="request">Datos actualizados del cliente</param>
    /// <returns>Cliente actualizado</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientResponse>> UpdateClient(Guid id, [FromBody] ClientUpdateRequest request)
    {
        var client = await _context.Clients
            .Include(c => c.CreatedByUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            return NotFound(new { message = "Cliente no encontrado" });
        }

        var currentUserId = GetCurrentUserId();

        // Actualizar propiedades
        client.FirstName = request.FirstName;
        client.LastName = request.LastName;
        client.Email = request.Email;
        client.Phone = request.Phone;
        client.AnnualIncome = request.AnnualIncome;

        // Crear log de actividad
        var activityLog = new ActivityLog
        {
            UserId = currentUserId,
            Action = "Update",
            Entity = "Client",
            EntityId = client.Id.ToString(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);

        await _context.SaveChangesAsync();

        var response = _mapper.Map<ClientResponse>(client);
        return Ok(response);
    }

    /// <summary>
    /// Eliminar un cliente
    /// </summary>
    /// <param name="id">ID del cliente</param>
    /// <returns>Confirmación de eliminación</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> DeleteClient(Guid id)
    {
        var client = await _context.Clients
            .Include(c => c.LoanSimulations)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            return NotFound(new { message = "Cliente no encontrado" });
        }

        // Verificar si tiene simulaciones de préstamo
        if (client.LoanSimulations.Any())
        {
            return Conflict(new { message = "No se puede eliminar el cliente porque tiene simulaciones de préstamo asociadas" });
        }

        var currentUserId = GetCurrentUserId();

        _context.Clients.Remove(client);

        // Crear log de actividad
        var activityLog = new ActivityLog
        {
            UserId = currentUserId,
            Action = "Delete",
            Entity = "Client",
            EntityId = client.Id.ToString(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Cliente eliminado exitosamente" });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                         User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Token inválido");
        }

        return userId;
    }
}