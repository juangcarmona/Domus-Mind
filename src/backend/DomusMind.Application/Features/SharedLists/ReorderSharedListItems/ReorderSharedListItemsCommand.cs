using DomusMind.Application.Abstractions.Messaging;

namespace DomusMind.Application.Features.SharedLists.ReorderSharedListItems;

public sealed record ReorderSharedListItemsCommand(
    Guid SharedListId,
    IReadOnlyList<Guid> ItemIds,
    Guid RequestedByUserId) : ICommand<bool>;
