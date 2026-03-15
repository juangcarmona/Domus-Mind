using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Tasks;
using DomusMind.Application.Features.Tasks.AssignTask;
using DomusMind.Application.Features.Tasks.CancelTask;
using DomusMind.Application.Features.Tasks.CompleteTask;
using DomusMind.Application.Features.Tasks.CreateTask;
using DomusMind.Application.Features.Tasks.ReassignTask;
using DomusMind.Application.Features.Tasks.RescheduleTask;
using DomusMind.Contracts.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public sealed class TasksController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public TasksController(ICurrentUser currentUser) => _currentUser = currentUser;

    [HttpPost]
    public async Task<IActionResult> CreateTask(
        [FromBody] CreateTaskRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new CreateTaskCommand(
                    request.Title, request.FamilyId, request.Description,
                    request.DueDate, _currentUser.UserId!.Value),
                cancellationToken);
            return Created($"/api/tasks/{response.TaskId}", response);
        }
        catch (TasksException ex) { return MapTasksException(ex); }
    }

    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> AssignTask(
        Guid id,
        [FromBody] AssignTaskRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new AssignTaskCommand(id, request.AssigneeId, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (TasksException ex) { return MapTasksException(ex); }
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> CompleteTask(
        Guid id,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new CompleteTaskCommand(id, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (TasksException ex) { return MapTasksException(ex); }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelTask(
        Guid id,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new CancelTaskCommand(id, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (TasksException ex) { return MapTasksException(ex); }
    }

    [HttpPost("{id:guid}/reschedule")]
    public async Task<IActionResult> RescheduleTask(
        Guid id,
        [FromBody] RescheduleTaskRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new RescheduleTaskCommand(id, request.NewDueDate, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (TasksException ex) { return MapTasksException(ex); }
    }

    [HttpPost("{id:guid}/reassign")]
    public async Task<IActionResult> ReassignTask(
        Guid id,
        [FromBody] ReassignTaskRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new ReassignTaskCommand(id, request.NewAssigneeId, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (TasksException ex) { return MapTasksException(ex); }
    }

    private IActionResult MapTasksException(TasksException ex) => ex.Code switch
    {
        TasksErrorCode.TaskNotFound          => NotFound(new { error = ex.Message }),
        TasksErrorCode.RoutineNotFound       => NotFound(new { error = ex.Message }),
        TasksErrorCode.AccessDenied          => StatusCode(403, new { error = ex.Message }),
        TasksErrorCode.TaskAlreadyCompleted  => Conflict(new { error = ex.Message }),
        TasksErrorCode.TaskAlreadyCancelled  => Conflict(new { error = ex.Message }),
        TasksErrorCode.RoutineAlreadyPaused  => Conflict(new { error = ex.Message }),
        TasksErrorCode.RoutineAlreadyActive  => Conflict(new { error = ex.Message }),
        TasksErrorCode.InvalidInput          => BadRequest(new { error = ex.Message }),
        _                                    => StatusCode(500, new { error = ex.Message }),
    };
}
