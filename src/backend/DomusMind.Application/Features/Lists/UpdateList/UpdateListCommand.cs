using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Lists;

namespace DomusMind.Application.Features.Lists.UpdateList;

public sealed record UpdateListCommand(
    Guid ListId,
    string? Name,
    Guid? AreaId,
    bool ClearArea,
    Guid? LinkedPlanId,
    bool ClearLinkedPlan,
    string? Kind,
    Guid RequestedByUserId) : ICommand<UpdateListResponse>;
