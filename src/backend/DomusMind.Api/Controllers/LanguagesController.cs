using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Features.Languages.GetSupportedLanguages;
using DomusMind.Contracts.Languages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Controllers;

[ApiController]
[Route("api/languages")]
public sealed class LanguagesController : ControllerBase
{
    /// <summary>Returns all active supported languages. Public endpoint - no authentication required.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SupportedLanguagesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupportedLanguages(
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var response = await dispatcher.Dispatch(
            new GetSupportedLanguagesQuery(),
            cancellationToken);

        return Ok(response);
    }
}
