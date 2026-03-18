using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.AddMember;
using DomusMind.Application.Features.Family.CompleteOnboarding;
using DomusMind.Application.Features.Family.CreateFamily;
using DomusMind.Application.Features.Family.UpdateFamilySettings;
using DomusMind.Application.Features.Family.GetEnrichedTimeline;
using DomusMind.Application.Features.Family.GetFamily;
using DomusMind.Application.Features.Family.GetFamilyMembers;
using DomusMind.Application.Features.Family.GetHouseholdTimeline;
using DomusMind.Application.Features.Family.GetMemberActivity;
using DomusMind.Application.Features.Family.GetMyFamily;
using DomusMind.Application.Features.Family.GetWeeklyGrid;
using DomusMind.Contracts.Family;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Controllers;

[ApiController]
[Route("api/families")]
[Authorize]
public sealed class FamiliesController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public FamiliesController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>Returns the family that belongs to the authenticated user, if one exists.</summary>
    [HttpGet("mine")]
    [ProducesResponseType(typeof(FamilyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyFamily(
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetMyFamilyQuery(_currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (FamilyException ex) when (ex.Code == FamilyErrorCode.FamilyNotFound)
        {
            return NotFound();
        }
    }

    /// <summary>Creates a new family. The authenticated user is granted access to the created family.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateFamilyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateFamily(
        [FromBody] CreateFamilyRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new CreateFamilyCommand(request.Name, request.PrimaryLanguageCode, _currentUser.UserId!.Value),
                cancellationToken);

            return CreatedAtAction(nameof(GetFamily), new { familyId = response.FamilyId }, response);
        }
        catch (FamilyException ex) when (ex.Code == FamilyErrorCode.InvalidInput)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (FamilyException ex) when (ex.Code == FamilyErrorCode.FamilyAlreadyExists)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>Adds a member to an existing family.</summary>
    [HttpPost("{familyId:guid}/members")]
    [ProducesResponseType(typeof(AddMemberResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMember(
        Guid familyId,
        [FromBody] AddMemberRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new AddMemberCommand(familyId, request.Name, request.Role, _currentUser.UserId!.Value),
                cancellationToken);

            return Created(
                $"/api/families/{familyId}/members/{response.MemberId}",
                response);
        }
        catch (FamilyException ex)
        {
            return MapFamilyException(ex);
        }
    }

    /// <summary>Completes household onboarding: adds the creator as manager plus optional additional members.</summary>
    [HttpPost("{familyId:guid}/onboarding")]
    [ProducesResponseType(typeof(CompleteOnboardingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteOnboarding(
        Guid familyId,
        [FromBody] CompleteOnboardingRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var additionalMembers = request.AdditionalMembers?
                .Select(m => new AdditionalMemberInput(m.Name, m.BirthDate, m.Type, m.Manager))
                .ToList()
                ?? [];

            var response = await dispatcher.Dispatch(
                new CompleteOnboardingCommand(
                    familyId,
                    _currentUser.UserId!.Value,
                    request.SelfName,
                    request.SelfBirthDate,
                    additionalMembers),
                cancellationToken);

            return Ok(response);
        }
        catch (FamilyException ex)
        {
            return MapFamilyException(ex);
        }
    }

    /// <summary>Updates household settings: name, primary language, first day of week, date format.</summary>
    [HttpPut("{familyId:guid}/settings")]
    [ProducesResponseType(typeof(UpdateFamilySettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSettings(
        Guid familyId,
        [FromBody] UpdateFamilySettingsRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new UpdateFamilySettingsCommand(
                    familyId,
                    _currentUser.UserId!.Value,
                    request.Name,
                    request.PrimaryLanguageCode,
                    request.FirstDayOfWeek,
                    request.DateFormatPreference),
                cancellationToken);

            return Ok(response);
        }
        catch (FamilyException ex)
        {
            return MapFamilyException(ex);
        }
    }

    /// <summary>Returns the family with the given id.</summary>
    [HttpGet("{familyId:guid}")]
    [ProducesResponseType(typeof(FamilyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFamily(
        Guid familyId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetFamilyQuery(familyId, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (FamilyException ex)
        {
            return MapFamilyException(ex);
        }
    }

    /// <summary>Returns all members of the given family.</summary>
    [HttpGet("{familyId:guid}/members")]
    [ProducesResponseType(typeof(IReadOnlyCollection<FamilyMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFamilyMembers(
        Guid familyId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetFamilyMembersQuery(familyId, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (FamilyException ex)
        {
            return MapFamilyException(ex);
        }
    }

    /// <summary>Returns a unified chronological timeline of events, tasks, and routines for the given family.</summary>
    [HttpGet("{familyId:guid}/timeline")]
    [ProducesResponseType(typeof(HouseholdTimelineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHouseholdTimeline(
        Guid familyId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetHouseholdTimelineQuery(familyId, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (FamilyException ex)
        {
            return MapFamilyException(ex);
        }
    }

    /// <summary>Returns all cross-context activity for a specific family member.</summary>
    [HttpGet("{familyId:guid}/members/{memberId:guid}/activity")]
    [ProducesResponseType(typeof(MemberActivityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMemberActivity(
        Guid familyId,
        Guid memberId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetMemberActivityQuery(familyId, memberId, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (FamilyException ex)
        {
            return MapFamilyException(ex);
        }
    }

    /// <summary>Returns an enriched, grouped, and filterable household timeline.</summary>
    [HttpGet("{familyId:guid}/timeline/enriched")]
    [ProducesResponseType(typeof(EnrichedTimelineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEnrichedTimeline(
        Guid familyId,
        [FromQuery] string? types,
        [FromQuery] Guid? memberFilter,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? statuses,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var typeFilter = string.IsNullOrWhiteSpace(types)
                ? null
                : (IReadOnlyCollection<string>)types.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var statusFilter = string.IsNullOrWhiteSpace(statuses)
                ? null
                : (IReadOnlyCollection<string>)statuses.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var response = await dispatcher.Dispatch(
                new GetEnrichedTimelineQuery(
                    familyId, typeFilter, memberFilter, from, to, statusFilter,
                    _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (FamilyException ex)
        {
            return MapFamilyException(ex);
        }
    }

    /// <summary>Returns a weekly coordination grid of events, tasks, and routines for all family members.</summary>
    [HttpGet("{familyId:guid}/weekly-grid")]
    [ProducesResponseType(typeof(WeeklyGridResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWeeklyGrid(
        Guid familyId,
        [FromQuery] DateTime? weekStart,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        // ASP.NET Core model binding can produce DateTimeKind.Unspecified for date-only strings.
        // Treat the incoming value as a UTC date — only the calendar date part is used.
        var start = DateTime.SpecifyKind(
            (weekStart ?? DateTime.UtcNow).Date,
            DateTimeKind.Utc);
        try
        {
            var response = await dispatcher.Dispatch(
                new GetWeeklyGridQuery(familyId, start, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (FamilyException ex)
        {
            return MapFamilyException(ex);
        }
    }

    private IActionResult MapFamilyException(FamilyException ex) => ex.Code switch
    {
        FamilyErrorCode.FamilyNotFound =>
            NotFound(new { error = ex.Message }),
        FamilyErrorCode.AccessDenied =>
            StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message }),
        FamilyErrorCode.MemberAlreadyExists =>
            Conflict(new { error = ex.Message }),
        FamilyErrorCode.FamilyAlreadyExists =>
            Conflict(new { error = ex.Message }),
        FamilyErrorCode.InvalidInput =>
            BadRequest(new { error = ex.Message }),
        _ =>
            StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message }),
    };
}
