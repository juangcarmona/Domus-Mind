using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Lists;

namespace DomusMind.Application.Features.Lists.AddItemToList;

public sealed record AddItemToListCommand(
    Guid ListId,
    string Name,
    string? Quantity,
    string? Note,
    Guid? AddedByMemberId,
    Guid RequestedByUserId) : ICommand<AddItemToListResponse>;
