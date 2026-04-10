using DomusMind.Application.Features.Lists;
using DomusMind.Application.Features.Lists.CreateList;
using DomusMind.Domain.Lists;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Lists;

public sealed class CreateListCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CreateListCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new CreateListCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubListAuthorizationService());
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsResponse()
    {
        var familyId = Guid.NewGuid();
        var handler = BuildHandler();

        var result = await handler.Handle(
            new CreateListCommand(familyId, "Weekly Shopping", "Shopping", null, null, Guid.NewGuid()),
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
            new CreateListCommand(familyId, "Emergency Kit", "Preparation", null, null, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<SharedList>()
            .SingleOrDefaultAsync(l => l.Id == ListId.From(result.ListId));
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
            new CreateListCommand(Guid.NewGuid(), "Kitchen Items", "Shopping", areaId, null, Guid.NewGuid()),
            CancellationToken.None);

        result.AreaId.Should().Be(areaId);
    }

    [Fact]
    public async Task Handle_WithLinkedEntity_PersistsLinkage()
    {
        var handler = BuildHandler();
        var linkedId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateListCommand(Guid.NewGuid(), "Trip Checklist", "Preparation", null, linkedId, Guid.NewGuid()),
            CancellationToken.None);

        result.LinkedPlanId.Should().Be(linkedId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyName_ThrowsListException(string name)
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new CreateListCommand(Guid.NewGuid(), name, "Shopping", null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.InvalidInput);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyKind_ThrowsListException(string kind)
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new CreateListCommand(Guid.NewGuid(), "Shopping", kind, null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_WhenAccessDenied_ThrowsListException()
    {
        var auth = new StubListAuthorizationService { CanAccess = false };
        var handler = BuildHandler(auth: auth);

        var act = () => handler.Handle(
            new CreateListCommand(Guid.NewGuid(), "My List", "General", null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_WritesListCreatedEventToLog()
    {
        var eventLog = new StubListEventLogWriter();
        var db = CreateDb();
        var handler = new CreateListCommandHandler(db, eventLog, new StubListAuthorizationService());

        await handler.Handle(
            new CreateListCommand(Guid.NewGuid(), "Testing Events", "General", null, null, Guid.NewGuid()),
            CancellationToken.None);

        eventLog.WrittenEvents.Should().HaveCount(1);
    }
}
