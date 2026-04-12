using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Lists;

namespace DomusMind.Application.Features.Lists.SetItemContext;

public sealed record SetItemContextCommand(
    Guid ListId,
    Guid ItemId,
    Guid? ItemAreaId,
    Guid? TargetMemberId,
    Guid RequestedByUserId) : ICommand<SetItemContextResponse>;
