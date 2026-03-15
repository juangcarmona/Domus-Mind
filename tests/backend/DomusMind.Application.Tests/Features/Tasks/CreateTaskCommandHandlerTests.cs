using DomusMind.Application.Features.Tasks;
using DomusMind.Application.Features.Tasks.CreateTask;
using DomusMind.Domain.Family;
using DomusMind.Domain.Tasks;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Tasks;

public sealed class CreateTaskCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CreateTaskCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubTasksAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new CreateTaskCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubTasksAuthorizationService());
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsResponse()
    {
        var handler = BuildHandler();
        var familyId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateTaskCommand("Buy milk", familyId, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.TaskId.Should().NotBeEmpty();
        result.FamilyId.Should().Be(familyId);
        result.Title.Should().Be("Buy milk");
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_PersistsTaskToDatabase()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new CreateTaskCommand("Take out trash", Guid.NewGuid(), null, null, Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<HouseholdTask>()
            .SingleOrDefaultAsync(t => t.Id == TaskId.From(result.TaskId));
        saved.Should().NotBeNull();
        saved!.Title.Value.Should().Be("Take out trash");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyTitle_ThrowsTasksException(string title)
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new CreateTaskCommand(title, Guid.NewGuid(), null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.InvalidInput);
    }

    [Fact]
    public async Task Handle_AccessDenied_ThrowsTasksException()
    {
        var auth = new StubTasksAuthorizationService { CanAccess = false };
        var handler = BuildHandler(auth: auth);

        var act = () => handler.Handle(
            new CreateTaskCommand("Valid Title", Guid.NewGuid(), null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.AccessDenied);
    }
}
