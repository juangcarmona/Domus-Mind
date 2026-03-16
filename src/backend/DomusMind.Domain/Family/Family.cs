using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family.Events;
using DomusMind.Domain.Family.ValueObjects;

namespace DomusMind.Domain.Family;

public sealed class Family : AggregateRoot<FamilyId>
{
    private readonly List<FamilyMember> _members = [];

    public FamilyName Name { get; private set; }
    public string? PrimaryLanguageCode { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<FamilyMember> Members => _members.AsReadOnly();

    private Family(FamilyId id, FamilyName name, string? primaryLanguageCode, DateTime createdAtUtc)
        : base(id)
    {
        Name = name;
        PrimaryLanguageCode = primaryLanguageCode;
        CreatedAtUtc = createdAtUtc;
    }

    public static Family Create(FamilyId id, FamilyName name, string? primaryLanguageCode, DateTime createdAtUtc)
    {
        var family = new Family(id, name, primaryLanguageCode, createdAtUtc);
        family.RaiseDomainEvent(new FamilyCreated(Guid.NewGuid(), id.Value, createdAtUtc));
        return family;
    }

    // Backward-compatible overload
    public FamilyMember AddMember(MemberId memberId, MemberName name, MemberRole role, DateTime addedAtUtc)
        => AddMember(memberId, name, role, false, null, addedAtUtc);

    public FamilyMember AddMember(MemberId memberId, MemberName name, MemberRole role, bool isManager, DateOnly? birthDate, DateTime addedAtUtc)
    {
        if (isManager && role.Value != "Adult")
            throw new InvalidOperationException(
                "Manager role can only be assigned to adult members.");

        if (_members.Any(m => m.Id == memberId))
            throw new InvalidOperationException(
                $"A member with id '{memberId.Value}' already exists in this family.");

        var member = FamilyMember.Create(memberId, name, role, isManager, birthDate, addedAtUtc);
        _members.Add(member);

        RaiseDomainEvent(new MemberAdded(Guid.NewGuid(), Id.Value, memberId.Value, addedAtUtc));

        return member;
    }

#pragma warning disable CS8618
    // EF Core parameterless constructor
    private Family() : base(default) { }
#pragma warning restore CS8618
}
