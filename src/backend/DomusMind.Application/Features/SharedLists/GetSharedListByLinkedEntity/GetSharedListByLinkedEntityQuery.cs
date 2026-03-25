using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.SharedLists;

namespace DomusMind.Application.Features.SharedLists.GetSharedListByLinkedEntity;

public sealed record GetSharedListByLinkedEntityQuery(
    string EntityType,
    Guid EntityId,
    Guid RequestedByUserId) : IQuery<GetSharedListByLinkedEntityResponse>;
