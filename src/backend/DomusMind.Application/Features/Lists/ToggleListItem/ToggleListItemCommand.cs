using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Lists;

namespace DomusMind.Application.Features.Lists.ToggleListItem;

public sealed record ToggleListItemCommand(
    Guid ListId,
    Guid ItemId,
    Guid RequestedByUserId) : ICommand<ToggleListItemResponse>;
