using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Family;
using DomusMind.Contracts.Family;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;

namespace DomusMind.Application.Features.Family.CreateFamily;

public sealed class CreateFamilyCommandHandler : ICommandHandler<CreateFamilyCommand, CreateFamilyResponse>
{
    private readonly IDomusMindDbContext _dbContext;
    private readonly IEventLogWriter _eventLogWriter;
    private readonly IFamilyAccessGranter _familyAccessGranter;

    public CreateFamilyCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAccessGranter familyAccessGranter)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _familyAccessGranter = familyAccessGranter;
    }

    public async Task<CreateFamilyResponse> Handle(
        CreateFamilyCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new FamilyException(FamilyErrorCode.InvalidInput, "Family name is required.");

        if (command.Name.Trim().Length > 100)
            throw new FamilyException(FamilyErrorCode.InvalidInput, "Family name cannot exceed 100 characters.");

        var familyId = FamilyId.New();
        var name = FamilyName.Create(command.Name);
        var now = DateTime.UtcNow;

        var family = Domain.Family.Family.Create(familyId, name, now);

        _dbContext.Set<Domain.Family.Family>().Add(family);

        await _familyAccessGranter.GrantAccessAsync(command.RequestedByUserId, familyId.Value, cancellationToken);

        await _eventLogWriter.WriteAsync(family.DomainEvents, cancellationToken);
        family.ClearDomainEvents();

        return new CreateFamilyResponse(familyId.Value, name.Value, now);
    }
}
