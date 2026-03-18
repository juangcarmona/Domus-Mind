using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Family;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Family;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Family.GetFamilyMembers;

public sealed class GetFamilyMembersQueryHandler
    : IQueryHandler<GetFamilyMembersQuery, IReadOnlyCollection<FamilyMemberResponse>>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IFamilyAuthorizationService _authorizationService;

    public GetFamilyMembersQueryHandler(
        IDomusMindDbContext dbContext,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _authorizationService = authorizationService;
    }

    public async Task<IReadOnlyCollection<FamilyMemberResponse>> Handle(
        GetFamilyMembersQuery query,
        CancellationToken cancellationToken)
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

        return family.Members
            .Select(m => new FamilyMemberResponse(
                m.Id.Value,
                family.Id.Value,
                m.Name.Value,
                m.Role.Value,
                m.IsManager,
                m.BirthDate,
                m.JoinedAtUtc,
                m.AuthUserId))
            .ToList()
            .AsReadOnly();
    }
}
