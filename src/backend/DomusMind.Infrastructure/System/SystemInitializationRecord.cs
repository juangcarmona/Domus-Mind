namespace DomusMind.Infrastructure.Initialization;

/// <summary>
/// Single-row sentinel that records when the system was first initialized.
/// Id is always 1. A PK violation on insert is the idempotency guard.
/// </summary>
public sealed class SystemInitializationRecord
{
    /// <summary>Always 1 — enforced by ValueGeneratedNever at the EF layer.</summary>
    public int Id { get; init; } = 1;

    public DateTimeOffset InitializedAtUtc { get; init; }
}
