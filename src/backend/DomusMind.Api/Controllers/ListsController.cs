using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Lists;
using DomusMind.Application.Features.Lists.AddItemToList;
using DomusMind.Application.Features.Lists.ArchiveList;
using DomusMind.Application.Features.Lists.CreateList;
using DomusMind.Application.Features.Lists.GetFamilyLists;
using DomusMind.Application.Features.Lists.GetListDetail;
using DomusMind.Application.Features.Lists.ToggleListItem;
using DomusMind.Application.Features.Lists.UpdateListItem;
using DomusMind.Application.Features.Lists.RemoveListItem;
using DomusMind.Application.Features.Lists.LinkList;
using DomusMind.Application.Features.Lists.UnlinkList;
using DomusMind.Application.Features.Lists.CreateLinkedListForEvent;
using DomusMind.Application.Features.Lists.GetListByLinkedEntity;
using DomusMind.Application.Features.Lists.RenameList;
using DomusMind.Application.Features.Lists.DeleteList;
using DomusMind.Application.Features.Lists.ReorderListItems;
using DomusMind.Application.Features.Lists.SetItemImportance;
using DomusMind.Application.Features.Lists.SetItemTemporal;
using DomusMind.Application.Features.Lists.ClearItemTemporal;
using DomusMind.Application.Features.Lists.SetItemContext;
using DomusMind.Application.Features.Lists.RestoreList;
using DomusMind.Application.Features.Lists.UpdateList;
using DomusMind.Contracts.Lists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Controllers;

