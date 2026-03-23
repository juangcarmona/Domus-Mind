using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Family.EnableMemberAccess;

/// <summary>
/// Admin-only: re-enables a previously disabled member account.
/// </summary>
public sealed class EnableMemberAccessCommandHandler
    : ICommandHandler<EnableMemberAccessCommand, EnableMemberAccessResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;
    private readonly IAuthUserRepository _authUserRepository;

    public EnableMemberAccessCommandHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService,
        IAuthUserRepository authUserRepository)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
        _authUserRepository = authUserRepository;
    }

    public async Task<EnableMemberAccessResponse> Handle(
        EnableMemberAccessCommand command,
        CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            command.RequestedByUserId, command.FamilyId, cancellationToken);

        if (!canAccess)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Access to this family is denied.");

        var family = await _dbContext.Set<Domain.Family.Family>()
            .AsNoTracking()
            .Include(f => f.Members)
            .SingleOrDefaultAsync(f => f.Id == FamilyId.From(command.FamilyId), cancellationToken);

        if (family is null)
            throw new FamilyException(FamilyErrorCode.FamilyNotFound, "Family was not found.");

        var requestingMember = family.Members.FirstOrDefault(m => m.AuthUserId == command.RequestedByUserId);
        if (requestingMember is null || !requestingMember.IsManager)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Only household managers can enable member access.");

        var memberId = MemberId.From(command.MemberId);
        var targetMember = family.Members.FirstOrDefault(m => m.Id == memberId);
        if (targetMember is null)
            throw new FamilyException(FamilyErrorCode.MemberNotFound, "Member was not found.");

        if (!targetMember.AuthUserId.HasValue)
            throw new FamilyException(FamilyErrorCode.InvalidInput, "This member does not have a linked account.");

        await _authUserRepository.EnableUserAsync(targetMember.AuthUserId.Value, cancellationToken);

        return new EnableMemberAccessResponse(command.MemberId);
    }
}
