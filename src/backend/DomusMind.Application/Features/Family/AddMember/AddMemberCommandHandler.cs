using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Family;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Features.Family.AddMember;

public sealed class AddMemberCommandHandler : ICommandHandler<AddMemberCommand, AddMemberResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAuthorizationService _authorizationService;

    public AddMemberCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAuthorizationService authorizationService)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _authorizationService = authorizationService;
    }

    public async Task<AddMemberResponse> Handle(
        AddMemberCommand command,
        CancellationToken cancellationToken)
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

        var memberId = MemberId.New();
        var name = MemberName.Create(command.Name);
        var role = MemberRole.Create(command.Role);
        var now = DateTime.UtcNow;

        try
        {
            family.AddMember(memberId, name, role, now);
        }
        catch (InvalidOperationException ex)
        {
            throw new FamilyException(FamilyErrorCode.MemberAlreadyExists, ex.Message);
        }

        await _eventLogWriter.WriteAsync(family.DomainEvents, cancellationToken);
        family.ClearDomainEvents();

        return new AddMemberResponse(memberId.Value, family.Id.Value, name.Value, role.Value, now);
    }
}
