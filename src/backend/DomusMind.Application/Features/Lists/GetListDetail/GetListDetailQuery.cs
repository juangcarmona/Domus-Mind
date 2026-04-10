using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Lists;

namespace DomusMind.Application.Features.Lists.GetListDetail;

public sealed record GetListDetailQuery(
    Guid ListId,
    Guid RequestedByUserId) : IQuery<GetListDetailResponse>;
