using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Features.Platform.GetCurrentDeploymentMode;
using DomusMind.Contracts.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Controllers;

[ApiController]
[Route("api/platform")]
public sealed class PlatformController : ControllerBase
{
    /// <summary>
    /// Returns the active deployment mode and effective runtime capabilities.
    /// Used by the UI and onboarding flow to adapt without forking feature logic.
    /// </summary>
    [HttpGet("deployment-mode")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DeploymentModeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeploymentMode(
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var result = await dispatcher.Dispatch(new GetCurrentDeploymentModeQuery(), cancellationToken);
        return Ok(result);
    }
}
