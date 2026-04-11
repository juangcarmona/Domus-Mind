using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Lists;

namespace DomusMind.Application.Features.Lists.ClearItemTemporal;

public sealed record ClearItemTemporalCommand(
    Guid ListId,
    Guid ItemId,
    Guid RequestedByUserId) : ICommand<ClearItemTemporalResponse>;
