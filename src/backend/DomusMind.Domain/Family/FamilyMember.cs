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

    private FamilyMember(MemberId id, MemberName name, MemberRole role, bool isManager, DateOnly? birthDate, DateTime joinedAtUtc)
        : base(id)
    {
        Name = name;
        Role = role;
        IsManager = isManager;
        BirthDate = birthDate;
        JoinedAtUtc = joinedAtUtc;
    }

    // Backward-compatible overload
    internal static FamilyMember Create(MemberId id, MemberName name, MemberRole role, DateTime joinedAtUtc)
        => new(id, name, role, false, null, joinedAtUtc);

    // Extended overload
    internal static FamilyMember Create(MemberId id, MemberName name, MemberRole role, bool isManager, DateOnly? birthDate, DateTime joinedAtUtc)
        => new(id, name, role, isManager, birthDate, joinedAtUtc);

#pragma warning disable CS8618
    // EF Core parameterless constructor
    private FamilyMember() : base(default) { }
#pragma warning restore CS8618
}
