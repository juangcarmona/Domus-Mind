using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Lists;

namespace DomusMind.Application.Features.Lists.LinkList;

public sealed record LinkListCommand(
    Guid ListId,
    string LinkedEntityType,
    Guid LinkedEntityId,
    Guid RequestedByUserId) : ICommand<LinkListResponse>;
