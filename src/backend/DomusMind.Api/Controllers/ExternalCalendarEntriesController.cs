using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.GetExternalCalendarEntry;
using DomusMind.Contracts.Calendar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Controllers;

/// <summary>
/// Provides direct access to persisted external calendar entries.
/// Entries are read-only; they are imported by the sync process.
/// </summary>
[ApiController]
[Route("api/families/{familyId:guid}/members/{memberId:guid}/external-calendar-entries")]
[Authorize]
public sealed class ExternalCalendarEntriesController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public ExternalCalendarEntriesController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>Returns a single persisted external calendar entry by its stored ID.</summary>
    [HttpGet("{entryId:guid}")]
    [ProducesResponseType(typeof(GetExternalCalendarEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExternalCalendarEntry(
        Guid familyId,
        Guid memberId,
        Guid entryId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await dispatcher.Dispatch(
                new GetExternalCalendarEntryQuery(familyId, memberId, entryId, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(result);
        }
        catch (CalendarException ex)
        {
            return ex.Code switch
            {
                CalendarErrorCode.EventNotFound =>
                    NotFound(new { error = ex.Message }),
                CalendarErrorCode.AccessDenied =>
                    StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message }),
                _ =>
                    StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message }),
            };
        }
    }
}
