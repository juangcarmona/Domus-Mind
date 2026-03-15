using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Domain.Abstractions;

namespace DomusMind.Application.Tests.Features.Family;

internal sealed class StubFamilyTimelineAuthorizationService : IFamilyAuthorizationService
{
    public bool CanAccess { get; set; } = true;

    public Task<bool> CanAccessFamilyAsync(
        Guid userId, Guid familyId, CancellationToken cancellationToken = default)
        => Task.FromResult(CanAccess);
}
