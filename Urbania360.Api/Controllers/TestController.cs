using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Urbania360.Api.Controllers;

/// <summary>
/// Controller de prueba para verificar autenticación
/// </summary>
[ApiController]
[Route("api/v1/test")]
[Produces("application/json")]
public class TestController : ControllerBase
{
    /// <summary>
    /// Endpoint público de prueba
    /// </summary>
    [HttpGet("public")]
    public ActionResult<object> GetPublic()
    {
        return Ok(new { message = "Endpoint público funcionando", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Endpoint protegido de prueba
    /// </summary>
    [HttpGet("protected")]
    [Authorize]
    public ActionResult<object> GetProtected()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
        var name = User.FindFirst("name")?.Value;

        var allClaims = User.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList();

        return Ok(new 
        { 
            message = "Endpoint protegido funcionando",
            user = new { userId, email, role, name },
            allClaims,
            timestamp = DateTime.UtcNow
        });
    }
}