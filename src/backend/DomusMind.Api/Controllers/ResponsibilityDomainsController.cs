using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Responsibilities;
using DomusMind.Application.Features.Responsibilities.AssignPrimaryOwner;
using DomusMind.Application.Features.Responsibilities.AssignSecondaryOwner;
using DomusMind.Application.Features.Responsibilities.CreateResponsibilityDomain;
using DomusMind.Application.Features.Responsibilities.DetectResponsibilityOverload;
using DomusMind.Application.Features.Responsibilities.GetHouseholdAreas;
using DomusMind.Application.Features.Responsibilities.GetResponsibilityBalance;
using DomusMind.Application.Features.Responsibilities.GetResponsibilityVisibility;
using DomusMind.Application.Features.Responsibilities.SuggestResponsibilityOwner;
using DomusMind.Application.Features.Responsibilities.TransferResponsibility;
using DomusMind.Contracts.Responsibilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Controllers;

[ApiController]
[Route("api/responsibility-domains")]
[Authorize]
public sealed class ResponsibilityDomainsController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public ResponsibilityDomainsController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>Creates a new responsibility domain for a family.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateResponsibilityDomainResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateResponsibilityDomain(
        [FromBody] CreateResponsibilityDomainRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new CreateResponsibilityDomainCommand(request.Name, request.FamilyId, _currentUser.UserId!.Value),
                cancellationToken);

            return Created($"/api/responsibility-domains/{response.ResponsibilityDomainId}", response);
        }
        catch (ResponsibilitiesException ex)
        {
            return MapResponsibilitiesException(ex);
        }
    }

    /// <summary>Assigns a primary owner to a responsibility domain.</summary>
    [HttpPost("{id:guid}/primary-owner")]
    [ProducesResponseType(typeof(AssignPrimaryOwnerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignPrimaryOwner(
        Guid id,
        [FromBody] AssignPrimaryOwnerRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new AssignPrimaryOwnerCommand(id, request.MemberId, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (ResponsibilitiesException ex)
        {
            return MapResponsibilitiesException(ex);
        }
    }

    /// <summary>Assigns a secondary owner to a responsibility domain.</summary>
    [HttpPost("{id:guid}/secondary-owners")]
    [ProducesResponseType(typeof(AssignSecondaryOwnerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignSecondaryOwner(
        Guid id,
        [FromBody] AssignSecondaryOwnerRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new AssignSecondaryOwnerCommand(id, request.MemberId, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (ResponsibilitiesException ex)
        {
            return MapResponsibilitiesException(ex);
        }
    }

    /// <summary>Transfers primary ownership of a responsibility domain to a new member.</summary>
    [HttpPost("{id:guid}/transfer")]
    [ProducesResponseType(typeof(TransferResponsibilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferResponsibility(
        Guid id,
        [FromBody] TransferResponsibilityRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new TransferResponsibilityCommand(id, request.NewPrimaryOwnerId, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (ResponsibilitiesException ex)
        {
            return MapResponsibilitiesException(ex);
        }
    }

    /// <summary>Returns all household areas (responsibility domains) with enriched owner context.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(HouseholdAreasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHouseholdAreas(
        [FromQuery] Guid familyId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetHouseholdAreasQuery(familyId, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (ResponsibilitiesException ex)
        {
            return MapResponsibilitiesException(ex);
        }
    }

    /// <summary>Returns the responsibility load balance per family member.</summary>
    [HttpGet("balance")]
    [ProducesResponseType(typeof(ResponsibilityBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetResponsibilityBalance(
        [FromQuery] Guid familyId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetResponsibilityBalanceQuery(familyId, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (ResponsibilitiesException ex)
        {
            return MapResponsibilitiesException(ex);
        }
    }

    /// <summary>Detects family members overloaded with responsibilities.</summary>
    [HttpGet("overload")]
    [ProducesResponseType(typeof(ResponsibilityOverloadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DetectOverload(
        [FromQuery] Guid familyId,
        [FromQuery] int threshold,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var effectiveThreshold = threshold > 0 ? threshold : 3;
            var response = await dispatcher.Dispatch(
                new DetectResponsibilityOverloadQuery(familyId, effectiveThreshold, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (ResponsibilitiesException ex)
        {
            return MapResponsibilitiesException(ex);
        }
    }

    /// <summary>Returns a per-member visibility map of responsibility domains.</summary>
    [HttpGet("visibility")]
    [ProducesResponseType(typeof(ResponsibilityVisibilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetResponsibilityVisibility(
        [FromQuery] Guid familyId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetResponsibilityVisibilityQuery(familyId, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (ResponsibilitiesException ex)
        {
            return MapResponsibilitiesException(ex);
        }
    }

    /// <summary>Suggests the best family member to primary-own a responsibility domain.</summary>
    [HttpGet("{id:guid}/suggest-owner")]
    [ProducesResponseType(typeof(SuggestResponsibilityOwnerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuggestOwner(
        Guid id,
        [FromQuery] Guid familyId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new SuggestResponsibilityOwnerQuery(familyId, id, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (ResponsibilitiesException ex)
        {
            return MapResponsibilitiesException(ex);
        }
    }

    private IActionResult MapResponsibilitiesException(ResponsibilitiesException ex) => ex.Code switch
    {
        ResponsibilitiesErrorCode.ResponsibilityDomainNotFound =>
            NotFound(new { error = ex.Message }),
        ResponsibilitiesErrorCode.AccessDenied =>
            StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message }),
        ResponsibilitiesErrorCode.DuplicateSecondaryOwner =>
            Conflict(new { error = ex.Message }),
        ResponsibilitiesErrorCode.MemberNotFound =>
            NotFound(new { error = ex.Message }),
        ResponsibilitiesErrorCode.InvalidInput =>
            BadRequest(new { error = ex.Message }),
        _ =>
            StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message }),
    };
}
