namespace DomusMind.Infrastructure.Persistence;

/// <summary>
/// Records that a given auth user has access to a given family.
/// This is an infrastructure-only authorization model; it does not belong to the domain.
/// </summary>
public sealed class UserFamilyAccess
{
    public Guid UserId { get; init; }
    public Guid FamilyId { get; init; }
    public DateTime GrantedAtUtc { get; init; }
}
