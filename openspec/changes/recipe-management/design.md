## Context

`Recipe` is already a first-class `AggregateRoot<RecipeId>` in the `DomusMind.Domain.MealPlanning` namespace. It has its own identity, raises `RecipeCreated`, and enforces ingredient uniqueness. However, the lifecycle is incomplete: the domain has no `Update`, `Delete`, `RemoveIngredient`, or `UpdateIngredient` methods. At the application layer, only `CreateRecipe` and `GetFamilyRecipes` slices exist. In the web app, recipes are only reachable through the slot inspector inside the weekly meal plan grid — there is no standalone recipe library surface.

The proposal scope is:
1. Complete the `Recipe` aggregate mutation surface (update metadata, manage ingredients, delete with guard)
2. Add `GetRecipeDetail` for full single-recipe retrieval
3. Add `allowedMealTypes` filtering to `GetFamilyRecipes`
4. Add a standalone recipe library surface in the web app

## Goals / Non-Goals

**Goals:**
- `Recipe` aggregate has a complete mutation surface (create, update, delete, ingredient management)
- API exposes full CRUD for recipes plus ingredient sub-resource endpoints
- Web app has a dedicated recipe library page (list, view, create, edit, delete)
- `allowedMealTypes` filtering is applied in the recipe picker inside the slot inspector
- Spec-stated delete guard (active-slot reference check) is enforced

**Non-Goals:**
- Recipe search beyond name-based client-side filtering
- Recipe import/export
- Per-member ownership (recipes remain household-scoped)
- Nutrition data, photos, or step-by-step instructions
- Redefining meal plan or slot assignment behavior

## Decisions

### 1. Hard delete with active-slot guard (not soft delete)

The spec states a recipe "may not be deleted if currently referenced by any active meal plan slot." This is a guard condition, not a lifecycle state. A soft-delete column would bleed infrastructure concerns into domain semantics and require all queries to filter it. Instead:

- `Recipe` raises a `RecipeDeleted` domain event.
- The `DeleteRecipe` handler queries for active-plan slots referencing the recipe *before* calling `Delete()` on the aggregate. If any exist, it returns a domain error — the aggregate delete is never called.
- The record is physically removed from the database.

**Alternative considered**: soft delete with `IsDeleted` flag. Rejected because it complicates `GetFamilyRecipes`, template application, and future queries. The spec guard is enforced at the application layer, which is the right location.

### 2. Ingredients managed as a nested collection on the Recipe aggregate

Ingredients are value objects within `Recipe`. They are not a separate aggregate and do not have their own REST resource from a domain perspective, but the API exposes them via sub-resource routes for ergonomics:

```
POST   /api/recipes/{id}/ingredients
PUT    /api/recipes/{id}/ingredients/{name}
DELETE /api/recipes/{id}/ingredients/{name}
```

Ingredient identity within a recipe is the ingredient name (case-insensitive). There is no separate `IngredientId`. This matches the existing domain invariant and the spec requirement for deduplication in shopping list derivation.

**Domain methods added to `Recipe`**:
- `Update(name, description, prepTime, cookTime, servings, isFavorite, allowedMealTypes, tags)` — full metadata replace
- `AddIngredient(ingredient)` — already exists, unchanged
- `RemoveIngredient(name)` — removes by name, case-insensitive
- `UpdateIngredient(name, newQuantity, newUnit)` — finds by name, replaces quantity/unit
- `Delete()` — raises `RecipeDeleted`, marks aggregate for removal

### 3. `GetFamilyRecipes` extended with optional `mealType` filter

`GET /api/recipes/family/{familyId}?mealType=Breakfast` — when `mealType` is provided, returns only recipes where `AllowedMealTypes` is empty (no restriction) or contains the given meal type. This matches the spec scenario.

The `RecipePickerPanel` component passes the current slot's meal type as a query parameter when fetching or filtering. The existing client-side name search is preserved.

**Slice path**: `Features/MealPlanning/GetFamilyRecipes/` — extend `GetFamilyRecipesQuery` with `MealType?` parameter; update handler.

### 4. `GetRecipeDetail` returns full recipe including ingredient list

`GET /api/recipes/{id}` returns full recipe detail including the ingredient list. `GetFamilyRecipesResponse` continues to return `IngredientCount` (not full ingredients) for the list view — this is appropriate for a summary.

**New slice**: `Features/MealPlanning/GetRecipeDetail/`  
**New contract**: `GetRecipeDetailResponse` includes `IReadOnlyList<IngredientDetail>`.

### 5. Web app recipe library as a new feature folder

The recipe library does not belong inside `features/meal-planning` — it is a peer capability, not a sub-view of the weekly grid. A new `features/recipe-library` folder is introduced.

Route: `/recipes` (or `/meal-planning/recipes` — TBD during implementation, but structurally separate from the week view).

State is managed via a new Redux slice `recipeLibrarySlice` (or by extending `mealPlanningSlice` with recipe-library actions — prefer a separate slice to avoid growing `mealPlanningSlice` further).

The `CreateRecipeModal` in the slot inspector is kept but optionally routes to the full recipe form after creation. A new `RecipeFormPage` or `EditRecipeModal` is introduced in `recipe-library`.

## Risks / Trade-offs

- **Ingredient identity by name** → Any rename of an ingredient requires remove + add, which will change the ingredient list order. Mitigation: document this in the spec; no client-side reorder is promised.
- **Active-slot check at delete** → Performed as a projection query in the handler. This is a point-in-time check; a race condition exists if two users act concurrently (one deletes, one assigns). Mitigation: acceptable for a household-scoped product; database constraint or event ordering is out of scope here.
- **`mealType` filter change to `GetFamilyRecipes`** → Existing callers (slot inspector) pass no filter today and get all recipes — this is backward-compatible. New callers can pass the filter. No breaking change.
- **`CreateRecipeModal` scope** → The existing quick-add modal in the slot inspector cannot capture ingredients. After this change, a recipe created inline will have no ingredients until edited in the library surface. Shopping list derivation will produce no items from it. This is an acceptable known limitation; the UI should communicate it (e.g., "No ingredients — add them in the recipe library").
