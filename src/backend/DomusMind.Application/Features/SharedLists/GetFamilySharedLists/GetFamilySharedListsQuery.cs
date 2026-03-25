using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.SharedLists;

namespace DomusMind.Application.Features.SharedLists.GetFamilySharedLists;

public sealed record GetFamilySharedListsQuery(
    Guid FamilyId,
    Guid RequestedByUserId) : IQuery<GetFamilySharedListsResponse>;
