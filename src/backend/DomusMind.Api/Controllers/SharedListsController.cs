using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.SharedLists;
using DomusMind.Application.Features.SharedLists.AddItemToSharedList;
using DomusMind.Application.Features.SharedLists.CreateSharedList;
using DomusMind.Application.Features.SharedLists.GetFamilySharedLists;
using DomusMind.Application.Features.SharedLists.GetSharedListDetail;
using DomusMind.Application.Features.SharedLists.ToggleSharedListItem;
using DomusMind.Contracts.SharedLists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Controllers;

[ApiController]
[Route("api/shared-lists")]
[Authorize]
public sealed class SharedListsController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public SharedListsController(ICurrentUser currentUser) => _currentUser = currentUser;

    /// <summary>Creates a new shared list for a family.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateSharedListResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateSharedList(
        [FromBody] CreateSharedListRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new CreateSharedListCommand(
                    request.FamilyId, request.Name, request.Kind,
                    request.AreaId, request.LinkedEntityType, request.LinkedEntityId,
                    _currentUser.UserId!.Value),
                cancellationToken);
            return Created($"/api/shared-lists/{response.ListId}", response);
        }
        catch (SharedListException ex) { return MapException(ex); }
    }

    /// <summary>Returns all shared lists for a family.</summary>
    [HttpGet("family/{familyId:guid}")]
    [ProducesResponseType(typeof(GetFamilySharedListsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFamilySharedLists(
        Guid familyId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetFamilySharedListsQuery(familyId, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (SharedListException ex) { return MapException(ex); }
    }

    /// <summary>Returns the full detail of a shared list including items.</summary>
    [HttpGet("{listId:guid}")]
    [ProducesResponseType(typeof(GetSharedListDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSharedListDetail(
        Guid listId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetSharedListDetailQuery(listId, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (SharedListException ex) { return MapException(ex); }
    }

    /// <summary>Adds a new item to a shared list.</summary>
    [HttpPost("{listId:guid}/items")]
    [ProducesResponseType(typeof(AddItemToSharedListResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(
        Guid listId,
        [FromBody] AddItemToSharedListRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new AddItemToSharedListCommand(
                    listId, request.Name, request.Quantity, request.Note,
                    null, _currentUser.UserId!.Value),
                cancellationToken);
            return Created($"/api/shared-lists/{listId}/items/{response.ItemId}", response);
        }
        catch (SharedListException ex) { return MapException(ex); }
    }

    /// <summary>Toggles the checked state of an item in a shared list.</summary>
    [HttpPost("{listId:guid}/items/{itemId:guid}/toggle")]
    [ProducesResponseType(typeof(ToggleSharedListItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleItem(
        Guid listId,
        Guid itemId,
        [FromBody] ToggleSharedListItemRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new ToggleSharedListItemCommand(
                    listId, itemId, request.UpdatedByMemberId, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (SharedListException ex) { return MapException(ex); }
    }

    private IActionResult MapException(SharedListException ex) => ex.Code switch
    {
        SharedListErrorCode.ListNotFound => NotFound(new { error = ex.Message }),
        SharedListErrorCode.ItemNotFound => NotFound(new { error = ex.Message }),
        SharedListErrorCode.AccessDenied => StatusCode(403, new { error = ex.Message }),
        SharedListErrorCode.InvalidInput => BadRequest(new { error = ex.Message }),
        _ => StatusCode(500, new { error = ex.Message }),
    };
}
