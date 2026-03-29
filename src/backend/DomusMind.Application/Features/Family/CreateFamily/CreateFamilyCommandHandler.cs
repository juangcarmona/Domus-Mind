using DomusMind.Application.Abstractions.Languages;
using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Platform;
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
    private readonly ISupportedLanguageReader _languageReader;
    private readonly IUserFamilyAccessReader _familyAccessReader;
    private readonly IHouseholdProvisioningPolicy _provisioningPolicy;
    private readonly IDeploymentModeContext _deploymentContext;

    public CreateFamilyCommandHandler(
        IDomusMindDbContext dbContext,
        IEventLogWriter eventLogWriter,
        IFamilyAccessGranter familyAccessGranter,
        ISupportedLanguageReader languageReader,
        IUserFamilyAccessReader familyAccessReader,
        IHouseholdProvisioningPolicy provisioningPolicy,
        IDeploymentModeContext deploymentContext)
    {
        _dbContext = dbContext;
        _eventLogWriter = eventLogWriter;
        _familyAccessGranter = familyAccessGranter;
        _languageReader = languageReader;
        _familyAccessReader = familyAccessReader;
        _provisioningPolicy = provisioningPolicy;
        _deploymentContext = deploymentContext;
    }

    public async Task<CreateFamilyResponse> Handle(
        CreateFamilyCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new FamilyException(FamilyErrorCode.InvalidInput, "Family name is required.");

        if (command.Name.Trim().Length > 100)
            throw new FamilyException(FamilyErrorCode.InvalidInput, "Family name cannot exceed 100 characters.");

        var policyResult = await _provisioningPolicy.EvaluateAsync(cancellationToken);
        if (!policyResult.Allowed)
            throw new FamilyException(FamilyErrorCode.HouseholdCreationNotAllowed, policyResult.Message, policyResult.ReasonCode);

        var existingFamilyId = await _familyAccessReader.GetFamilyIdForUserAsync(
            command.RequestedByUserId, cancellationToken);

        if (existingFamilyId.HasValue)
            throw new FamilyException(
                FamilyErrorCode.FamilyAlreadyExists,
                "You already have a family associated with your account. Onboarding can only be completed once.");

        string? languageCode = null;
        if (!string.IsNullOrWhiteSpace(command.PrimaryLanguageCode))
        {
            var isValid = await _languageReader.IsActiveAsync(command.PrimaryLanguageCode, cancellationToken);
            if (!isValid)
                throw new FamilyException(
                    FamilyErrorCode.InvalidInput,
                    $"Language code '{command.PrimaryLanguageCode}' is not a supported language.");
            languageCode = command.PrimaryLanguageCode;
        }

        var familyId = FamilyId.New();
        var name = FamilyName.Create(command.Name);
        var now = DateTime.UtcNow;

        var family = Domain.Family.Family.Create(familyId, name, languageCode, now);

        _dbContext.Set<Domain.Family.Family>().Add(family);

        // For SingleInstance, mark this family as the singleton. The DB unique index on
        // singleton_key closes the TOCTOU race between the policy pre-check and the INSERT.
        if (_deploymentContext.Mode == DeploymentMode.SingleInstance)
            _dbContext.SetProperty(family, "singleton_key", "singleton");

        await _familyAccessGranter.GrantAccessAsync(command.RequestedByUserId, familyId.Value, cancellationToken);

        await _eventLogWriter.WriteAsync(family.DomainEvents, cancellationToken);
        family.ClearDomainEvents();

        return new CreateFamilyResponse(familyId.Value, name.Value, languageCode, now);
    }
}
