using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family.ValueObjects;

namespace DomusMind.Domain.Family;

public sealed class FamilyMember : Entity<MemberId>
{
    public MemberName Name { get; private set; }
    public MemberRole Role { get; private set; }
    public bool IsManager { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }

    /// <summary>
    /// Links this member to an authenticated user. Only set for members who have a
    /// DomusMind account (typically adults who registered). Children and pets have no
    /// auth account and therefore no AuthUserId.
    /// </summary>
    public Guid? AuthUserId { get; private set; }

    private FamilyMember(MemberId id, MemberName name, MemberRole role, bool isManager, DateOnly? birthDate, DateTime joinedAtUtc, Guid? authUserId = null)
        : base(id)
    {
        Name = name;
        Role = role;
        IsManager = isManager;
        BirthDate = birthDate;
        JoinedAtUtc = joinedAtUtc;
        AuthUserId = authUserId;
    }

    // Backward-compatible overload
    internal static FamilyMember Create(MemberId id, MemberName name, MemberRole role, DateTime joinedAtUtc)
        => new(id, name, role, false, null, joinedAtUtc);

    // Extended overload
    internal static FamilyMember Create(MemberId id, MemberName name, MemberRole role, bool isManager, DateOnly? birthDate, DateTime joinedAtUtc, Guid? authUserId = null)
        => new(id, name, role, isManager, birthDate, joinedAtUtc, authUserId);

#pragma warning disable CS8618
    // EF Core parameterless constructor
    private FamilyMember() : base(default) { }
#pragma warning restore CS8618
}
