using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Family.UpdateMember;

public sealed class UpdateMemberCommandHandler : ICommandHandler<UpdateMemberCommand, UpdateMemberResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public UpdateMemberCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<UpdateMemberResponse> Handle(UpdateMemberCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new FamilyException(FamilyErrorCode.InvalidInput, "Member name is required.");

        if (string.IsNullOrWhiteSpace(command.Role))
            throw new FamilyException(FamilyErrorCode.InvalidInput, "Member role is required.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);

        if (!canAccess)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Access to this family is denied.");

        var family = await _dbContext.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleOrDefaultAsync(f => f.Id == FamilyId.From(command.FamilyId), cancellationToken);

        if (family is null)
            throw new FamilyException(FamilyErrorCode.FamilyNotFound, "Family was not found.");

        // Ensure the requesting user is a manager
        var requestingMember = family.Members.FirstOrDefault(m => m.AuthUserId == command.RequestedByUserId);
        if (requestingMember is null || !requestingMember.IsManager)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Only household managers can edit member details.");

        var name = MemberName.Create(command.Name);
        var role = MemberRole.Create(command.Role);
        var memberId = MemberId.From(command.MemberId);
        var now = DateTime.UtcNow;

        if (command.IsManager && role.Value != "Adult")
            throw new FamilyException(FamilyErrorCode.InvalidInput, "Manager role can only be assigned to adult members.");

        FamilyMember updatedMember;
        try
        {
            updatedMember = family.UpdateMember(memberId, name, role, command.IsManager, command.BirthDate, now);
        }
        catch (InvalidOperationException ex)
        {
            throw new FamilyException(FamilyErrorCode.MemberNotFound, ex.Message);
        }

        await _eventLogWriter.WriteAsync(family.DomainEvents, cancellationToken);
        family.ClearDomainEvents();

        return new UpdateMemberResponse(
            updatedMember.Id.Value,
            family.Id.Value,
            updatedMember.Name.Value,
            updatedMember.Role.Value,
            updatedMember.IsManager,
            updatedMember.BirthDate,
            updatedMember.JoinedAtUtc);
    }
}
