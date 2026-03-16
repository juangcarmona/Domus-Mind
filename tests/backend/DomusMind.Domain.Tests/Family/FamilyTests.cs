using DomusMind.Domain.Family;
using DomusMind.Domain.Family.Events;
using DomusMind.Domain.Family.ValueObjects;
using FluentAssertions;

namespace DomusMind.Domain.Tests.Family;

public sealed class FamilyTests
{
    private static Domain.Family.Family BuildFamily(string name = "Smith Family")
    {
        var id = FamilyId.New();
        var familyName = FamilyName.Create(name);
        return Domain.Family.Family.Create(id, familyName, null, DateTime.UtcNow);
    }

    // ── Family.Create ──────────────────────────────────────────────────────────

    [Fact]
    public void Create_GivenValidName_SetsFamilyName()
    {
        var family = BuildFamily("Johnson Family");

        family.Name.Value.Should().Be("Johnson Family");
    }

    [Fact]
    public void Create_SetsCreatedAtUtc()
    {
        var before = DateTime.UtcNow;
        var family = BuildFamily();
        var after = DateTime.UtcNow;

        family.CreatedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_EmitsOneFamilyCreatedEvent()
    {
        var family = BuildFamily();

        family.DomainEvents.Should().HaveCount(1);
        family.DomainEvents.Single().Should().BeOfType<FamilyCreated>();
    }

    [Fact]
    public void Create_FamilyCreatedEvent_ContainsCorrectFamilyId()
    {
        var id = FamilyId.New();
        var family = Domain.Family.Family.Create(id, FamilyName.Create("Test"), null, DateTime.UtcNow);

        var evt = family.DomainEvents.OfType<FamilyCreated>().Single();
        evt.FamilyId.Should().Be(id.Value);
    }

    [Fact]
    public void Create_StartsWithNoMembers()
    {
        var family = BuildFamily();

        family.Members.Should().BeEmpty();
    }

    // ── AddMember ──────────────────────────────────────────────────────────────

    [Fact]
    public void AddMember_GivenValidInput_AddsMemberToRoster()
    {
        var family = BuildFamily();

        family.AddMember(MemberId.New(), MemberName.Create("Alice"), MemberRole.Adult, DateTime.UtcNow);

        family.Members.Should().HaveCount(1);
    }

    [Fact]
    public void AddMember_EmitsMemberAddedEvent()
    {
        var family = BuildFamily();
        family.ClearDomainEvents();

        family.AddMember(MemberId.New(), MemberName.Create("Bob"), MemberRole.Child, DateTime.UtcNow);

        family.DomainEvents.Should().HaveCount(1);
        family.DomainEvents.Single().Should().BeOfType<MemberAdded>();
    }

    [Fact]
    public void AddMember_MemberAddedEvent_ContainsCorrectIds()
    {
        var family = BuildFamily();
        family.ClearDomainEvents();

        var memberId = MemberId.New();
        family.AddMember(memberId, MemberName.Create("Carol"), MemberRole.Adult, DateTime.UtcNow);

        var evt = family.DomainEvents.OfType<MemberAdded>().Single();
        evt.FamilyId.Should().Be(family.Id.Value);
        evt.MemberId.Should().Be(memberId.Value);
    }

    [Fact]
    public void AddMember_DuplicateMemberId_ThrowsInvalidOperationException()
    {
        var family = BuildFamily();
        var memberId = MemberId.New();
        family.AddMember(memberId, MemberName.Create("Alice"), MemberRole.Adult, DateTime.UtcNow);

        var act = () => family.AddMember(memberId, MemberName.Create("Alice Again"), MemberRole.Adult, DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddMember_ReturnsMemberWithCorrectValues()
    {
        var family = BuildFamily();
        var memberId = MemberId.New();
        var now = DateTime.UtcNow;

        var member = family.AddMember(memberId, MemberName.Create("Dave"), MemberRole.Child, now);

        member.Id.Should().Be(memberId);
        member.Name.Value.Should().Be("Dave");
        member.Role.Value.Should().Be("Child");
        member.JoinedAtUtc.Should().Be(now);
    }

    // ── FamilyName value object ────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FamilyName_Create_EmptyOrWhitespace_ThrowsArgumentException(string value)
    {
        var act = () => FamilyName.Create(value);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FamilyName_Create_TrimsWhitespace()
    {
        var name = FamilyName.Create("  Smith  ");

        name.Value.Should().Be("Smith");
    }

    [Fact]
    public void FamilyName_Create_ExceededLength_ThrowsArgumentException()
    {
        var act = () => FamilyName.Create(new string('X', 101));

        act.Should().Throw<ArgumentException>();
    }

    // ── MemberRole value object ───────────────────────────────────────────────

    [Theory]
    [InlineData("Adult")]
    [InlineData("Child")]
    [InlineData("Caregiver")]
    public void MemberRole_Create_ValidRole_Succeeds(string role)
    {
        var result = MemberRole.Create(role);

        result.Value.Should().Be(role);
    }

    [Fact]
    public void MemberRole_Create_InvalidRole_ThrowsArgumentException()
    {
        var act = () => MemberRole.Create("Unknown");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MemberRole_Create_CaseInsensitive_Normalises()
    {
        var result = MemberRole.Create("adult");

        result.Value.Should().Be("Adult");
    }

    // ── ClearDomainEvents ─────────────────────────────────────────────────────

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var family = BuildFamily();
        family.DomainEvents.Should().NotBeEmpty();

        family.ClearDomainEvents();

        family.DomainEvents.Should().BeEmpty();
    }
}
