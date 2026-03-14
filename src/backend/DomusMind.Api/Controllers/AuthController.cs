using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public AuthController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    /// Returns the health status of the auth subsystem. Anonymous.
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "ok" });
    }

    /// <summary>
    /// Returns the identity of the currently authenticated user.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        if (_currentUser.UserId is null)
        {
            return Unauthorized();
        }

        return Ok(new MeResponse(_currentUser.UserId.Value, _currentUser.Email));
    }
}
