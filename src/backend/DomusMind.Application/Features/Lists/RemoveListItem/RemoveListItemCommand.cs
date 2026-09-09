using DomusMind.Application.Abstractions.Messaging;

namespace DomusMind.Application.Features.Lists.RemoveListItem;

public sealed record RemoveListItemCommand(
    Guid ListId,
    Guid ItemId,
    Guid RequestedByUserId) : ICommand<bool>;
