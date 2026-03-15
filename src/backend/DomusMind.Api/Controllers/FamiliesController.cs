using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Family;
using DomusMind.Application.Features.Family.AddMember;
using DomusMind.Application.Features.Family.CreateFamily;
using DomusMind.Application.Features.Family.GetFamily;
using DomusMind.Application.Features.Family.GetFamilyMembers;
using DomusMind.Application.Features.Family.GetHouseholdTimeline;
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
                new CreateFamilyCommand(request.Name, _currentUser.UserId!.Value),
                cancellationToken);

            return CreatedAtAction(nameof(GetFamily), new { familyId = response.FamilyId }, response);
        }
        catch (FamilyException ex) when (ex.Code == FamilyErrorCode.InvalidInput)
        {
            return BadRequest(new { error = ex.Message });
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

    private IActionResult MapFamilyException(FamilyException ex) => ex.Code switch
    {
        FamilyErrorCode.FamilyNotFound =>
            NotFound(new { error = ex.Message }),
        FamilyErrorCode.AccessDenied =>
            StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message }),
        FamilyErrorCode.MemberAlreadyExists =>
            Conflict(new { error = ex.Message }),
        FamilyErrorCode.InvalidInput =>
            BadRequest(new { error = ex.Message }),
        _ =>
            StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message }),
    };
}
