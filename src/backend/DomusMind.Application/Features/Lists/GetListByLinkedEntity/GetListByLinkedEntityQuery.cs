using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Lists;

namespace DomusMind.Application.Features.Lists.GetListByLinkedEntity;

public sealed record GetListByLinkedEntityQuery(
    string EntityType,
    Guid EntityId,
    Guid RequestedByUserId) : IQuery<GetListByLinkedEntityResponse>;
