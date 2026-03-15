using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.AddEventParticipant;
using DomusMind.Application.Features.Calendar.AddReminder;
using DomusMind.Application.Features.Calendar.CancelEvent;
using DomusMind.Application.Features.Calendar.DetectCalendarConflicts;
using DomusMind.Application.Features.Calendar.GetFamilyPlans;
using DomusMind.Application.Features.Calendar.GetFamilyTimeline;
using DomusMind.Application.Features.Calendar.ProposeAlternativeTimes;
using DomusMind.Application.Features.Calendar.RemoveEventParticipant;
using DomusMind.Application.Features.Calendar.RemoveReminder;
using DomusMind.Application.Features.Calendar.RescheduleEvent;
using DomusMind.Application.Features.Calendar.ScheduleEvent;
using DomusMind.Application.Features.Calendar.SuggestEventParticipants;
using DomusMind.Contracts.Calendar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Controllers;

[ApiController]
[Route("api/events")]
[Authorize]
public sealed class EventsController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public EventsController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>Schedules a new event for a family.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ScheduleEventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ScheduleEvent(
        [FromBody] ScheduleEventRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new ScheduleEventCommand(
                    request.Title,
                    request.FamilyId,
                    request.StartTime,
                    request.EndTime,
                    request.Description,
                    _currentUser.UserId!.Value),
                cancellationToken);

            return Created($"/api/events/{response.CalendarEventId}", response);
        }
        catch (CalendarException ex)
        {
            return MapCalendarException(ex);
        }
    }

    /// <summary>Returns all events for a family, ordered by start time.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(FamilyTimelineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFamilyTimeline(
        [FromQuery] Guid familyId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetFamilyTimelineQuery(familyId, from, to, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (CalendarException ex)
        {
            return MapCalendarException(ex);
        }
    }

    /// <summary>Reschedules an existing event.</summary>
    [HttpPost("{id:guid}/reschedule")]
    [ProducesResponseType(typeof(RescheduleEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RescheduleEvent(
        Guid id,
        [FromBody] RescheduleEventRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new RescheduleEventCommand(id, request.NewStartTime, request.NewEndTime, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (CalendarException ex)
        {
            return MapCalendarException(ex);
        }
    }

    /// <summary>Cancels an event.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(CancelEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelEvent(
        Guid id,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new CancelEventCommand(id, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (CalendarException ex)
        {
            return MapCalendarException(ex);
        }
    }

    /// <summary>Adds a participant to an event.</summary>
    [HttpPost("{id:guid}/participants")]
    [ProducesResponseType(typeof(AddEventParticipantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddEventParticipant(
        Guid id,
        [FromBody] AddEventParticipantRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new AddEventParticipantCommand(id, request.MemberId, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (CalendarException ex)
        {
            return MapCalendarException(ex);
        }
    }

    /// <summary>Removes a participant from an event.</summary>
    [HttpDelete("{id:guid}/participants/{memberId:guid}")]
    [ProducesResponseType(typeof(RemoveEventParticipantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveEventParticipant(
        Guid id,
        Guid memberId,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new RemoveEventParticipantCommand(id, memberId, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (CalendarException ex)
        {
            return MapCalendarException(ex);
        }
    }

    /// <summary>Adds a reminder to an event.</summary>
    [HttpPost("{id:guid}/reminders")]
    [ProducesResponseType(typeof(AddReminderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddReminder(
        Guid id,
        [FromBody] AddReminderRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new AddReminderCommand(id, request.MinutesBefore, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (CalendarException ex)
        {
            return MapCalendarException(ex);
        }
    }

    /// <summary>Removes a reminder from an event.</summary>
    [HttpDelete("{id:guid}/reminders/{minutesBefore:int}")]
    [ProducesResponseType(typeof(RemoveReminderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveReminder(
        Guid id,
        int minutesBefore,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new RemoveReminderCommand(id, minutesBefore, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (CalendarException ex)
        {
            return MapCalendarException(ex);
        }
    }

    /// <summary>Detects scheduling conflicts between family calendar events.</summary>
    [HttpGet("conflicts")]
    [ProducesResponseType(typeof(CalendarConflictsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DetectCalendarConflicts(
        [FromQuery] Guid familyId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime? to,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new DetectCalendarConflictsQuery(familyId, from, to, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (CalendarException ex)
        {
            return MapCalendarException(ex);
        }
    }

    /// <summary>Suggests family members to add as participants to an event.</summary>
    [HttpGet("{id:guid}/suggest-participants")]
    [ProducesResponseType(typeof(SuggestEventParticipantsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuggestEventParticipants(
        Guid id,
        [FromQuery] Guid familyId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new SuggestEventParticipantsQuery(familyId, id, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (CalendarException ex)
        {
            return MapCalendarException(ex);
        }
    }

    /// <summary>Proposes alternative time slots for an event to avoid conflicts.</summary>
    [HttpGet("{id:guid}/alternative-times")]
    [ProducesResponseType(typeof(ProposeAlternativeTimesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProposeAlternativeTimes(
        Guid id,
        [FromQuery] Guid familyId,
        [FromQuery] int count,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var suggestionCount = count > 0 ? count : 3;
            var response = await dispatcher.Dispatch(
                new ProposeAlternativeTimesQuery(familyId, id, suggestionCount, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (CalendarException ex)
        {
            return MapCalendarException(ex);
        }
    }

    /// <summary>Returns family plans with optional member-scoped visibility filtering.</summary>
    [HttpGet("plans")]
    [ProducesResponseType(typeof(FamilyPlansResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFamilyPlans(
        [FromQuery] Guid familyId,
        [FromQuery] Guid? memberId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetFamilyPlansQuery(familyId, memberId, from, to, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (CalendarException ex)
        {
            return MapCalendarException(ex);
        }
    }

    private IActionResult MapCalendarException(CalendarException ex) => ex.Code switch
    {
        CalendarErrorCode.EventNotFound =>
            NotFound(new { error = ex.Message }),
        CalendarErrorCode.AccessDenied =>
            StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message }),
        CalendarErrorCode.EventAlreadyCancelled =>
            Conflict(new { error = ex.Message }),
        CalendarErrorCode.DuplicateParticipant =>
            Conflict(new { error = ex.Message }),
        CalendarErrorCode.ParticipantNotFound =>
            NotFound(new { error = ex.Message }),
        CalendarErrorCode.DuplicateReminderOffset =>
            Conflict(new { error = ex.Message }),
        CalendarErrorCode.ReminderOffsetNotFound =>
            NotFound(new { error = ex.Message }),
        CalendarErrorCode.InvalidInput =>
            BadRequest(new { error = ex.Message }),
        _ =>
            StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message }),
    };
}
