using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Family;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Family.CompleteOnboarding;

public sealed class CompleteOnboardingCommandHandler
    : ICommandHandler<CompleteOnboardingCommand, CompleteOnboardingResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public CompleteOnboardingCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<CompleteOnboardingResponse> Handle(
        CompleteOnboardingCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.SelfName))
            throw new FamilyException(FamilyErrorCode.InvalidInput, "Your name is required.");

        foreach (var additional in command.AdditionalMembers)
        {
            if (string.IsNullOrWhiteSpace(additional.Name))
                throw new FamilyException(FamilyErrorCode.InvalidInput, "Each member must have a name.");

            var mappedRole = MapType(additional.Type);
            if (additional.Manager && mappedRole != "Adult")
                throw new FamilyException(
                    FamilyErrorCode.InvalidInput,
                    $"Manager role can only be assigned to adult members, but '{additional.Name}' has type '{additional.Type}'.");
        }

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);

        if (!canAccess)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Access to this family is denied.");

        var family = await _dbContext.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleOrDefaultAsync(f => f.Id == FamilyId.From(command.FamilyId), cancellationToken);

        if (family is null)
            throw new FamilyException(FamilyErrorCode.FamilyNotFound, "Family was not found.");

        var now = DateTime.UtcNow;
        var addedMembers = new List<FamilyMember>();

        try
        {
            var creatorId = MemberId.New();
            var creator = family.AddMember(
                creatorId,
                MemberName.Create(command.SelfName),
                MemberRole.Adult,
                true,
                command.SelfBirthDate,
                now,
                command.RequestedByUserId);
            addedMembers.Add(creator);

            foreach (var additional in command.AdditionalMembers)
            {
                var memberId = MemberId.New();
                var role = MemberRole.Create(MapType(additional.Type));
                var member = family.AddMember(
                    memberId,
                    MemberName.Create(additional.Name),
                    role,
                    additional.Manager,
                    additional.BirthDate,
                    now);
                addedMembers.Add(member);
            }
        }
        catch (InvalidOperationException ex)
        {
            throw new FamilyException(FamilyErrorCode.InvalidInput, ex.Message);
        }

        await _eventLogWriter.WriteAsync(family.DomainEvents, cancellationToken);
        family.ClearDomainEvents();

        return new CompleteOnboardingResponse(
            family.Id.Value,
            family.Name.Value,
            addedMembers
                .Select(m => new OnboardingMemberItem(
                    m.Id.Value,
                    m.Name.Value,
                    m.Role.Value,
                    m.IsManager,
                    m.BirthDate,
                    m.JoinedAtUtc))
                .ToList()
                .AsReadOnly());
    }

    private static string MapType(string? type) => (type?.ToLowerInvariant()) switch
    {
        "child" => "Child",
        "pet" => "Pet",
        _ => "Adult",
    };
}
