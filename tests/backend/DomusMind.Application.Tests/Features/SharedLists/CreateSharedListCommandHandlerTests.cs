using DomusMind.Application.Features.SharedLists;
using DomusMind.Application.Features.SharedLists.CreateSharedList;
using DomusMind.Domain.SharedLists;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.SharedLists;

public sealed class CreateSharedListCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CreateSharedListCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubSharedListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new CreateSharedListCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubSharedListAuthorizationService());
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsResponse()
    {
        var familyId = Guid.NewGuid();
        var handler = BuildHandler();

        var result = await handler.Handle(
            new CreateSharedListCommand(familyId, "Weekly Shopping", "Shopping", null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.ListId.Should().NotBeEmpty();
        result.FamilyId.Should().Be(familyId);
        result.Name.Should().Be("Weekly Shopping");
        result.Kind.Should().Be("Shopping");
    }

    [Fact]
    public async Task Handle_PersistsListToDatabase()
    {
        var db = CreateDb();
        var handler = BuildHandler(db: db);
        var familyId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateSharedListCommand(familyId, "Emergency Kit", "Preparation", null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<SharedList>()
            .SingleOrDefaultAsync(l => l.Id == SharedListId.From(result.ListId));
        saved.Should().NotBeNull();
        saved!.Name.Value.Should().Be("Emergency Kit");
    }

    [Fact]
    public async Task Handle_WithAreaId_PersistsAreaId()
    {
        var db = CreateDb();
        var handler = BuildHandler(db: db);
        var areaId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateSharedListCommand(Guid.NewGuid(), "Kitchen Items", "Shopping", areaId, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.AreaId.Should().Be(areaId);
    }

    [Fact]
    public async Task Handle_WithLinkedEntity_PersistsLinkage()
    {
        var handler = BuildHandler();
        var linkedId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateSharedListCommand(Guid.NewGuid(), "Trip Checklist", "Preparation", null, "CalendarEvent", linkedId, Guid.NewGuid()),
            CancellationToken.None);

        result.LinkedEntityType.Should().Be("CalendarEvent");
        result.LinkedEntityId.Should().Be(linkedId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyName_ThrowsSharedListException(string name)
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new CreateSharedListCommand(Guid.NewGuid(), name, "Shopping", null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(e => e.Code == SharedListErrorCode.InvalidInput);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyKind_ThrowsSharedListException(string kind)
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new CreateSharedListCommand(Guid.NewGuid(), "Shopping", kind, null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(e => e.Code == SharedListErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_WhenAccessDenied_ThrowsSharedListException()
    {
        var auth = new StubSharedListAuthorizationService { CanAccess = false };
        var handler = BuildHandler(auth: auth);

        var act = () => handler.Handle(
            new CreateSharedListCommand(Guid.NewGuid(), "My List", "General", null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<SharedListException>()
            .Where(e => e.Code == SharedListErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_WritesSharedListCreatedEventToLog()
    {
        var eventLog = new StubSharedListEventLogWriter();
        var db = CreateDb();
        var handler = new CreateSharedListCommandHandler(db, eventLog, new StubSharedListAuthorizationService());

        await handler.Handle(
            new CreateSharedListCommand(Guid.NewGuid(), "Testing Events", "General", null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        eventLog.WrittenEvents.Should().HaveCount(1);
    }
}
