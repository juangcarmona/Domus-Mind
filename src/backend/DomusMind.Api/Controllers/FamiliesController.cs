using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.AddMember;
using DomusMind.Application.Features.Family.CompleteOnboarding;
using DomusMind.Application.Features.Family.CreateFamily;
using DomusMind.Application.Features.Family.DisableMemberAccess;
using DomusMind.Application.Features.Family.EnableMemberAccess;
using DomusMind.Application.Features.Family.UpdateFamilySettings;
using DomusMind.Application.Features.Family.GetEnrichedTimeline;
using DomusMind.Application.Features.Family.GetFamily;
using DomusMind.Application.Features.Family.GetFamilyMembers;
using DomusMind.Application.Features.Family.GetHouseholdTimeline;
using DomusMind.Application.Features.Family.GetMemberActivity;
using DomusMind.Application.Features.Family.GetMemberDetails;
using DomusMind.Application.Features.Family.GetMyFamily;
using DomusMind.Application.Features.Family.GetWeeklyGrid;
using DomusMind.Application.Features.Family.InviteMember;
using DomusMind.Application.Features.Family.LinkMemberAccount;
using DomusMind.Application.Features.Family.ProvisionMemberAccess;
using DomusMind.Application.Features.Family.RegenerateTemporaryPassword;
using DomusMind.Application.Features.Family.UpdateMember;
using DomusMind.Application.Features.Family.UpdateMemberProfile;
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
                new AddMemberCommand(familyId, request.Name, request.Role, request.BirthDate, request.IsManager, _currentUser.UserId!.Value),
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
    [ProducesResponseType(typeof(IReadOnlyCollection<MemberDirectoryItemResponse>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// [DEPRECATED] Returns a basic chronological timeline of events, tasks, and routines for the given family.
    /// Use GET /api/families/{familyId}/timeline/enriched instead, which supports filtering, status, and cross-context enrichment.
    /// This endpoint will be removed in V1.1.
    /// </summary>
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

    /// <summary>Returns the details of a single family member.</summary>
    [HttpGet("{familyId:guid}/members/{memberId:guid}")]
    [ProducesResponseType(typeof(MemberDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMemberDetails(
        Guid familyId,
        Guid memberId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetMemberDetailsQuery(familyId, memberId, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (FamilyException ex)
        {
            return MapFamilyException(ex);
        }
    }

    /// <summary>
    /// Re-enables a disabled member's login access. Manager only.
    /// </summary>
    [HttpPost("{familyId:guid}/members/{memberId:guid}/enable-access")]
    [ProducesResponseType(typeof(EnableMemberAccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableMemberAccess(
        Guid familyId,
        Guid memberId,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new EnableMemberAccessCommand(familyId, memberId, _currentUser.UserId!.Value),
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
        [FromQuery] string? from,
        [FromQuery] string? to,
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

            DateOnly? fromDate = from is not null ? DateOnly.ParseExact(from, "yyyy-MM-dd") : null;
            DateOnly? toDate = to is not null ? DateOnly.ParseExact(to, "yyyy-MM-dd") : null;

            var response = await dispatcher.Dispatch(
                new GetEnrichedTimelineQuery(
                    familyId, typeFilter, memberFilter, fromDate, toDate, statusFilter,
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
        [FromQuery] string? weekStart,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var start = weekStart is not null
            ? DateOnly.ParseExact(weekStart, "yyyy-MM-dd")
            : DateOnly.FromDateTime(DateTime.UtcNow);
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

    /// <summary>Invites a new family member by generating temporary login credentials. Manager only.</summary>
    [HttpPost("{familyId:guid}/members/invite")]
    [ProducesResponseType(typeof(InviteMemberResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InviteMember(
        Guid familyId,
        [FromBody] InviteMemberRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new InviteMemberCommand(
                    familyId,
                    request.Name,
                    request.Role,
                    request.BirthDate,
                    request.IsManager,
                    request.Username,
                    request.TemporaryPassword,
                    _currentUser.UserId!.Value),
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

    /// <summary>Links an existing family member to a new login account. Manager only.</summary>
    [HttpPost("{familyId:guid}/members/{memberId:guid}/link-account")]
    [ProducesResponseType(typeof(LinkMemberAccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkMemberAccount(
        Guid familyId,
        Guid memberId,
        [FromBody] LinkMemberAccountRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new LinkMemberAccountCommand(
                    familyId,
                    memberId,
                    request.Username,
                    request.TemporaryPassword,
                    _currentUser.UserId!.Value),
                cancellationToken);

            return Created(
                $"/api/families/{familyId}/members/{memberId}",
                response);
        }
        catch (FamilyException ex)
        {
            return MapFamilyException(ex);
        }
    }

    /// <summary>Updates the details of a family member. Manager only.</summary>
    [HttpPut("{familyId:guid}/members/{memberId:guid}")]
    [ProducesResponseType(typeof(UpdateMemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMember(
        Guid familyId,
        Guid memberId,
        [FromBody] UpdateMemberRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new UpdateMemberCommand(
                    familyId,
                    memberId,
                    request.Name,
                    request.Role,
                    request.BirthDate,
                    request.IsManager,
                    _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (FamilyException ex)
        {
            return MapFamilyException(ex);
        }
    }

    /// <summary>
    /// Updates the lightweight profile fields for a member: preferred name, contact phone/email, household note.
    /// The member themselves or a household manager may call this endpoint.
    /// </summary>
    [HttpPatch("{familyId:guid}/members/{memberId:guid}/profile")]
    [ProducesResponseType(typeof(UpdateMemberProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMemberProfile(
        Guid familyId,
        Guid memberId,
        [FromBody] UpdateMemberProfileRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new UpdateMemberProfileCommand(
                    familyId,
                    memberId,
                    _currentUser.UserId!.Value,
                    request.PreferredName,
                    request.PrimaryPhone,
                    request.PrimaryEmail,
                    request.HouseholdNote),
                cancellationToken);

            return Ok(response);
        }
        catch (FamilyException ex)
        {
            return MapFamilyException(ex);
        }
    }

    /// <summary>
    /// Provisions a login account for an existing family member. Manager only.
    /// The generated temporary password is returned once and cannot be retrieved again.
    /// </summary>
    [HttpPost("{familyId:guid}/members/{memberId:guid}/provision-access")]
    [ProducesResponseType(typeof(ProvisionMemberAccessResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProvisionMemberAccess(
        Guid familyId,
        Guid memberId,
        [FromBody] ProvisionMemberAccessRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new ProvisionMemberAccessCommand(
                    familyId,
                    memberId,
                    request.Email,
                    request.DisplayName,
                    _currentUser.UserId!.Value),
                cancellationToken);

            return Created(
                $"/api/families/{familyId}/members/{memberId}",
                response);
        }
        catch (FamilyException ex)
        {
            return MapFamilyException(ex);
        }
    }

    /// <summary>
    /// Generates a new temporary password for a member's existing account. Manager only.
    /// All existing sessions for that user are revoked. The new password is returned once.
    /// </summary>
    [HttpPost("{familyId:guid}/members/{memberId:guid}/regenerate-password")]
    [ProducesResponseType(typeof(RegenerateTemporaryPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegeneratePassword(
        Guid familyId,
        Guid memberId,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new RegenerateTemporaryPasswordCommand(
                    familyId,
                    memberId,
                    _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (FamilyException ex)
        {
            return MapFamilyException(ex);
        }
    }

    /// <summary>
    /// Disables a member's login access. Manager only.
    /// All active sessions for that user are revoked immediately.
    /// </summary>
    [HttpPost("{familyId:guid}/members/{memberId:guid}/disable-access")]
    [ProducesResponseType(typeof(DisableMemberAccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableMemberAccess(
        Guid familyId,
        Guid memberId,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new DisableMemberAccessCommand(
                    familyId,
                    memberId,
                    _currentUser.UserId!.Value),
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
        FamilyErrorCode.MemberNotFound =>
            NotFound(new { error = ex.Message }),
        FamilyErrorCode.FamilyAlreadyExists =>
            Conflict(new { error = ex.Message }),
        FamilyErrorCode.InvalidInput =>
            BadRequest(new { error = ex.Message }),
        _ =>
            StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message }),
    };
}
