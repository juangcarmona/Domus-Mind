using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.GetMemberAgenda;
using DomusMind.Contracts.Calendar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Controllers;

[ApiController]
[Route("api/families/{familyId:guid}/members/{memberId:guid}/agenda")]
[Authorize]
public sealed class MemberAgendaController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public MemberAgendaController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    /// Returns the member agenda for a given date window,
    /// merging native plans with read-only external calendar entries.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(MemberAgendaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMemberAgenda(
        Guid familyId,
        Guid memberId,
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await dispatcher.Dispatch(
                new GetMemberAgendaQuery(familyId, memberId, from, to, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(result);
        }
        catch (CalendarException ex)
        {
            return ex.Code switch
            {
                CalendarErrorCode.AccessDenied =>
                    StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message }),
                _ =>
                    StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message }),
            };
        }
    }
}
