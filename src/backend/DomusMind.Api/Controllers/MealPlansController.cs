using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.MealPlanning.ApplyWeeklyTemplate;
using DomusMind.Application.Features.MealPlanning.CopyMealPlanFromPreviousWeek;
using DomusMind.Application.Features.MealPlanning.CreateMealPlan;
using DomusMind.Application.Features.MealPlanning.GetMealPlan;
using DomusMind.Application.Features.MealPlanning.GetMealPlansForAgenda;
using DomusMind.Application.Features.MealPlanning.RequestShoppingList;
using DomusMind.Application.Features.MealPlanning.UpdateMealSlot;
using DomusMind.Contracts.MealPlanning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Controllers;

[ApiController]
[Route("api/meal-plans")]
[Authorize]
public sealed class MealPlansController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public MealPlansController(ICurrentUser currentUser) => _currentUser = currentUser;

    /// <summary>Creates a new meal plan for a family week.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateMealPlanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateMealPlan(
        [FromBody] CreateMealPlanRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new CreateMealPlanCommand(
                    request.MealPlanId, request.FamilyId, request.WeekStart,
                    request.ResponsibilityDomainId, _currentUser.UserId!.Value),
                cancellationToken);
            if (response.AlreadyExisted)
                return Ok(response);
            return Created($"/api/meal-plans/{response.Id}", response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Returns a meal plan by its ID.</summary>
    [HttpGet("{planId:guid}")]
    [ProducesResponseType(typeof(GetMealPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMealPlan(
        Guid planId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetMealPlanQuery(planId, null, null, _currentUser.UserId!.Value),
                cancellationToken);
            if (response.MealPlan is null) return NotFound();
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
    }

    /// <summary>Returns the meal plan for a specific family and week. Returns 200 with null MealPlan when none exists.</summary>
    [HttpGet("family/{familyId:guid}/week/{weekStart}")]
    [ProducesResponseType(typeof(GetMealPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMealPlanByWeek(
        Guid familyId,
        DateOnly weekStart,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetMealPlanQuery(null, familyId, weekStart, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Updates a single meal slot in a meal plan.</summary>
    [HttpPut("{planId:guid}/slots/{dayOfWeek}/{mealType}")]
    [ProducesResponseType(typeof(UpdateMealSlotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateMealSlot(
        Guid planId,
        string dayOfWeek,
        string mealType,
        [FromBody] UpdateMealSlotRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new UpdateMealSlotCommand(
                    planId, dayOfWeek, mealType,
                    request.MealSourceType, request.RecipeId, request.FreeText,
                    request.Notes, request.IsOptional, request.IsLocked,
                    _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Creates a meal plan by applying a weekly template.</summary>
    [HttpPost("apply-template")]
    [ProducesResponseType(typeof(ApplyWeeklyTemplateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ApplyWeeklyTemplate(
        [FromBody] ApplyWeeklyTemplateRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new ApplyWeeklyTemplateCommand(
                    request.MealPlanId, request.FamilyId, request.WeekStart,
                    request.TemplateId, _currentUser.UserId!.Value),
                cancellationToken);
            if (response.AlreadyExisted)
                return Ok(response);
            return Created($"/api/meal-plans/{response.MealPlanId}", response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Creates a new meal plan by copying slots from a previous week.</summary>
    [HttpPost("copy-from-previous-week")]
    [ProducesResponseType(typeof(CopyMealPlanFromPreviousWeekResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CopyFromPreviousWeek(
        [FromBody] CopyMealPlanFromPreviousWeekRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new CopyMealPlanFromPreviousWeekCommand(
                    request.MealPlanId, request.FamilyId, request.WeekStart,
                    request.SourceMealPlanId, _currentUser.UserId!.Value),
                cancellationToken);
            if (!response.Success || response.AlreadyExisted)
                return Ok(response);
            return Created($"/api/meal-plans/{response.MealPlanId}", response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Requests generation of a shopping list from the meal plan's recipe slots.</summary>
    [HttpPost("{planId:guid}/shopping-list")]
    [ProducesResponseType(typeof(RequestShoppingListResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RequestShoppingList(
        Guid planId,
        [FromBody] RequestShoppingListRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new RequestShoppingListCommand(
                    planId, request.FamilyId, request.ShoppingListName,
                    _currentUser.UserId!.Value),
                cancellationToken);
            return Created($"/api/shared-lists/{response.ShoppingListId}", response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Returns meal plan slots for the agenda view for a specific week.</summary>
    [HttpGet("family/{familyId:guid}/agenda/{weekStart}")]
    [ProducesResponseType(typeof(MealPlansForAgendaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetForAgenda(
        Guid familyId,
        DateOnly weekStart,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetMealPlansForAgendaQuery(familyId, weekStart, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
    }
}
