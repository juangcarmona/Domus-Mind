using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Family.UpdateMemberProfile;

public sealed class UpdateMemberProfileCommandHandler : ICommandHandler<UpdateMemberProfileCommand, UpdateMemberProfileResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public UpdateMemberProfileCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<UpdateMemberProfileResponse> Handle(UpdateMemberProfileCommand command, CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);

        if (!canAccess)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Access to this family is denied.");

        var family = await _dbContext.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleOrDefaultAsync(f => f.Id == FamilyId.From(command.FamilyId), cancellationToken);

        if (family is null)
            throw new FamilyException(FamilyErrorCode.FamilyNotFound, "Family was not found.");

        // Only the member themselves or a manager may update profile fields
        var requestingMember = family.Members.FirstOrDefault(m => m.AuthUserId == command.RequestedByUserId);
        var isManager = requestingMember?.IsManager ?? false;
        var isSelf = requestingMember?.Id.Value == command.MemberId;

        if (!isManager && !isSelf)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Only household managers or the member themselves can update profile details.");

        var memberId = MemberId.From(command.MemberId);
        FamilyMember updatedMember;
        try
        {
            updatedMember = family.UpdateMemberProfile(
                memberId,
                command.PreferredName,
                command.PrimaryPhone,
                command.PrimaryEmail,
                command.HouseholdNote,
                DateTime.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new FamilyException(FamilyErrorCode.MemberNotFound, ex.Message);
        }

        await _eventLogWriter.WriteAsync(family.DomainEvents, cancellationToken);
        family.ClearDomainEvents();

        return new UpdateMemberProfileResponse(
            updatedMember.Id.Value,
            family.Id.Value,
            updatedMember.PreferredName,
            updatedMember.PrimaryPhone,
            updatedMember.PrimaryEmail,
            updatedMember.HouseholdNote);
    }
}
