## Why

Recipe is a first-class aggregate in the Meal Planning context, but its lifecycle is incomplete. Creation exists; update, delete, and ingredient editing do not. The only surface entry point is the slot inspector inside the weekly meal plan — there is no way to browse, view, or manage the recipe library independently. Several spec-stated requirements (recipe update, delete guard, `allowedMealTypes` filtering) are unimplemented. Completing the recipe lifecycle unblocks shopping list accuracy, reuse through templates, and a coherent recipe library experience.

## What Changes

- Add `UpdateRecipe` capability: name, description, times, servings, tags, `allowedMealTypes`, `isFavorite`
- Add `DeleteRecipe` capability: guarded by active-plan-slot check
- Add `GetRecipeDetail` capability: returns full recipe including ingredients
- Add `AddIngredient` / `RemoveIngredient` / `UpdateIngredient` capabilities on a recipe (post-create mutation)
- Enforce `allowedMealTypes` filtering in `GetFamilyRecipes` when a slot context is provided
- Complete the `CreateRecipe` UI form: expose ingredients, tags, `allowedMealTypes`, `isFavorite`
- Add a standalone recipe library surface in the web app (browse, view, create, edit, delete)

Out of scope for this change:
- Redefining the Meal Plan or slot assignment model
- Recipe search beyond name-based filtering
- Recipe import/export
- Per-member recipe ownership (recipes are always household-scoped)

## Capabilities

### New Capabilities

- `recipe-management`: Full lifecycle management of a household recipe — update, delete (with guard), detail retrieval, and ingredient post-create mutation. Includes the web app recipe library surface.

### Modified Capabilities

- `meal-planning`: Add `allowedMealTypes` filtering to `GetFamilyRecipes` when a slot context is provided. No other meal plan behavior changes.

## Impact

**Backend**
- `DomusMind.Domain`: Add `Update`, `Delete` (tombstone or removal), `RemoveIngredient`, `UpdateIngredient` methods to `Recipe` aggregate
- `DomusMind.Application`: New slices — `UpdateRecipe`, `DeleteRecipe`, `GetRecipeDetail`, `AddRecipeIngredient`, `RemoveRecipeIngredient`, `UpdateRecipeIngredient`; update `GetFamilyRecipes` to accept optional `mealType` filter
- `DomusMind.Contracts`: New request/response records for update, delete, detail
- `DomusMind.Api`: New endpoints on `RecipesController` — `GET /api/recipes/{id}`, `PUT /api/recipes/{id}`, `DELETE /api/recipes/{id}`, ingredient sub-resource endpoints
- EF migration required if soft-delete or new column is added

**Frontend**
- New `recipe-library` feature folder
- `RecipesPage` — standalone library surface (list + filter + create)
- `RecipeDetailPanel` or page — full recipe view with ingredients
- `EditRecipeModal` or page — full edit form including ingredients
- Update `CreateRecipeModal` to expose remaining fields (ingredients, tags, `allowedMealTypes`, `isFavorite`) or route to the new full form
- Update `RecipePickerPanel` to filter by `allowedMealTypes` when slot meal type is known
