namespace DomusMind.Application.Abstractions.System;

/// <summary>
/// Server-side gate for the first-run initialization state.
/// Once initialized, the system permanently blocks re-initialization.
/// </summary>
public interface ISystemInitializationState
{
    /// <summary>Returns true if the system has been initialized.</summary>
    Task<bool> IsInitializedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Marks the system as initialized. Idempotent — calling more than once is safe.
    /// </summary>
    Task MarkInitializedAsync(CancellationToken cancellationToken);
}
