using DomusMind.Application.Abstractions.Languages;
using DomusMind.Application.Abstractions.Persistence;
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
