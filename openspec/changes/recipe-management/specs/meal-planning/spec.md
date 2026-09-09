## MODIFIED Requirements

### Requirement: Recipe Library

A household SHALL maintain a library of recipes scoped to the family.

A recipe has a name (unique within the family), optional description, optional preparation and cook times, optional servings count, an ingredient list, optional tags for classification, an optional set of allowed meal types indicating which meal types the recipe is appropriate for, and an `isFavorite` flag.

Ingredients within a recipe must have unique names (to support deduplication during shopping list derivation). Ingredient quantity and unit are optional. When both preparation and cook times are provided, total time is derived as their sum; if either is absent, total time is unset.

A recipe may not be deleted if it is currently referenced by any slot in an Active meal plan. Draft and Completed plans do not block deletion.

Recipes may be updated after creation; ingredients can be added, updated, or removed individually after creation.

When browsing recipes for assignment to a slot, the household SHALL only be shown recipes that are compatible with the slot's meal type. A recipe is compatible when its `allowedMealTypes` list is empty (no restriction) or contains the target meal type.

#### Scenario: Household adds a recipe to the library

- GIVEN a family exists
- WHEN the household creates a recipe with a unique name
- THEN the recipe is added to the household's recipe library
- AND it becomes available for assignment to meal slots

#### Scenario: Household attempts to create a recipe with a duplicate name

- GIVEN a recipe named "Pasta Bolognese" already exists in the family's library
- WHEN the household creates another recipe with the same name
- THEN the creation is rejected
- AND the existing recipe is unchanged

#### Scenario: Recipe is assigned to allowed meal types

- GIVEN a recipe with `allowedMealTypes = [Dinner]`
- WHEN the household browses recipes for a Breakfast slot
- THEN the recipe is not presented as a valid option for that slot

#### Scenario: Recipe with no allowedMealTypes restriction is shown for any slot

- GIVEN a recipe with an empty `allowedMealTypes` list
- WHEN the household browses recipes for any meal type slot
- THEN the recipe is presented as a valid option
