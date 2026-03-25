using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.SharedLists;

namespace DomusMind.Application.Features.SharedLists.ToggleSharedListItem;

public sealed record ToggleSharedListItemCommand(
    Guid SharedListId,
    Guid ItemId,
    Guid? UpdatedByMemberId,
    Guid RequestedByUserId) : ICommand<ToggleSharedListItemResponse>;
