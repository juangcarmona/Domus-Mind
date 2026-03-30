using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Features.Setup;
using DomusMind.Application.Features.Setup.GetSetupStatus;
using DomusMind.Application.Features.Setup.InitializeSystem;
using DomusMind.Contracts.Setup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Controllers;

[ApiController]
[Route("api/setup")]
public sealed class SetupController : ControllerBase
{
    /// <summary>
    /// Returns whether the system has been initialized.
    /// Use this to determine if a first-run setup wizard should be shown.
    /// </summary>
    [HttpGet("status")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SetupStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var result = await dispatcher.Dispatch(new GetSetupStatusQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Initializes the system by creating the first administrator account.
    /// This endpoint is only usable once. Subsequent calls return 409 Conflict.
    /// </summary>
    [HttpPost("initialize")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(InitializeSystemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Initialize(
        [FromBody] InitializeSystemRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new InitializeSystemCommand(request.Email, request.Password, request.DisplayName),
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (SetupException ex) when (ex.Code == SetupErrorCode.AlreadyInitialized)
        {
            return Conflict(new { code = "setup.already_initialized", message = ex.Message });
        }
        catch (SetupException ex) when (ex.Code == SetupErrorCode.WeakPassword)
        {
            return BadRequest(new { code = "setup.weak_password", message = ex.Message });
        }
        catch (SetupException ex) when (ex.Code == SetupErrorCode.EmailAlreadyTaken)
        {
            return Conflict(new { code = "setup.email_already_taken", message = ex.Message });
        }
        catch (SetupException ex) when (ex.Code == SetupErrorCode.NotApplicable)
        {
            return BadRequest(new { code = "setup.not_applicable", message = ex.Message });
        }
    }
}
