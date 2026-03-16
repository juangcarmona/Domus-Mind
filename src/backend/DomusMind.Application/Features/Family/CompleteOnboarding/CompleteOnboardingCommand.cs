using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Family;

namespace DomusMind.Application.Features.Family.CompleteOnboarding;

public sealed record AdditionalMemberInput(
    string Name,
    DateOnly? BirthDate,
    string? Type,
    bool Manager);

public sealed record CompleteOnboardingCommand(
    Guid FamilyId,
    Guid RequestedByUserId,
    string SelfName,
    DateOnly? SelfBirthDate,
    IReadOnlyCollection<AdditionalMemberInput> AdditionalMembers)
    : ICommand<CompleteOnboardingResponse>;
