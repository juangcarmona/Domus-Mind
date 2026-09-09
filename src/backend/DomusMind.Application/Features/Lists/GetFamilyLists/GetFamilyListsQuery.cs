using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Lists;

namespace DomusMind.Application.Features.Lists.GetFamilyLists;

public sealed record GetFamilyListsQuery(
    Guid FamilyId,
    Guid RequestedByUserId) : IQuery<GetFamilyListsResponse>;