[ApiController]
[Route("api/shared-lists")]
[Authorize]
public sealed class ListsController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public ListsController(ICurrentUser currentUser) => _currentUser = currentUser;

    /// <summary>Creates a new shared list for a family.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateListResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateList(
        [FromBody] CreateListRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new CreateListCommand(
                    request.FamilyId, request.Name, request.Kind,
                    request.AreaId, request.LinkedPlanId,
                    _currentUser.UserId!.Value),
                cancellationToken);
            return Created($"/api/shared-lists/{response.ListId}", response);
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Returns all shared lists for a family.</summary>
    [HttpGet("family/{familyId:guid}")]
    [ProducesResponseType(typeof(GetFamilyListsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFamilyLists(
        Guid familyId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetFamilyListsQuery(familyId, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Returns the full detail of a shared list including items.</summary>
    [HttpGet("{listId:guid}")]
    [ProducesResponseType(typeof(GetListDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetListDetail(
        Guid listId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetListDetailQuery(listId, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Adds a new item to a shared list.</summary>
    [HttpPost("{listId:guid}/items")]
    [ProducesResponseType(typeof(AddItemToListResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(
        Guid listId,
        [FromBody] AddItemToListRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new AddItemToListCommand(
                    listId, request.Name, request.Quantity, request.Note,
                    null, _currentUser.UserId!.Value),
                cancellationToken);
            return Created($"/api/shared-lists/{listId}/items/{response.ItemId}", response);
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Toggles the checked state of an item in a shared list.</summary>
    [HttpPost("{listId:guid}/items/{itemId:guid}/toggle")]
    [ProducesResponseType(typeof(ToggleListItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleItem(
        Guid listId,
        Guid itemId,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new ToggleListItemCommand(
                    listId, itemId, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Updates fields of an item in a shared list.</summary>
    [HttpPatch("{listId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(UpdateListItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(
        Guid listId,
        Guid itemId,
        [FromBody] UpdateListItemRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new UpdateListItemCommand(
                    listId, itemId, request.Name, request.Quantity, request.Note, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Removes an item from a shared list.</summary>
    [HttpDelete("{listId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(
        Guid listId,
        Guid itemId,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            await dispatcher.Dispatch(
                new RemoveListItemCommand(listId, itemId, _currentUser.UserId!.Value),
                cancellationToken);
            return NoContent();
        }
        catch (ListException ex) { return MapException(ex); }
    }

    private IActionResult MapException(ListException ex) => ex.Code switch
    {
        ListErrorCode.ListNotFound => NotFound(new { error = ex.Message }),
        ListErrorCode.ItemNotFound => NotFound(new { error = ex.Message }),
        ListErrorCode.AccessDenied => StatusCode(403, new { error = ex.Message }),
        ListErrorCode.InvalidInput => BadRequest(new { error = ex.Message }),
        _ => StatusCode(500, new { error = ex.Message }),
    };

    /// <summary>Links an existing shared list to a calendar event.</summary>
    [HttpPatch("{listId:guid}/link")]
    [ProducesResponseType(typeof(LinkListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> LinkList(
        Guid listId,
        [FromBody] LinkListRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new LinkListCommand(listId, request.LinkedEntityType, request.LinkedEntityId, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Removes the entity linkage from a shared list.</summary>
    [HttpDelete("{listId:guid}/link")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UnlinkList(
        Guid listId,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            await dispatcher.Dispatch(
                new UnlinkListCommand(listId, _currentUser.UserId!.Value),
                cancellationToken);
            return NoContent();
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Creates a new shared list linked to a calendar event.</summary>
    [HttpPost("linked/calendar-event/{eventId:guid}")]
    [ProducesResponseType(typeof(CreateListResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateLinkedListForEvent(
        Guid eventId,
        [FromBody] CreateLinkedListForEventRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new CreateLinkedListForEventCommand(eventId, request.FamilyId, request.Name, _currentUser.UserId!.Value),
                cancellationToken);
            return Created($"/api/shared-lists/{response.ListId}", response);
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Returns the shared list linked to a specific entity, if one exists.</summary>
    [HttpGet("by-entity/{entityType}/{entityId:guid}")]
    [ProducesResponseType(typeof(GetListByLinkedEntityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetListByLinkedEntity(
        string entityType,
        Guid entityId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetListByLinkedEntityQuery(entityType, entityId, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Renames a shared list.</summary>
    [HttpPatch("{listId:guid}/name")]
    [ProducesResponseType(typeof(RenameListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RenameList(
        Guid listId,
        [FromBody] RenameListRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new RenameListCommand(listId, request.Name, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Deletes a shared list and all its items.</summary>
    [HttpDelete("{listId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteList(
        Guid listId,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            await dispatcher.Dispatch(
                new DeleteListCommand(listId, _currentUser.UserId!.Value),
                cancellationToken);
            return NoContent();
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Reorders the items in a list.</summary>
    [HttpPatch("{listId:guid}/items/order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReorderItems(
        Guid listId,
        [FromBody] ReorderListItemsRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            await dispatcher.Dispatch(
                new ReorderListItemsCommand(listId, request.ItemIds, _currentUser.UserId!.Value),
                cancellationToken);
            return NoContent();
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Updates metadata of a list (name, area, linked plan, kind).</summary>
    [HttpPatch("{listId:guid}")]
    [ProducesResponseType(typeof(UpdateListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateList(
        Guid listId,
        [FromBody] UpdateListRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new UpdateListCommand(
                    listId,
                    request.Name,
                    request.AreaId,
                    request.ClearArea,
                    request.LinkedPlanId,
                    request.ClearLinkedPlan,
                    request.Kind,
                    request.Color,
                    request.ClearColor,
                    _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Archives a list, removing it from the active index.</summary>
    [HttpPost("{listId:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveList(
        Guid listId,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            await dispatcher.Dispatch(
                new ArchiveListCommand(listId, _currentUser.UserId!.Value),
                cancellationToken);
            return NoContent();
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Restores an archived list to active state.</summary>
    [HttpPost("{listId:guid}/restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RestoreList(
        Guid listId,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            await dispatcher.Dispatch(
                new RestoreListCommand(listId, _currentUser.UserId!.Value),
                cancellationToken);
            return NoContent();
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Sets or clears the importance flag on a list item.</summary>
    [HttpPatch("{listId:guid}/items/{itemId:guid}/importance")]
    [ProducesResponseType(typeof(SetItemImportanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetItemImportance(
        Guid listId,
        Guid itemId,
        [FromBody] SetItemImportanceRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new SetItemImportanceCommand(listId, itemId, request.Importance, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Sets temporal fields (due date, reminder, repeat) on a list item.</summary>
    [HttpPatch("{listId:guid}/items/{itemId:guid}/temporal")]
    [ProducesResponseType(typeof(SetItemTemporalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetItemTemporal(
        Guid listId,
        Guid itemId,
        [FromBody] SetItemTemporalRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new SetItemTemporalCommand(listId, itemId, request.DueDate, request.Reminder, request.Repeat, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Clears all temporal fields from a list item.</summary>
    [HttpDelete("{listId:guid}/items/{itemId:guid}/temporal")]
    [ProducesResponseType(typeof(ClearItemTemporalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ClearItemTemporal(
        Guid listId,
        Guid itemId,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new ClearItemTemporalCommand(listId, itemId, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (ListException ex) { return MapException(ex); }
    }

    /// <summary>Sets item-level context: area and target member.</summary>
    [HttpPatch("{listId:guid}/items/{itemId:guid}/context")]
    [ProducesResponseType(typeof(SetItemContextResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetItemContext(
        Guid listId,
        Guid itemId,
        [FromBody] SetItemContextRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new SetItemContextCommand(listId, itemId, request.ItemAreaId, request.TargetMemberId, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (ListException ex) { return MapException(ex); }
    }
}