## 1. Domain — Recipe Aggregate Mutations

- [x] 1.1 Add `Update(name, description, prepTimeMinutes, cookTimeMinutes, servings, isFavorite, allowedMealTypes, tags)` method to `Recipe` aggregate; enforce name uniqueness at application layer
- [x] 1.2 Add `RemoveIngredient(name)` method to `Recipe` aggregate (case-insensitive match)
- [x] 1.3 Add `UpdateIngredient(name, newQuantity, newUnit)` method to `Recipe` aggregate
- [x] 1.4 Add `Delete()` method to `Recipe` aggregate that raises `RecipeDeleted` domain event

## 2. Backend — Contracts

- [x] 2.1 Add `GetRecipeDetailResponse` record with full ingredient list (`IReadOnlyList<IngredientDetail>`)
- [x] 2.2 Add `UpdateRecipeRequest` and `UpdateRecipeResponse` records
- [x] 2.3 Add `DeleteRecipeResponse` record
- [x] 2.4 Add `AddRecipeIngredientRequest` and `AddRecipeIngredientResponse` records
- [x] 2.5 Add `UpdateRecipeIngredientRequest` and `UpdateRecipeIngredientResponse` records
- [x] 2.6 Add `RemoveRecipeIngredientResponse` record
- [x] 2.7 Extend `GetFamilyRecipesQuery` with optional `MealType?` parameter; update `GetFamilyRecipesResponse` if needed

## 3. Backend — Application Slices

- [x] 3.1 Add `GetRecipeDetail` query slice (`GetRecipeDetailQuery`, `GetRecipeDetailQueryHandler`)
- [x] 3.2 Add `UpdateRecipe` command slice (`UpdateRecipeCommand`, `UpdateRecipeCommandHandler`, `UpdateRecipeValidator`)
- [x] 3.3 Add `DeleteRecipe` command slice (`DeleteRecipeCommand`, `DeleteRecipeCommandHandler`) — includes active-slot guard query
- [x] 3.4 Add `AddRecipeIngredient` command slice (`AddRecipeIngredientCommand`, `AddRecipeIngredientCommandHandler`)
- [x] 3.5 Add `UpdateRecipeIngredient` command slice (`UpdateRecipeIngredientCommand`, `UpdateRecipeIngredientCommandHandler`)
- [x] 3.6 Add `RemoveRecipeIngredient` command slice (`RemoveRecipeIngredientCommand`, `RemoveRecipeIngredientCommandHandler`)
- [x] 3.7 Update `GetFamilyRecipesQueryHandler` to filter by `MealType` when provided (empty `allowedMealTypes` OR contains the given meal type)

## 4. Backend — API

- [x] 4.1 Add `GET /api/recipes/{id}` endpoint to `RecipesController`
- [x] 4.2 Add `PUT /api/recipes/{id}` endpoint to `RecipesController`
- [x] 4.3 Add `DELETE /api/recipes/{id}` endpoint to `RecipesController`
- [x] 4.4 Add `POST /api/recipes/{id}/ingredients` endpoint to `RecipesController`
- [x] 4.5 Add `PUT /api/recipes/{id}/ingredients/{name}` endpoint to `RecipesController`
- [x] 4.6 Add `DELETE /api/recipes/{id}/ingredients/{name}` endpoint to `RecipesController`
- [x] 4.7 Add optional `mealType` query parameter to `GET /api/recipes/family/{familyId}`

## 5. Backend — Build and Tests

- [x] 5.1 Run `dotnet build` — verify no compilation errors
- [x] 5.2 Add unit tests for `Recipe.Update` covering name-change and no-op cases
- [x] 5.3 Add unit tests for `Recipe.RemoveIngredient` and `Recipe.UpdateIngredient`
- [x] 5.4 Add unit tests for `DeleteRecipe` handler — active-slot guard blocks deletion, inactive slots do not
- [x] 5.5 Add unit tests for `GetFamilyRecipes` with `mealType` filter
- [x] 5.6 Run `dotnet test` — all tests pass

## 6. Frontend — API Client and State

- [x] 6.1 Add `getRecipeDetail(recipeId)` API call in the meal planning API module
- [x] 6.2 Add `updateRecipe(...)` API call
- [x] 6.3 Add `deleteRecipe(recipeId)` API call
- [x] 6.4 Add `addIngredient(recipeId, ingredient)` API call
- [x] 6.5 Add `updateIngredient(recipeId, name, quantity, unit)` API call
- [x] 6.6 Add `removeIngredient(recipeId, name)` API call
- [x] 6.7 Create `recipeLibrarySlice` with state for recipe list, selected recipe detail, and mutation statuses
- [x] 6.8 Update `GetFamilyRecipes` call in the slot inspector to pass `mealType` when a slot is selected

## 7. Frontend — Recipe Library Surface

- [x] 7.1 Create `src/features/recipe-library/` feature folder
- [x] 7.2 Create `RecipesPage` component — recipe list with name, cook time, servings, ingredient count; "New Recipe" button
- [x] 7.3 Create `RecipeDetailPanel` component — full recipe view including ingredient list; edit and delete actions
- [x] 7.4 Create `RecipeFormModal` (or page) — full recipe creation and edit form; includes ingredients, tags, `allowedMealTypes`, `isFavorite`
- [x] 7.5 Wire delete action with confirmation and active-slot error handling (show user-friendly rejection message)
- [x] 7.6 Register `RecipesPage` route in the app router
- [x] 7.7 Add navigation entry for the recipe library surface

## 8. Frontend — Slot Inspector Update

- [x] 8.1 Update `RecipePickerPanel` to pass current slot's `mealType` to the recipes fetch; confirm filtered results display correctly
- [x] 8.2 Add inline note in `CreateRecipeModal` indicating ingredients can be added via the recipe library

## 9. Frontend — Build

- [x] 9.1 Run `npm run build` — verify no TypeScript or build errors
