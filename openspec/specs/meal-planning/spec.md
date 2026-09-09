# Meal Planning Specification

## Purpose

Meal Planning is the household's weekly food coordination capability. It answers what meals are planned for each day and meal type, what the household usually eats, and what needs to be purchased to cook those meals.

A meal plan represents the household's intended meals for one calendar week. Recipes form the household's reusable ingredient library. Weekly templates capture recurring patterns for fast reuse. Shopping lists are derived from assigned recipes and created in the Lists context.

Meal Planning is scoped to the household — not to individual members — and is optimized for low-effort reuse over from-scratch planning.

---

## Requirements

### Requirement: Meal Plan Creation

A household SHALL be able to create a meal plan for any given calendar week.

A meal plan is always scoped to one family and one week. The week is defined by its start date, which must align with the household's configured first day of week and spans exactly seven days.

On creation, the full slot grid is materialized: one slot per day of week × per meal type (Breakfast, MidMorningSnack, Lunch, AfternoonSnack, Dinner), totalling 35 slots. All slots start as Unplanned.

A new meal plan is created in Draft status.

Only one Active plan may exist per family per week.

#### Scenario: Household creates a meal plan for the current week

- GIVEN a family exists with a configured first day of week
- WHEN the household creates a meal plan for a valid week start date
- THEN a meal plan is created in Draft status for that family and week
- AND all 35 slots (7 days × 5 meal types) are initialized as Unplanned

#### Scenario: A meal plan already exists for the target week

- GIVEN a meal plan already exists for the family and week
- WHEN the household attempts to create another plan for the same week
- THEN the existing plan is returned
- AND no duplicate plan is created

---

### Requirement: Meal Plan Lifecycle

A meal plan progresses through a defined lifecycle: Draft → Active → Completed.

A Draft plan is being set up. It is not yet the household's working plan for the week. An Active plan is the household's working plan for the week. A Completed plan is read-only and represents a past week.

Transition to Active must occur before the plan is treated as the household's working plan. Transition to Completed prevents all mutation.

#### Scenario: Plan is promoted from Draft to Active

- GIVEN a meal plan exists in Draft status
- WHEN the household promotes it to Active
- THEN the plan becomes the Active plan for that family and week
- AND no other Active plan may exist for the same family and week

---

### Requirement: Meal Slot Assignment

A household SHALL be able to assign or clear any slot in a meal plan.

A slot is identified by its day of week and meal type. Each slot carries a source type:

- **Recipe** — references a recipe from the household library
- **FreeText** — a free-text label (e.g., "pasta night")
- **External** — a meal outside the household (school lunch, restaurant)
- **Leftovers** — designated as leftovers from a prior meal
- **Unplanned** — explicitly left open

Each slot may also carry optional notes, an `isOptional` flag (non-binding meal, common for snacks), and an `isLocked` flag (slot is stable and should not change).

Slot structure is fixed at plan creation. This operation updates content only, not structure.

#### Scenario: Household assigns a recipe to a slot

- GIVEN a meal plan in Draft or Active status exists
- AND a recipe from the household library is available
- WHEN the household assigns the recipe to a specific day and meal type
- THEN the slot's source type is set to Recipe with the recipe reference
- AND the slot is updated within the plan

#### Scenario: Household clears a slot

- GIVEN a meal plan with a populated slot
- WHEN the household clears that slot
- THEN the slot's source type is set to Unplanned
- AND any previous recipe reference or free text is removed
- AND the slot remains in the grid

#### Scenario: Household attempts to mutate a locked slot

- GIVEN a slot with `isLocked = true`
- WHEN the household submits a content change without unlocking the slot
- THEN the update is rejected
- AND the slot content is unchanged

#### Scenario: Household unlocks and mutates a locked slot

- GIVEN a slot with `isLocked = true`
- WHEN the household submits a content change that includes `isLocked = false`
- THEN the slot is unlocked and the content change is applied in the same operation

#### Scenario: Household attempts to mutate a Completed plan

- GIVEN a meal plan in Completed status
- WHEN the household attempts to update any slot
- THEN the update is rejected
- AND the plan remains unchanged

---

### Requirement: Meal Plan Viewing

A household SHALL be able to retrieve the full detail of a meal plan for any week.

