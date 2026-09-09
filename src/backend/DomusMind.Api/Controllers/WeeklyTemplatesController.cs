using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.MealPlanning.CreateWeeklyTemplate;
using DomusMind.Contracts.MealPlanning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Controllers;

[ApiController]
[Route("api/weekly-templates")]
[Authorize]
public sealed class WeeklyTemplatesController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public WeeklyTemplatesController(ICurrentUser currentUser) => _currentUser = currentUser;

    /// <summary>Creates a new weekly meal plan template for a family.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateWeeklyTemplateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateWeeklyTemplate(
        [FromBody] CreateWeeklyTemplateRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new CreateWeeklyTemplateCommand(
                    request.TemplateId, request.FamilyId, request.Name,
                    request.Slots, _currentUser.UserId!.Value),
                cancellationToken);
            return Created($"/api/weekly-templates/{response.TemplateId}", response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
