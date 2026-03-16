using DomusMind.Application.Features.Calendar;
using DomusMind.Application.Features.Calendar.SuggestEventParticipants;
using DomusMind.Domain.Calendar;
using DomusMind.Domain.Calendar.ValueObjects;
using DomusMind.Domain.Family;
using DomusMind.Domain.Family.ValueObjects;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Calendar;

public sealed class SuggestEventParticipantsQueryHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static SuggestEventParticipantsQueryHandler BuildHandler(
        DomusMindDbContext db,
        StubCalendarAuthorizationService? auth = null)
        => new(db, auth ?? new StubCalendarAuthorizationService());

    private static Domain.Calendar.CalendarEvent MakeEvent(
        FamilyId familyId, string title, DateTime? start = null)
        => Domain.Calendar.CalendarEvent.Create(
            CalendarEventId.New(), familyId,
            EventTitle.Create(title), null,
            start ?? DateTime.UtcNow.AddDays(1), null, DateTime.UtcNow);

    private static Domain.Family.Family MakeFamily(FamilyId familyId, params (MemberId id, string name)[] members)
    {
        var family = Domain.Family.Family.Create(familyId, FamilyName.Create("Test Family"), null, DateTime.UtcNow);
        foreach (var (id, name) in members)
            family.AddMember(id, MemberName.Create(name), MemberRole.Create("Adult"), DateTime.UtcNow);
        family.ClearDomainEvents();
        return family;
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsCalendarException()
    {
        var db = CreateDb();
        var auth = new StubCalendarAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db, auth);

        var act = () => handler.Handle(
            new SuggestEventParticipantsQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_EventNotFound_ThrowsCalendarException()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var act = () => handler.Handle(
            new SuggestEventParticipantsQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<CalendarException>()
            .Where(e => e.Code == CalendarErrorCode.EventNotFound);
    }

    [Fact]
    public async Task Handle_SuggestsMembersNotAlreadyParticipating()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member1 = MemberId.New();
        var member2 = MemberId.New();

        var family = MakeFamily(familyId, (member1, "Alice"), (member2, "Bob"));
        db.Set<Domain.Family.Family>().Add(family);

        var evt = MakeEvent(familyId, "School Trip");
        evt.AddParticipant(member1); // member1 already participating
        db.Set<Domain.Calendar.CalendarEvent>().Add(evt);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new SuggestEventParticipantsQuery(familyId.Value, evt.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Suggestions.Should().HaveCount(1);
        result.Suggestions.Single().MemberId.Should().Be(member2.Value);
    }

    [Fact]
    public async Task Handle_AllMembersParticipating_ReturnsEmptySuggestions()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var member1 = MemberId.New();

        var family = MakeFamily(familyId, (member1, "Alice"));
        db.Set<Domain.Family.Family>().Add(family);

        var evt = MakeEvent(familyId, "Dentist");
        evt.AddParticipant(member1);
        db.Set<Domain.Calendar.CalendarEvent>().Add(evt);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new SuggestEventParticipantsQuery(familyId.Value, evt.Id.Value, Guid.NewGuid()),
            CancellationToken.None);

        result.Suggestions.Should().BeEmpty();
    }
}
