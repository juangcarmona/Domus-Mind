using DomusMind.Application.Abstractions.Languages;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Platform;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Domain.Abstractions;

namespace DomusMind.Application.Tests.Features.Family;

/// <summary>Shared test stubs for Family handler tests.</summary>
internal sealed class StubFamilyAuthorizationService : IFamilyAuthorizationService
{
    public bool CanAccess { get; set; } = true;

    public Task<bool> CanAccessFamilyAsync(Guid userId, Guid familyId, CancellationToken cancellationToken = default)
        => Task.FromResult(CanAccess);
}

internal sealed class StubFamilyAccessGranter : IFamilyAccessGranter
{
    public List<(Guid UserId, Guid FamilyId)> GrantedAccesses { get; } = [];

    public Task GrantAccessAsync(Guid userId, Guid familyId, CancellationToken cancellationToken = default)
    {
        GrantedAccesses.Add((userId, familyId));
        return Task.CompletedTask;
    }
}

internal sealed class StubUserFamilyAccessReader : IUserFamilyAccessReader
{
    private readonly Guid? _familyId;

    public StubUserFamilyAccessReader(Guid? familyId = null)
    {
        _familyId = familyId;
    }

    public Task<Guid?> GetFamilyIdForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult(_familyId);

    public Task<IReadOnlyDictionary<Guid, Guid>> GetAllFamilyIdsByUserAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyDictionary<Guid, Guid>>(new Dictionary<Guid, Guid>());
}

internal sealed class StubEventLogWriter : IEventLogWriter
{
    public List<IDomainEvent> WrittenEvents { get; } = [];

    public Task WriteAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        WrittenEvents.AddRange(domainEvents);
        return Task.CompletedTask;
    }
}

internal sealed class StubSupportedLanguageReader : ISupportedLanguageReader
{
    private readonly HashSet<string> _supportedCodes;

    public StubSupportedLanguageReader(IEnumerable<string>? supportedCodes = null)
    {
        _supportedCodes = new HashSet<string>(
            supportedCodes ?? ["en", "de", "es", "fr", "it", "ja", "zh"],
            StringComparer.OrdinalIgnoreCase);
    }

    public Task<IReadOnlyCollection<SupportedLanguageItem>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var items = _supportedCodes
            .Select(c => new SupportedLanguageItem(c, c + "-XX", c, c, c == "en", 0))
            .ToList();
        return Task.FromResult<IReadOnlyCollection<SupportedLanguageItem>>(items.AsReadOnly());
    }

    public Task<bool> IsActiveAsync(string code, CancellationToken cancellationToken = default)
        => Task.FromResult(_supportedCodes.Contains(code));
}

internal sealed class StubHouseholdProvisioningPolicy : IHouseholdProvisioningPolicy
{
    private readonly ProvisioningPolicyResult _result;

    public StubHouseholdProvisioningPolicy(bool allowed = true)
    {
        _result = allowed
            ? ProvisioningPolicyResult.Permit()
            : ProvisioningPolicyResult.DenySingleInstanceBound();
    }

    public StubHouseholdProvisioningPolicy(ProvisioningPolicyResult result)
    {
        _result = result;
    }

    public Task<ProvisioningPolicyResult> EvaluateAsync(CancellationToken cancellationToken)
        => Task.FromResult(_result);
}

internal sealed class StubDeploymentModeContext : IDeploymentModeContext
{
    public StubDeploymentModeContext(DeploymentMode mode = DeploymentMode.SingleInstance)
    {
        Mode = mode;
    }

    public DeploymentMode Mode { get; }
    public bool CanCreateHousehold => true;
    public bool InvitationsEnabled => false;
    public bool RequireInvitationForSignup => false;
    public bool EmailEnabled => false;
    public bool SupportsAdminTools => false;
    public int MaxHouseholdsPerDeployment => 0;
}
