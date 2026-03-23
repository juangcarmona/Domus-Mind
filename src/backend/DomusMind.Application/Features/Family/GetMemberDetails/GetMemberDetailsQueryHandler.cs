using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Family.GetFamilyMembers;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Family.GetMemberDetails;

public sealed class GetMemberDetailsQueryHandler : IQueryHandler<GetMemberDetailsQuery, MemberDetailResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;
    private readonly IAuthUserRepository _authUserRepository;

    public GetMemberDetailsQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService,
        IAuthUserRepository authUserRepository)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
        _authUserRepository = authUserRepository;
    }

    public async Task<MemberDetailResponse> Handle(GetMemberDetailsQuery query, CancellationToken cancellationToken)
    {
        var canAccess = await _authorizationService.CanAccessFamilyAsync(
            query.RequestedByUserId, query.FamilyId, cancellationToken);

        if (!canAccess)
            throw new FamilyException(FamilyErrorCode.AccessDenied, "Access to this family is denied.");

        var family = await _dbContext.Set<Domain.Family.Family>()
            .AsNoTracking()
            .Include(f => f.Members)
            .SingleOrDefaultAsync(f => f.Id == FamilyId.From(query.FamilyId), cancellationToken);

        if (family is null)
            throw new FamilyException(FamilyErrorCode.FamilyNotFound, "Family was not found.");

        var requestingMember = family.Members.FirstOrDefault(m => m.AuthUserId == query.RequestedByUserId);
        var isRequestingManager = requestingMember?.IsManager ?? false;

        var targetMember = family.Members.FirstOrDefault(m => m.Id == MemberId.From(query.MemberId));
        if (targetMember is null)
            throw new FamilyException(FamilyErrorCode.MemberNotFound, "Member was not found.");

        IReadOnlyDictionary<Guid, AuthUserStatusProjection> authStatus =
            new Dictionary<Guid, AuthUserStatusProjection>();

        if (targetMember.AuthUserId.HasValue)
            authStatus = await _authUserRepository.GetStatusByIdsAsync([targetMember.AuthUserId.Value], cancellationToken);

        var item = GetFamilyMembersQueryHandler.BuildDirectoryItem(
            targetMember, family.Id.Value, query.RequestedByUserId, isRequestingManager, authStatus);

        var lastLoginAtUtc = targetMember.AuthUserId.HasValue && authStatus.TryGetValue(targetMember.AuthUserId.Value, out var ap)
            ? ap.LastLoginAtUtc
            : null;

        return new MemberDetailResponse(
            item.MemberId,
            item.FamilyId,
            item.Name,
            item.PreferredName,
            item.Role,
            item.IsManager,
            item.BirthDate,
            item.JoinedAtUtc,
            item.AuthUserId,
            item.AccessStatus,
            item.LinkedEmail,
            lastLoginAtUtc,
            item.IsCurrentUser,
            item.HasAccount,
            item.CanGrantAccess,
            item.CanEdit,
            item.AvatarInitial,
            targetMember.PrimaryPhone,
            targetMember.PrimaryEmail,
            targetMember.HouseholdNote);
    }
}
