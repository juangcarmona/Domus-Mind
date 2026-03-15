using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Family;

namespace DomusMind.Application.Features.Family.CreateFamily;

public sealed record CreateFamilyCommand(string Name, Guid RequestedByUserId)
    : ICommand<CreateFamilyResponse>;
