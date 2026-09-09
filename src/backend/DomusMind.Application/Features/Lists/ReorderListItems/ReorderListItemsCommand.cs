using DomusMind.Application.Abstractions.Messaging;

namespace DomusMind.Application.Features.Lists.ReorderListItems;

public sealed record ReorderListItemsCommand(
    Guid ListId,
    IReadOnlyList<Guid> ItemIds,
    Guid RequestedByUserId) : ICommand<bool>;
