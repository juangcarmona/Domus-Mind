using DomusMind.Application.Features.Lists;
using DomusMind.Application.Features.Lists.AddItemToList;
using DomusMind.Domain.Family;
using DomusMind.Domain.Lists;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Lists;

public sealed class AddItemToListCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static AddItemToListCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubListAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new AddItemToListCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubListAuthorizationService());
    }

    private static async Task<(DomusMindDbContext db, SharedList list)> SeedListAsync()
    {
        var db = CreateDb();
        var familyId = FamilyId.New();
        var list = ListTestHelpers.MakeList(familyId);
        list.ClearDomainEvents();
        db.Set<SharedList>().Add(list);
        await db.SaveChangesAsync();
        return (db, list);
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsResponse()
    {
        var (db, list) = await SeedListAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new AddItemToListCommand(list.Id.Value, "Milk", "2L", "whole milk", null, Guid.NewGuid()),
            CancellationToken.None);

        result.ItemId.Should().NotBeEmpty();
        result.ListId.Should().Be(list.Id.Value);
        result.Name.Should().Be("Milk");
        result.Checked.Should().BeFalse();
        result.Order.Should().Be(1);
        result.Quantity.Should().Be("2L");
        result.Note.Should().Be("whole milk");
    }

    [Fact]
    public async Task Handle_PersistsItemToDatabase()
    {
        var (db, list) = await SeedListAsync();
        var handler = BuildHandler(db: db);

        var result = await handler.Handle(
            new AddItemToListCommand(list.Id.Value, "Eggs", null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        var savedList = await db.Set<SharedList>()
            .Include(l => l.Items)
            .SingleAsync(l => l.Id == list.Id);
        savedList.Items.Should().HaveCount(1);
        savedList.Items.Single().Name.Value.Should().Be("Eggs");
    }

    [Fact]
    public async Task Handle_SecondItem_AssignsOrderTwo()
    {
        var (db, list) = await SeedListAsync();
        var handler = BuildHandler(db: db);

        await handler.Handle(
            new AddItemToListCommand(list.Id.Value, "Bread", null, null, null, Guid.NewGuid()),
            CancellationToken.None);
        var result = await handler.Handle(
            new AddItemToListCommand(list.Id.Value, "Butter", null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.Order.Should().Be(2);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyName_ThrowsListException(string name)
    {
        var (_, list) = await SeedListAsync();
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new AddItemToListCommand(list.Id.Value, name, null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_ListNotFound_ThrowsListException()
    {
        var handler = BuildHandler();
        var missingListId = Guid.NewGuid();

        var act = () => handler.Handle(
            new AddItemToListCommand(missingListId, "Item", null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.ListNotFound);
    }

    [Fact]
    public async Task Handle_WhenAccessDenied_ThrowsListException()
    {
        var (db, list) = await SeedListAsync();
        var auth = new StubListAuthorizationService { CanAccess = false };
        var handler = BuildHandler(db: db, auth: auth);

        var act = () => handler.Handle(
            new AddItemToListCommand(list.Id.Value, "Item", null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ListException>()
            .Where(e => e.Code == ListErrorCode.AccessDenied);
    }

    [Fact]
    public async Task Handle_WritesListItemAddedEventToLog()
    {
        var (db, list) = await SeedListAsync();
        var eventLog = new StubListEventLogWriter();
        var handler = new AddItemToListCommandHandler(db, eventLog, new StubListAuthorizationService());

        await handler.Handle(
            new AddItemToListCommand(list.Id.Value, "Tomatoes", null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        eventLog.WrittenEvents.Should().HaveCount(1);
    }
}
