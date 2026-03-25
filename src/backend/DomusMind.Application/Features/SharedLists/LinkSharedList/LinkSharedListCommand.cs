using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.SharedLists;

namespace DomusMind.Application.Features.SharedLists.LinkSharedList;

public sealed record LinkSharedListCommand(
    Guid SharedListId,
    string LinkedEntityType,
    Guid LinkedEntityId,
    Guid RequestedByUserId) : ICommand<LinkSharedListResponse>;