The response includes all 35 slots in day-then-meal-type order, including Unplanned slots. For Recipe slots, recipe metadata is included inline.

The plan may be retrieved by plan identifier or by family + week start.

#### Scenario: Household views the current week's meal plan

- GIVEN an Active meal plan exists for the current week
- WHEN the household retrieves it by family and week start
- THEN all 35 slots are returned ordered by day (from the household's first day of week) then by meal type
- AND Unplanned slots are included in the response

#### Scenario: Household views a plan with recipe slots

- GIVEN a meal plan with one or more Recipe-type slots
- WHEN the household retrieves the plan
- THEN each Recipe slot includes the recipe name, servings, and time metadata inline

---

### Requirement: Recipe Library

A household SHALL maintain a library of recipes scoped to the family.

A recipe has a name (unique within the family), optional description, optional preparation and cook times, optional servings count, an ingredient list, optional tags for classification, an optional set of allowed meal types indicating which meal types the recipe is appropriate for, and an `isFavorite` flag.

Ingredients within a recipe must have unique names (to support deduplication during shopping list derivation). Ingredient quantity and unit are optional. When both preparation and cook times are provided, total time is derived as their sum; if either is absent, total time is unset.

A recipe may not be deleted if it is currently referenced by any active meal plan slot.

Recipes may be updated after creation; ingredients can be added or modified.

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

---

### Requirement: Weekly Templates

A household SHALL be able to create named, reusable weekly meal patterns.

A weekly template captures a set of slot assignments (day + meal type combinations) with the same source type and metadata as meal slots. Template names must be unique within the family. Templates may have fewer than 35 slots defined; unrepresented slots default to Unplanned when the template is applied.

Templates may be updated after creation; slot assignments can be added or modified.

#### Scenario: Household creates a weekly template

- GIVEN a family exists
- WHEN the household creates a template with a unique name and optional slot assignments
- THEN the template is saved to the family's template library
- AND it becomes available to apply to future weeks

#### Scenario: Household attempts to create a template with a duplicate name

- GIVEN a template named "Standard week" already exists
- WHEN the household creates another template with the same name
- THEN the creation is rejected

---

### Requirement: Apply Weekly Template

A household SHALL be able to create a new meal plan for a target week pre-populated from an existing weekly template.

Template application creates a new meal plan with slots copied from the template. Slots not represented in the template default to Unplanned. The plan records which template was applied. Template application is a snapshot — subsequent changes to the template do not affect already-created plans.

Recipe references within the template must be valid at the time of application. If a recipe referenced by the template has been deleted, the application fails.

If a plan already exists for the target week, the existing plan is returned and the template is not re-applied.

After a template is applied, individual slots may still be modified.

#### Scenario: Household applies a template to an empty week

- GIVEN a weekly template exists and no plan exists for the target week
- WHEN the household applies the template to a target week
- THEN a meal plan is created in Draft status with slots populated from the template
- AND slots not covered by the template are Unplanned
- AND the plan records a reference to the applied template

#### Scenario: Template references a deleted recipe

- GIVEN a weekly template with a slot that references a recipe
- AND that recipe has since been deleted from the household library
- WHEN the household applies the template to a target week
- THEN the application fails
- AND no meal plan is created

#### Scenario: A plan already exists for the target week

- GIVEN a meal plan already exists for the target family and week
- WHEN the household applies a template to the same week
- THEN the existing plan is returned
- AND the template is not re-applied

---

### Requirement: Copy from Previous Week

A household SHALL be able to create a new meal plan for a target week by cloning the slot assignments from a previous week's plan.

The source is either an explicitly provided plan or the plan for the immediately preceding week. The copy carries over slot source types, recipe references, free text, notes, and flags. Shopping list references are never transferred — derivation starts fresh for the new week.

If no source plan exists for the preceding week, the operation returns a recoverable outcome (not a hard failure) and the household can proceed to create a plan from scratch or apply a template. If a plan already exists for the target week, the existing plan is returned.

This operation is distinct from applying a template. Templates are named, reusable patterns; copy-from-week is ad-hoc reuse.

#### Scenario: Household copies the previous week's plan

- GIVEN a meal plan exists for the preceding week
- AND no plan exists for the target week
- WHEN the household copies the previous week
- THEN a new meal plan is created in Draft status for the target week
- AND all slot assignments from the source plan are copied
- AND shopping list references from the source plan are not carried over

#### Scenario: No source plan exists for the preceding week

- GIVEN no meal plan exists for the preceding week
- WHEN the household attempts to copy the previous week
- THEN the operation returns a recoverable "no previous plan" outcome
- AND the household can create a plan from scratch or apply a template

---

### Requirement: Shopping List Derivation

A household SHALL be able to derive a shopping list from a meal plan's recipe assignments.

Derivation consolidates all ingredients from all Recipe-type slots in the plan. Ingredients are deduplicated by name (case-insensitive) within the same unit. Ingredients with differing units are kept as separate items.

Each derivation request creates a new shopping list in the Lists context. Previous shopping lists derived from the same plan are not mutated. The meal plan records a reference to the most recently derived shopping list and increments a version counter on each derivation.

The resulting shopping list is a first-class List in the Lists context. Meal Planning does not own it after creation and does not receive feedback when items are checked off.

At least one Recipe-type slot must be assigned in the plan for derivation to proceed.

#### Scenario: Household requests a shopping list from an active plan

- GIVEN a meal plan with at least one Recipe-type slot
- AND the recipe has at least one ingredient
- WHEN the household requests a shopping list
- THEN a shopping list is created in the Lists context with one item per consolidated ingredient
- AND the meal plan records the shopping list reference and increments its derivation version

#### Scenario: Household re-requests a shopping list

- GIVEN a meal plan that already has a shopping list reference
- WHEN the household requests a new shopping list
- THEN a new shopping list is created
- AND the previous shopping list is not modified
- AND the meal plan's derivation version increments

#### Scenario: Plan has no recipe slots assigned

- GIVEN a meal plan with no Recipe-type slots
- WHEN the household requests a shopping list
- THEN the request is rejected
- AND no shopping list is created

---

### Requirement: Agenda Projection

Meal slots project into the Agenda surface as non-timed household-level entries.

Slots with `mealSourceType = Unplanned` and no notes are not projected. Projected slots are read-only in the Agenda — editing occurs only in the Meal Planning surface.

#### Scenario: Assigned meal slot appears in the Agenda

- GIVEN a meal plan with a Recipe-type slot on a specific day
- WHEN the household views the Agenda for that day
- THEN the meal slot appears as a non-timed household entry
- AND it is visually distinct from Calendar Events and Tasks

#### Scenario: Unplanned slot without notes is excluded from Agenda

- GIVEN a slot with `mealSourceType = Unplanned` and no notes
- WHEN the household views the Agenda for that day
- THEN the slot does not appear

---

## Notes

### Ambiguity: Active plan promotion timing

The context document states a plan may be promoted to Active "explicitly, or automatically when the week begins." The automatic promotion rule is marked as TBD. The current spec treats promotion as explicit only.

### Resolved: Mutation on Draft vs. Active plans

Both Draft and Active plans allow slot mutation. Only Completed plans prevent mutation. The source `update-meal-slot` feature spec contained a contradictory precondition; the intent documented in the same file ("Draft plans allow mutation; Completed plans do not") is treated as the authoritative statement.

### Future behavior: Completed transition

The `Completed` status is defined in the domain model to prevent modeling debt but its transition trigger is not yet specified. It is included in the spec as a lifecycle state; scenarios for Completed transition are intentionally absent.

### Partial specification: Shopping list unit consolidation

The consolidation rule for mismatched units (e.g., "500g flour" and "2 cups flour") results in separate list items. Consolidated quantity calculation when units match is referenced but not fully specified.

---

## Source References

- `docs/04_contexts/meal-planning.md`
- `00_product/surfaces/meal-planning.md`
- `specs/features/meal-planning/create-meal-plan.md`
- `specs/features/meal-planning/view-meal-plan.md`
- `specs/features/meal-planning/update-meal-slot.md`
- `specs/features/meal-planning/create-recipe.md`
- `specs/features/meal-planning/create-weekly-template.md`
- `specs/features/meal-planning/apply-weekly-template.md`
- `specs/features/meal-planning/copy-meal-plan-from-previous-week.md`
- `specs/features/meal-planning/request-shopping-list.md`
