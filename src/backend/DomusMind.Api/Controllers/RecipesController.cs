using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Features.MealPlanning.AddRecipeIngredient;
using DomusMind.Application.Features.MealPlanning.CreateRecipe;
using DomusMind.Application.Features.MealPlanning.DeleteRecipe;
using DomusMind.Application.Features.MealPlanning.GetFamilyRecipes;
using DomusMind.Application.Features.MealPlanning.GetRecipeDetail;
using DomusMind.Application.Features.MealPlanning.RemoveRecipeIngredient;
using DomusMind.Application.Features.MealPlanning.UpdateRecipe;
using DomusMind.Application.Features.MealPlanning.UpdateRecipeIngredient;
using DomusMind.Contracts.MealPlanning;
using DomusMind.Domain.MealPlanning.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Controllers;

[ApiController]
[Route("api/recipes")]
[Authorize]
public sealed class RecipesController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public RecipesController(ICurrentUser currentUser) => _currentUser = currentUser;

    /// <summary>Creates a new recipe for a family.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateRecipeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateRecipe(
        [FromBody] CreateRecipeRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new CreateRecipeCommand(
                    request.RecipeId, request.FamilyId, request.Name,
                    request.Description, request.PrepTimeMinutes, request.CookTimeMinutes,
                    request.Servings, request.IsFavorite,
                    request.AllowedMealTypes, request.Tags,
                    request.Ingredients,
                    _currentUser.UserId!.Value),
                cancellationToken);
            return Created($"/api/recipes/{response.Id}", response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Returns all recipes for a family, optionally filtered by meal type compatibility.</summary>
    [HttpGet("family/{familyId:guid}")]
    [ProducesResponseType(typeof(GetFamilyRecipesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFamilyRecipes(
        Guid familyId,
        [FromQuery] string? mealType,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            MealType? parsedMealType = null;
            if (!string.IsNullOrWhiteSpace(mealType) && Enum.TryParse<MealType>(mealType, ignoreCase: true, out var mt))
                parsedMealType = mt;

            var response = await dispatcher.Dispatch(
                new GetFamilyRecipesQuery(familyId, _currentUser.UserId!.Value, parsedMealType),
                cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
    }

    /// <summary>Returns the full detail of a single recipe including ingredients.</summary>
    [HttpGet("{recipeId:guid}")]
    [ProducesResponseType(typeof(GetRecipeDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRecipeDetail(
        Guid recipeId,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new GetRecipeDetailQuery(recipeId, _currentUser.UserId!.Value),
                cancellationToken);
            if (response is null) return NotFound();
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
    }

    /// <summary>Updates a recipe's metadata.</summary>
    [HttpPut("{recipeId:guid}")]
    [ProducesResponseType(typeof(UpdateRecipeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateRecipe(
        Guid recipeId,
        [FromBody] UpdateRecipeRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new UpdateRecipeCommand(
                    recipeId, request.Name, request.Description,
                    request.PrepTimeMinutes, request.CookTimeMinutes, request.Servings,
                    request.IsFavorite, request.AllowedMealTypes, request.Tags,
                    _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Deletes a recipe. Rejected if assigned to an active meal plan slot.</summary>
    [HttpDelete("{recipeId:guid}")]
    [ProducesResponseType(typeof(DeleteRecipeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteRecipe(
        Guid recipeId,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new DeleteRecipeCommand(recipeId, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Adds an ingredient to a recipe.</summary>
    [HttpPost("{recipeId:guid}/ingredients")]
    [ProducesResponseType(typeof(AddRecipeIngredientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddIngredient(
        Guid recipeId,
        [FromBody] AddRecipeIngredientRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new AddRecipeIngredientCommand(recipeId, request.Name, request.Quantity, request.Unit, _currentUser.UserId!.Value),
                cancellationToken);
            return Created($"/api/recipes/{recipeId}", response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Updates an ingredient's quantity and unit.</summary>
    [HttpPut("{recipeId:guid}/ingredients/{ingredientName}")]
    [ProducesResponseType(typeof(UpdateRecipeIngredientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateIngredient(
        Guid recipeId,
        string ingredientName,
        [FromBody] UpdateRecipeIngredientRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new UpdateRecipeIngredientCommand(recipeId, ingredientName, request.Quantity, request.Unit, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    /// <summary>Removes an ingredient from a recipe.</summary>
    [HttpDelete("{recipeId:guid}/ingredients/{ingredientName}")]
    [ProducesResponseType(typeof(RemoveRecipeIngredientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveIngredient(
        Guid recipeId,
        string ingredientName,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await dispatcher.Dispatch(
                new RemoveRecipeIngredientCommand(recipeId, ingredientName, _currentUser.UserId!.Value),
                cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
