using DomusMind.Domain.Abstractions;
using DomusMind.Domain.Family.Events;
using DomusMind.Domain.Family.ValueObjects;

namespace DomusMind.Domain.Family;

public sealed class Family : AggregateRoot<FamilyId>
{
    private readonly List<FamilyMember> _members = [];

    public FamilyName Name { get; private set; }
    public string? PrimaryLanguageCode { get; private set; }
    public string? FirstDayOfWeek { get; private set; }
    public string? DateFormatPreference { get; private set; }
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

    public void UpdateSettings(FamilyName name, string? primaryLanguageCode, string? firstDayOfWeek, string? dateFormatPreference, DateTime updatedAtUtc)
    {
        Name = name;
        PrimaryLanguageCode = primaryLanguageCode;
        FirstDayOfWeek = firstDayOfWeek;
        DateFormatPreference = dateFormatPreference;
        RaiseDomainEvent(new Events.FamilySettingsUpdated(Guid.NewGuid(), Id.Value, updatedAtUtc));
    }

    // Backward-compatible overload
    public FamilyMember AddMember(MemberId memberId, MemberName name, MemberRole role, DateTime addedAtUtc)
        => AddMember(memberId, name, role, false, null, addedAtUtc, null);

    public FamilyMember AddMember(MemberId memberId, MemberName name, MemberRole role, bool isManager, DateOnly? birthDate, DateTime addedAtUtc, Guid? authUserId = null)
    {
        if (isManager && role.Value != "Adult")
            throw new InvalidOperationException(
                "Manager role can only be assigned to adult members.");

        if (_members.Any(m => m.Id == memberId))
            throw new InvalidOperationException(
                $"A member with id '{memberId.Value}' already exists in this family.");

        var member = FamilyMember.Create(memberId, name, role, isManager, birthDate, addedAtUtc, authUserId);
        _members.Add(member);

        RaiseDomainEvent(new MemberAdded(Guid.NewGuid(), Id.Value, memberId.Value, addedAtUtc));

        return member;
    }

    public FamilyMember UpdateMember(MemberId memberId, MemberName name, MemberRole role, bool isManager, DateOnly? birthDate, DateTime updatedAtUtc)
    {
        if (isManager && role.Value != "Adult")
            throw new InvalidOperationException(
                "Manager role can only be assigned to adult members.");

        var member = _members.SingleOrDefault(m => m.Id == memberId)
            ?? throw new InvalidOperationException(
                $"A member with id '{memberId.Value}' does not exist in this family.");

        member.Update(name, role, isManager, birthDate);

        RaiseDomainEvent(new Events.MemberUpdated(Guid.NewGuid(), Id.Value, memberId.Value, updatedAtUtc));

        return member;
    }

    public FamilyMember LinkMemberAccount(MemberId memberId, Guid authUserId, DateTime linkedAtUtc)
    {
        var member = _members.SingleOrDefault(m => m.Id == memberId)
            ?? throw new InvalidOperationException(
                $"A member with id '{memberId.Value}' does not exist in this family.");

        if (member.AuthUserId.HasValue)
            throw new InvalidOperationException(
                $"Member '{memberId.Value}' already has a linked account.");

        member.LinkAccount(authUserId);

        RaiseDomainEvent(new Events.MemberAccountLinked(Guid.NewGuid(), Id.Value, memberId.Value, authUserId, linkedAtUtc));

        return member;
    }

    /// <summary>Updates the lightweight profile fields (preferred name, contacts, note) for an existing member.</summary>
    public FamilyMember UpdateMemberProfile(MemberId memberId, string? preferredName, string? primaryPhone, string? primaryEmail, string? householdNote, DateTime updatedAtUtc)
    {
        var member = _members.SingleOrDefault(m => m.Id == memberId)
            ?? throw new InvalidOperationException(
                $"A member with id '{memberId.Value}' does not exist in this family.");

        member.UpdateProfile(preferredName, primaryPhone, primaryEmail, householdNote);

        RaiseDomainEvent(new Events.MemberUpdated(Guid.NewGuid(), Id.Value, memberId.Value, updatedAtUtc));

        return member;
    }

#pragma warning disable CS8618
    // EF Core parameterless constructor
    private Family() : base(default) { }
#pragma warning restore CS8618
}
