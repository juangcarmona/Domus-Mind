using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Lists;

namespace DomusMind.Application.Features.Lists.UpdateListItem;

public sealed record UpdateListItemCommand(
    Guid ListId,
    Guid ItemId,
    string Name,
    string? Quantity,
    string? Note,
    Guid RequestedByUserId) : ICommand<UpdateListItemResponse>;
