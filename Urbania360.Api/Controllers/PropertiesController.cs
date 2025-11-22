using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Urbania360.Api.DTOs.Properties;
using Urbania360.Domain.Entities;
using Urbania360.Infrastructure.Data;

namespace Urbania360.Api.Controllers;

/// <summary>
/// Controller para gestión de propiedades
/// </summary>
[ApiController]
[Route("api/v1/properties")]
[Authorize]
[Produces("application/json")]
public class PropertiesController : ControllerBase
{
    private readonly UrbaniaDbContext _context;
    private readonly IMapper _mapper;

    public PropertiesController(UrbaniaDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtener lista paginada de propiedades con búsqueda
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetProperties(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Properties
            .Include(p => p.PropertyImages)
            .Include(p => p.PropertyConsults)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(searchLower) ||
                p.Code.ToLower().Contains(searchLower) ||
                p.Address.ToLower().Contains(searchLower) ||
                p.District.ToLower().Contains(searchLower));
        }

        var total = await query.CountAsync();

        var properties = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = properties.Select(p => new PropertySummaryResponse
        {
            Id = p.Id,
            Code = p.Code,
            Title = p.Title,
            District = p.District,
            Type = p.Type,
            Price = p.Price,
            Currency = p.Currency,
            ThumbnailUrl = p.PropertyImages.FirstOrDefault()?.Url,
            ConsultsCount = p.PropertyConsults.Count
        }).ToList();

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
    /// Obtener una propiedad por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PropertyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyResponse>> GetProperty(Guid id)
    {
        var property = await _context.Properties
            .Include(p => p.PropertyImages)
            .Include(p => p.PropertyConsults)
            .Include(p => p.CreatedByUser)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null)
        {
            return NotFound(new { message = "Propiedad no encontrada" });
        }

        // Registrar consulta si hay usuario autenticado
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid userId))
        {
            var consult = new PropertyConsult
            {
                PropertyId = id,
                UserId = userId,
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.PropertyConsults.Add(consult);
            await _context.SaveChangesAsync();
        }

        var response = _mapper.Map<PropertyResponse>(property);
        return Ok(response);
    }

    /// <summary>
    /// Crear una nueva propiedad
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Agent")]
    [ProducesResponseType(typeof(PropertyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PropertyResponse>> CreateProperty([FromBody] PropertyCreateRequest request)
    {
        // Verificar si el código ya existe
        if (await _context.Properties.AnyAsync(p => p.Code == request.Code))
        {
            return Conflict(new { message = "El código de propiedad ya existe" });
        }

        // Obtener usuario actual
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(new { message = "Usuario no autenticado" });
        }

        var property = new Property
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Title = request.Title,
            Address = request.Address,
            District = request.District,
            Province = request.Province,
            Type = request.Type,
            AreaM2 = request.AreaM2,
            Price = request.Price,
            Currency = request.Currency,
            CreatedByUserId = userId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Properties.Add(property);

        // Agregar imágenes
        foreach (var imageUrl in request.ImagesUrl)
        {
            var image = new PropertyImage
            {
                PropertyId = property.Id,
                Url = imageUrl
            };
            _context.PropertyImages.Add(image);
        }

        // Registrar actividad
        var activityLog = new ActivityLog
        {
            UserId = userId,
            Action = "Create",
            Entity = "Property",
            EntityId = property.Id.ToString(),
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.ActivityLogs.Add(activityLog);

        await _context.SaveChangesAsync();

        // Cargar relaciones
        await _context.Entry(property)
            .Collection(p => p.PropertyImages)
            .LoadAsync();
        await _context.Entry(property)
            .Reference(p => p.CreatedByUser)
            .LoadAsync();

        var response = _mapper.Map<PropertyResponse>(property);
        return CreatedAtAction(nameof(GetProperty), new { id = property.Id }, response);
    }

    /// <summary>
    /// Actualizar una propiedad
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Agent")]
    [ProducesResponseType(typeof(PropertyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PropertyResponse>> UpdateProperty(Guid id, [FromBody] PropertyUpdateRequest request)
    {
        var property = await _context.Properties
            .Include(p => p.PropertyImages)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null)
        {
            return NotFound(new { message = "Propiedad no encontrada" });
        }

        // Verificar si el código ya existe en otra propiedad
        if (await _context.Properties.AnyAsync(p => p.Code == request.Code && p.Id != id))
        {
            return Conflict(new { message = "El código de propiedad ya existe" });
        }

        // Actualizar campos
        property.Code = request.Code;
        property.Title = request.Title;
        property.Address = request.Address;
        property.District = request.District;
        property.Province = request.Province;
        property.Type = request.Type;
        property.AreaM2 = request.AreaM2;
        property.Price = request.Price;
        property.Currency = request.Currency;

        // Sincronizar imágenes: eliminar las que no están en la nueva lista
        var existingImages = property.PropertyImages.ToList();
        var newImageUrls = request.ImagesUrl;

        // Eliminar imágenes que ya no están
        foreach (var existingImage in existingImages)
        {
            if (!newImageUrls.Contains(existingImage.Url))
            {
                _context.PropertyImages.Remove(existingImage);
            }
        }

        // Agregar nuevas imágenes
        var existingUrls = existingImages.Select(i => i.Url).ToList();
        foreach (var imageUrl in newImageUrls.Where(url => !existingUrls.Contains(url)))
        {
            var image = new PropertyImage
            {
                PropertyId = property.Id,
                Url = imageUrl
            };
            _context.PropertyImages.Add(image);
        }

        // Registrar actividad
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid userId))
        {
            var activityLog = new ActivityLog
            {
                UserId = userId,
                Action = "Update",
                Entity = "Property",
                EntityId = property.Id.ToString(),
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(activityLog);
        }

        await _context.SaveChangesAsync();

        // Recargar relaciones
        await _context.Entry(property)
            .Collection(p => p.PropertyImages)
            .LoadAsync();
        await _context.Entry(property)
            .Reference(p => p.CreatedByUser)
            .LoadAsync();

        var response = _mapper.Map<PropertyResponse>(property);
        return Ok(response);
    }

    /// <summary>
    /// Eliminar una propiedad
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Agent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> DeleteProperty(Guid id)
    {
        var property = await _context.Properties
            .Include(p => p.LoanSimulations)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null)
        {
            return NotFound(new { message = "Propiedad no encontrada" });
        }

        // Verificar si tiene simulaciones asociadas
        if (property.LoanSimulations.Any())
        {
            return Conflict(new { message = "No se puede eliminar una propiedad con simulaciones asociadas" });
        }

        _context.Properties.Remove(property);

        // Registrar actividad
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid userId))
        {
            var activityLog = new ActivityLog
            {
                UserId = userId,
                Action = "Delete",
                Entity = "Property",
                EntityId = property.Id.ToString(),
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(activityLog);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
