using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Lists;

namespace DomusMind.Application.Features.Lists.CreateList;

public sealed record CreateListCommand(
    Guid FamilyId,
    string Name,
    string? Kind,
    Guid? AreaId,
    Guid? LinkedEntityId,
    Guid RequestedByUserId) : ICommand<CreateListResponse>;
