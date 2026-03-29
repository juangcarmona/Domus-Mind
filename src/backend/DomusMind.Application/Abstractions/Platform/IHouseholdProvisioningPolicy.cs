namespace DomusMind.Application.Abstractions.Platform;

/// <summary>
/// Determines whether a new household may be created in the current deployment.
/// Enforces deployment-mode constraints before household bootstrap flows proceed.
/// Must not modify domain state.
/// </summary>
public interface IHouseholdProvisioningPolicy
{
    Task<ProvisioningPolicyResult> EvaluateAsync(CancellationToken cancellationToken);
}
