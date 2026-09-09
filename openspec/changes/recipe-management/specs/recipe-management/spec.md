## ADDED Requirements

### Requirement: Recipe Detail Retrieval

A household SHALL be able to retrieve the full detail of a single recipe, including its complete ingredient list.

#### Scenario: Household retrieves a recipe by ID

- GIVEN a recipe exists in the household's library
- WHEN the household retrieves it by recipe ID
- THEN the full recipe is returned including name, description, times, servings, tags, allowedMealTypes, isFavorite, and the complete ingredient list
- AND each ingredient includes name, optional quantity, and optional unit

#### Scenario: Recipe not found

- GIVEN no recipe exists with the requested ID in the household's library
- WHEN the household attempts to retrieve it
- THEN the request returns a not-found outcome

---

### Requirement: Recipe Update

A household SHALL be able to update a recipe's metadata after creation.

Updatable fields: name, description, preparation time, cook time, servings, isFavorite, allowedMealTypes, and tags. Name must remain unique within the family. All fields are replaced in full (no partial patch).

#### Scenario: Household updates a recipe

- GIVEN a recipe exists in the household's library
- WHEN the household submits updated metadata
- THEN the recipe is updated with the new values
- AND the recipe's updatedAt timestamp is refreshed

#### Scenario: Household attempts to rename a recipe to a conflicting name

- GIVEN a recipe named "Pasta Bolognese" already exists in the family's library
- AND the household attempts to rename a different recipe to "Pasta Bolognese"
- THEN the update is rejected
- AND neither recipe is changed

---

### Requirement: Recipe Deletion

A household SHALL be able to delete a recipe from the library, subject to a referential guard.

A recipe SHALL NOT be deleted if it is currently referenced by any slot in an Active meal plan. Draft or Completed plans do not block deletion.

#### Scenario: Household deletes an unreferenced recipe

- GIVEN a recipe exists and is not referenced by any Active plan slot
- WHEN the household deletes it
- THEN the recipe is removed from the library
- AND it is no longer available for assignment to meal slots

#### Scenario: Household attempts to delete a recipe referenced by an Active plan

- GIVEN a recipe is referenced by at least one slot in an Active meal plan
- WHEN the household attempts to delete it
- THEN the deletion is rejected
- AND the recipe remains in the library

#### Scenario: Household deletes a recipe referenced only by a Draft or Completed plan

- GIVEN a recipe is referenced by a slot in a Draft or Completed meal plan only (no Active plans)
- WHEN the household deletes it
- THEN the recipe is removed from the library
- AND affected Draft or Completed slots retain their reference but the recipe metadata is no longer resolvable

---

### Requirement: Recipe Ingredient Management

A household SHALL be able to add, update, and remove individual ingredients from a recipe after it has been created.

Ingredient identity within a recipe is the ingredient name (case-insensitive). A recipe may not contain two ingredients with the same name.

#### Scenario: Household adds an ingredient to an existing recipe

- GIVEN a recipe exists
- WHEN the household adds a new ingredient with a unique name
- THEN the ingredient is added to the recipe
- AND the recipe's ingredient count increases by one

#### Scenario: Household attempts to add a duplicate ingredient

- GIVEN a recipe already contains an ingredient named "Olive oil"
- WHEN the household attempts to add another ingredient named "olive oil"
- THEN the operation is rejected
- AND the recipe is unchanged

#### Scenario: Household updates an ingredient's quantity or unit

- GIVEN a recipe contains an ingredient named "Flour"
- WHEN the household updates its quantity and unit
- THEN the ingredient is updated
- AND the ingredient name is unchanged

#### Scenario: Household removes an ingredient from a recipe

- GIVEN a recipe contains an ingredient
- WHEN the household removes it by name
- THEN the ingredient is removed from the recipe
- AND the recipe's ingredient count decreases by one

---

### Requirement: Recipe Library Surface

The web application SHALL provide a dedicated recipe library surface where a household can browse, view, create, edit, and delete recipes independently of the weekly meal plan view.

#### Scenario: Household browses the recipe library

- GIVEN one or more recipes exist in the household's library
- WHEN the household navigates to the recipe library surface
- THEN all recipes are listed with their name, cook time, servings, tag count, and ingredient count
- AND the list is ordered by name

#### Scenario: Household views a recipe's full detail

- GIVEN a recipe exists in the library
- WHEN the household selects it from the list
- THEN the full recipe detail is displayed including the complete ingredient list

#### Scenario: Household creates a recipe from the library surface

- GIVEN the household is on the recipe library surface
- WHEN the household submits a new recipe with a name and optional fields including ingredients
- THEN the recipe is created and appears in the library

#### Scenario: Household edits a recipe from the library surface

- GIVEN a recipe exists in the library
- WHEN the household edits it and saves
- THEN the recipe reflects the updated values

#### Scenario: Household deletes a recipe from the library surface

- GIVEN a recipe exists and is not blocked by deletion guard
- WHEN the household deletes it from the library surface
- THEN it is removed and no longer appears in the list
