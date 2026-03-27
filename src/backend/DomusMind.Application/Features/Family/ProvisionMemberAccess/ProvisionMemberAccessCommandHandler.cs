using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Auth;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Family.ProvisionMemberAccess;

/// <summary>
/// Admin-only: creates a login account for an existing family member.
/// The system generates a temporary password that is returned once in the response.
/// The admin is responsible for sharing it out-of-band with the member.
/// </summary>
public sealed class ProvisionMemberAccessCommandHandler
    : ICommandHandler<ProvisionMemberAccessCommand, ProvisionMemberAccessResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;
    private readonly IAuthUserRepository _authUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITemporaryPasswordGenerator _passwordGenerator;
    private readonly IFamilyAccessGranter _familyAccessGranter;

    public ProvisionMemberAccessCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService,
        IAuthUserRepository authUserRepository,
        IPasswordHasher passwordHasher,
        ITemporaryPasswordGenerator passwordGenerator,
        IFamilyAccessGranter familyAccessGranter)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
        _authUserRepository = authUserRepository;
        _passwordHasher = passwordHasher;
        _passwordGenerator = passwordGenerator;
        _familyAccessGranter = familyAccessGranter;
    }

    public async Task<ProvisionMemberAccessResponse> Handle(
        ProvisionMemberAccessCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            throw new FamilyException(FamilyErrorCode.InvalidInput, "Email is required.");

        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);

        if (!canAccess)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Access to this family is denied.");

        var family = await _dbContext.Set<Domain.Family.Family>()
            .Include(f => f.Members)
            .SingleOrDefaultAsync(f => f.Id == FamilyId.From(command.FamilyId), cancellationToken);

        if (family is null)
            throw new FamilyException(FamilyErrorCode.FamilyNotFound, "Family was not found.");

        var requestingMember = family.Members.FirstOrDefault(m => m.AuthUserId == command.RequestedByUserId);
        if (requestingMember is null || !requestingMember.IsManager)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Only household managers can provision member access.");

        var memberId = MemberId.From(command.MemberId);
        var targetMember = family.Members.FirstOrDefault(m => m.Id == memberId);
        if (targetMember is null)
            throw new FamilyException(FamilyErrorCode.MemberNotFound, "Member was not found.");

        if (targetMember.AuthUserId.HasValue)
            throw new FamilyException(FamilyErrorCode.InvalidInput, "This member already has a linked account.");

        var email = command.Email.Trim().ToLowerInvariant();

        var existingAuthUser = await _authUserRepository.FindByEmailAsync(email, cancellationToken);
        if (existingAuthUser is not null)
            throw new FamilyException(FamilyErrorCode.InvalidInput, "An account with this email already exists.");

        // Generate temporary password server-side - never persisted in plain text
        var temporaryPassword = _passwordGenerator.Generate();
        var passwordHash = _passwordHasher.Hash(temporaryPassword);

        var authUserId = Guid.NewGuid();
        var authUserRecord = new AuthUserRecord(
            authUserId,
            email,
            passwordHash,
            MustChangePassword: true,
            DisplayName: command.DisplayName?.Trim(),
            IsDisabled: false,
            MemberId: command.MemberId);

        await _authUserRepository.AddAsync(authUserRecord, cancellationToken);

        var now = DateTime.UtcNow;

        try
        {
            family.LinkMemberAccount(memberId, authUserId, now);
        }
        catch (InvalidOperationException ex)
        {
            throw new FamilyException(FamilyErrorCode.InvalidInput, ex.Message);
        }

        await _familyAccessGranter.GrantAccessAsync(authUserId, command.FamilyId, cancellationToken);

        await _authUserRepository.SaveChangesAsync(cancellationToken);

        await _eventLogWriter.WriteAsync(family.DomainEvents, cancellationToken);
        family.ClearDomainEvents();

        // temporaryPassword is returned exactly once and never retrievable afterwards
        return new ProvisionMemberAccessResponse(
            authUserId,
            command.MemberId,
            email,
            temporaryPassword,
            MustChangePassword: true);
    }
}
