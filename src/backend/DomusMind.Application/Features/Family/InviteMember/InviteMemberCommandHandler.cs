using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Auth;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Family.InviteMember;

public sealed class InviteMemberCommandHandler : ICommandHandler<InviteMemberCommand, InviteMemberResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;
    private readonly IAuthUserRepository _authUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IFamilyAccessGranter _familyAccessGranter;

    public InviteMemberCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService,
        IAuthUserRepository authUserRepository,
        IPasswordHasher passwordHasher,
        IFamilyAccessGranter familyAccessGranter)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
        _authUserRepository = authUserRepository;
        _passwordHasher = passwordHasher;
        _familyAccessGranter = familyAccessGranter;
    }

    public async Task<InviteMemberResponse> Handle(InviteMemberCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new FamilyException(FamilyErrorCode.InvalidInput, "Member name is required.");

        if (string.IsNullOrWhiteSpace(command.Role))
            throw new FamilyException(FamilyErrorCode.InvalidInput, "Member role is required.");

        if (string.IsNullOrWhiteSpace(command.Username))
            throw new FamilyException(FamilyErrorCode.InvalidInput, "Username is required.");

        if (string.IsNullOrWhiteSpace(command.TemporaryPassword) || command.TemporaryPassword.Length < 8)
            throw new FamilyException(FamilyErrorCode.InvalidInput, "Temporary password must be at least 8 characters.");

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
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Only household managers can invite new members.");

        var username = command.Username.Trim().ToLowerInvariant();

        var existingAuthUser = await _authUserRepository.FindByEmailAsync(username, cancellationToken);
        if (existingAuthUser is not null)
            throw new FamilyException(FamilyErrorCode.InvalidInput, "The username is already taken.");

        var authUserId = Guid.NewGuid();
        var passwordHash = _passwordHasher.Hash(command.TemporaryPassword);
        var authUserRecord = new AuthUserRecord(authUserId, username, passwordHash, MustChangePassword: true);
        await _authUserRepository.AddAsync(authUserRecord, cancellationToken);

        var memberId = MemberId.New();
        var name = MemberName.Create(command.Name);
        var role = MemberRole.Create(command.Role);
        var now = DateTime.UtcNow;

        if (command.IsManager && role.Value != "Adult")
            throw new FamilyException(FamilyErrorCode.InvalidInput, "Manager role can only be assigned to adult members.");

        try
        {
            family.AddMember(memberId, name, role, command.IsManager, command.BirthDate, now, authUserId);
        }
        catch (InvalidOperationException ex)
        {
            throw new FamilyException(FamilyErrorCode.MemberAlreadyExists, ex.Message);
        }

        await _familyAccessGranter.GrantAccessAsync(authUserId, command.FamilyId, cancellationToken);

        await _eventLogWriter.WriteAsync(family.DomainEvents, cancellationToken);
        family.ClearDomainEvents();

        await _authUserRepository.SaveChangesAsync(cancellationToken);

        return new InviteMemberResponse(
            memberId.Value,
            family.Id.Value,
            name.Value,
            role.Value,
            command.IsManager,
            command.BirthDate,
            username,
            now);
    }
}
