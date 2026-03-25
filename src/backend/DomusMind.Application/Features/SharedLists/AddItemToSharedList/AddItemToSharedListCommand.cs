using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.SharedLists;

namespace DomusMind.Application.Features.SharedLists.AddItemToSharedList;

public sealed record AddItemToSharedListCommand(
    Guid SharedListId,
    string Name,
    string? Quantity,
    string? Note,
    Guid? AddedByMemberId,
    Guid RequestedByUserId) : ICommand<AddItemToSharedListResponse>;
