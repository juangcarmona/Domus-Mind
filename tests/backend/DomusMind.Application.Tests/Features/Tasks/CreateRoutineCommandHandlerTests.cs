using DomusMind.Application.Features.Tasks;
using DomusMind.Application.Features.Tasks.CreateRoutine;
using DomusMind.Domain.Tasks;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DomusMind.Application.Tests.Features.Tasks;

public sealed class CreateRoutineCommandHandlerTests
{
    private static DomusMindDbContext CreateDb()
        => new(new DbContextOptionsBuilder<DomusMindDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CreateRoutineCommandHandler BuildHandler(
        DomusMindDbContext? db = null,
        StubTasksAuthorizationService? auth = null)
    {
        var context = db ?? CreateDb();
        return new CreateRoutineCommandHandler(
            context, new EventLogWriter(context),
            auth ?? new StubTasksAuthorizationService());
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsResponse()
    {
        var handler = BuildHandler();
        var familyId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateRoutineCommand("Morning Workout", familyId, "Daily at 07:00", Guid.NewGuid()),
            CancellationToken.None);

        result.RoutineId.Should().NotBeEmpty();
        result.FamilyId.Should().Be(familyId);
        result.Name.Should().Be("Morning Workout");
        result.Cadence.Should().Be("Daily at 07:00");
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_PersistsRoutineToDatabase()
    {
        var db = CreateDb();
        var handler = BuildHandler(db);

        var result = await handler.Handle(
            new CreateRoutineCommand("Evening Walk", Guid.NewGuid(), "Daily at 18:30", Guid.NewGuid()),
            CancellationToken.None);

        var saved = await db.Set<Routine>()
            .SingleOrDefaultAsync(r => r.Id == RoutineId.From(result.RoutineId));
        saved.Should().NotBeNull();
        saved!.Name.Value.Should().Be("Evening Walk");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyName_ThrowsTasksException(string name)
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new CreateRoutineCommand(name, Guid.NewGuid(), "Weekly", Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.InvalidInput);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_EmptyCadence_ThrowsTasksException(string cadence)
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new CreateRoutineCommand("Valid Name", Guid.NewGuid(), cadence, Guid.NewGuid()),
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
            new CreateRoutineCommand("Weekend Clean", Guid.NewGuid(), "Weekly", Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<TasksException>()
            .Where(e => e.Code == TasksErrorCode.AccessDenied);
    }
}
