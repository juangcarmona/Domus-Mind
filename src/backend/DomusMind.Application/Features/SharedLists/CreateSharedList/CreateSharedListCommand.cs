using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.SharedLists;

namespace DomusMind.Application.Features.SharedLists.CreateSharedList;

public sealed record CreateSharedListCommand(
    Guid FamilyId,
    string Name,
    string Kind,
    Guid? AreaId,
    string? LinkedEntityType,
    Guid? LinkedEntityId,
    Guid RequestedByUserId) : ICommand<CreateSharedListResponse>;
