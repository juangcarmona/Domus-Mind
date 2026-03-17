using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.Tasks;
using DomusMind.Application.Features.Tasks.CreateRoutine;
using DomusMind.Application.Features.Tasks.GetRoutinesByFamily;
using DomusMind.Application.Features.Tasks.PauseRoutine;
using DomusMind.Application.Features.Tasks.ResumeRoutine;
using DomusMind.Application.Features.Tasks.UpdateRoutine;
using DomusMind.Contracts.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Controllers;

[ApiController]
[Route("api/routines")]
[Authorize]
public sealed class RoutinesController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public RoutinesController(ICurrentUser currentUser) => _currentUser = currentUser;

    [HttpGet]
    [ProducesResponseType(typeof(RoutineListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRoutines(
        [FromQuery] Guid familyId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetRoutinesByFamilyQuery(familyId, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (TasksException ex)
        {
            return MapTasksException(ex);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateRoutineResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateRoutine(
        [FromBody] CreateRoutineRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new CreateRoutineCommand(
                    request.Name,
                    request.FamilyId,
                    request.Scope,
                    request.Kind,
                    request.Color,
                    request.Frequency,
                    request.DaysOfWeek,
                    request.DaysOfMonth,
                    request.MonthOfYear,
                    request.Time,
                    request.TargetMemberIds,
                    _currentUser.UserId!.Value),
                cancellationToken);

            return Created($"/api/routines/{response.RoutineId}", response);
        }
        catch (TasksException ex)
        {
            return MapTasksException(ex);
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UpdateRoutineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRoutine(
        Guid id,
        [FromBody] UpdateRoutineRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new UpdateRoutineCommand(
                    id,
                    request.Name,
                    request.Scope,
                    request.Kind,
                    request.Color,
                    request.Frequency,
                    request.DaysOfWeek,
                    request.DaysOfMonth,
                    request.MonthOfYear,
                    request.Time,
                    request.TargetMemberIds,
                    _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (TasksException ex)
        {
            return MapTasksException(ex);
        }
    }

    [HttpPost("{id:guid}/pause")]
    [ProducesResponseType(typeof(PauseRoutineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PauseRoutine(
        Guid id,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new PauseRoutineCommand(id, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (TasksException ex)
        {
            return MapTasksException(ex);
        }
    }

    [HttpPost("{id:guid}/resume")]
    [ProducesResponseType(typeof(ResumeRoutineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResumeRoutine(
        Guid id,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new ResumeRoutineCommand(id, _currentUser.UserId!.Value),
                cancellationToken);

            return Ok(response);
        }
        catch (TasksException ex)
        {
            return MapTasksException(ex);
        }
    }

    private IActionResult MapTasksException(TasksException ex) => ex.Code switch
    {
        TasksErrorCode.TaskNotFound         => NotFound(new { error = ex.Message }),
        TasksErrorCode.RoutineNotFound      => NotFound(new { error = ex.Message }),
        TasksErrorCode.AccessDenied         => StatusCode(403, new { error = ex.Message }),
        TasksErrorCode.TaskAlreadyCompleted => Conflict(new { error = ex.Message }),
        TasksErrorCode.TaskAlreadyCancelled => Conflict(new { error = ex.Message }),
        TasksErrorCode.RoutineAlreadyPaused => Conflict(new { error = ex.Message }),
        TasksErrorCode.RoutineAlreadyActive => Conflict(new { error = ex.Message }),
        TasksErrorCode.InvalidInput         => BadRequest(new { error = ex.Message }),
        _                                   => StatusCode(500, new { error = ex.Message }),
    };
}