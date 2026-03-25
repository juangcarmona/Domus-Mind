using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.SharedLists;

namespace DomusMind.Application.Features.SharedLists.GetSharedListDetail;

public sealed record GetSharedListDetailQuery(
    Guid SharedListId,
    Guid RequestedByUserId) : IQuery<GetSharedListDetailResponse>;
