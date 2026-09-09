# DomusMind - Meal Planning Context

Status: V2 (targeted — not part of V1 core)
Audience: Product / Engineering / Architecture
Scope: Household meal coordination and shopping integration
Depends on: Family, Lists, Responsibilities (soft), Calendar (projection)

---

## Purpose

The Meal Planning context defines the **household meal coordination layer**.

It is responsible for:

- planning weekly meals for the household
- organizing recipes and their ingredients
- identifying what needs to be purchased
- generating shopping lists for the Lists context
- enabling reusable weekly patterns via templates
- supporting fast reuse of previous weeks

This context answers:

- what meals are planned for the week?
- what are we cooking on Tuesday dinner?
- what do we need to buy for this week's meals?
- what is our usual weekly meal pattern?

---

## What Meal Planning Is Not

- A nutritional analysis tool
- A recipe social platform or database
- An individual diet management system
- An automatic task generator
- A replacement for the Shopping list or the Lists context

---

## Positioning Within the Domain

Meal Planning is a V2 bounded context.

It extends the DomusMind household operating model beyond the V1 core.

It depends on:

- **Family** — for household identity and scoping
- **Lists** — shopping lists derived from meal plans are created as `List` aggregates in the Lists context, not owned here
- **Responsibilities** — a meal plan may carry a soft reference to a responsibility domain (e.g. `food`) as optional area ownership; this has no behavioral effect
- **Calendar** — meal slots have temporal semantics; they project into the Agenda surface as a read concern

Meal Planning must not:

- own a `List` aggregate (this belongs to the Lists context)
- create Tasks automatically (explicit household action only)
- own time semantics (Calendar remains the source of truth for time)

---

## Bounded Context Boundary

The Meal Planning context owns:

- meal plan identity, structure, and lifecycle
- meal slot assignment within a plan
- the recipe library for the household
- weekly templates for reuse
- the derivation of ingredient sets for a plan week

The Meal Planning context does NOT own:

- the shopping list container (Lists context owns that)
- the purchase execution or tracking (Lists: checked items)
- task assignment or tracking (Tasks context)
- reminder delivery (Calendar or list temporal fields)
- dietary analysis or recommendation

---

## Aggregate Roots

### MealPlan

`MealPlan` is the primary aggregate root of this context.

It represents a **planned set of meals for a household over a defined week**.

MealPlan owns:

- plan identity
- week definition (start date: the household's configured first day of week)
- family association
- status (`Draft`, `Active`, `Completed`)
- collection of meal slots — one per day and meal type combination, materialized at plan creation
- optional template reference
- optional responsibility domain reference (soft, informational only)
- `shoppingListId` (optional reference, set after derivation)
- `shoppingListVersion` (integer, incremented on each derivation request)
- `lastDerivedAt` (timestamp of last shopping list derivation)
- `affectsWholeHousehold` flag (default: true)

**Invariants:**

- must belong to exactly one family
- week start must equal the household's configured first day of week
- week spans weekStart through weekStart + 6 days
- only one Active plan per family per week
- meal slots are materialized at plan creation covering all days × all meal types
- meal slots must not duplicate (same day + same meal type combination within a plan)
- only an Active plan can have meal slots mutated
- a Completed plan prevents all mutation

---

### Recipe

`Recipe` represents a household recipe — a named set of ingredients with optional preparation notes and planning metadata.

Recipe owns:

- recipe identity
- name (unique within family)
- description (optional)
- preparation time in minutes (optional)
- cook time in minutes (optional)
- total time in minutes (optional; equals prepTime + cookTime when both present)
- servings count (optional)
- collection of ingredients
- family association
- `allowedMealTypes` — the meal types this recipe is appropriate for (optional; absence means unrestricted)
- `tags` — free-form classification labels (optional)
- `isFavorite` flag (default: false)

**Invariants:**

- must belong to exactly one family
- name must be unique within a family
- ingredients must be non-empty if the recipe is used in a meal slot
- ingredient name must be unique within a recipe

---

### WeeklyTemplate

`WeeklyTemplate` represents a reusable weekly meal pattern.

It enables households to recreate a familiar meal structure without planning from scratch.

WeeklyTemplate owns:

- template identity
- name (unique within family)
- family association
- collection of meal slot templates (one per day and meal type combination)

**Invariants:**

- must belong to exactly one family
- name must be unique within a family

---

## Internal Entities

### MealSlot

Represents a single scheduled meal within a `MealPlan`.

A meal slot is defined by:

- day of week (first configured day through last day of the household week)
- meal type (Breakfast, MidMorningSnack, Lunch, AfternoonSnack, Dinner)
- `mealSourceType` — how the slot is populated:
  - `Recipe` — references a recipe from the library
  - `FreeText` — a free-text label (e.g., "pasta")
  - `External` — a meal outside the household (school lunch, restaurant)
  - `Leftovers` — designated as leftovers from a prior meal
  - `Unplanned` — explicitly left open
- `recipeId` (optional — valid only when `mealSourceType = Recipe`)
- `freeText` (optional — required when `mealSourceType = FreeText`)
- `notes` (optional)
- `isOptional` — marks the slot as non-binding (default: false; often true for weekend snacks)
- `isLocked` — marks the slot as stable and not to be changed (default: false; useful for weekday routines)
- `affectsWholeHousehold` — indicates whether the meal applies to the full household (default: true; inherited from plan unless overridden)

A `MealSlot` belongs to exactly one `MealPlan`.

Slots are materialized at plan creation for all days × all meal types. A slot without any assignment carries `mealSourceType = Unplanned`.

**Invariants:**

- `recipeId` must be null when `mealSourceType ≠ Recipe`
- `freeText` must be non-empty when `mealSourceType = FreeText`
- a locked slot may not be mutated unless explicitly unlocked first

### Ingredient

Represents a component of a `Recipe`.

An ingredient carries:

- name
- quantity (optional)
- unit (optional)

An `Ingredient` belongs to exactly one `Recipe`.  
Ingredient name must be unique within a recipe (for deduplication during shopping list derivation).

---

### MealSlotTemplate

Represents a slot within a `WeeklyTemplate`.

Mirrors the shape of `MealSlot` but belongs to a `WeeklyTemplate`, not a `MealPlan`.

Carries:

- day of week
- meal type
- `mealSourceType`
- `recipeId` (optional)
- `freeText` (optional)
- `notes` (optional)
- `isOptional`
- `isLocked`

---

## Meal Type Taxonomy

Meal types are ordered. Read models must respect this order:

1. Breakfast
2. MidMorningSnack
3. Lunch
4. AfternoonSnack
5. Dinner

---

## Plan Lifecycle

```
Draft → Active → Completed
```

- `Draft` — plan is being set up; slots can be populated; not yet treated as the household plan for the week
- `Active` — plan is the household's working plan for the week; only one active plan per family per week
- `Completed` — week has passed; plan is read-only

Transition rules:

- a plan is created in `Draft` status by default
- a plan is promoted to `Active` explicitly, or automatically when the week begins (TBD by product)
- `Completed` transition is future behavior; defined now to prevent modeling debt

---

## Domain Events

Meal Planning emits:

```
MealPlanCreated
MealPlanStatusChanged
MealSlotAssigned
MealSlotCleared
WeeklyTemplateCreated
WeeklyTemplateUpdated
WeeklyTemplateApplied
MealPlanCopiedFromPreviousWeek
RecipeCreated
RecipeUpdated
RecipeDeleted
ShoppingListRequested
```

Event notes:

- `ShoppingListRequested` — emitted after a household requests shopping list derivation from a meal plan. The Lists context reacts by creating a `List` of kind `shopping` prefilled with the consolidated ingredient set.
- `WeeklyTemplateApplied` — emitted after a template is applied to create a new `MealPlan`. Contains the resulting `mealPlanId`.
- `MealPlanCopiedFromPreviousWeek` — emitted after a plan is cloned from the previous week. Contains source and target `mealPlanId` values.

---

## Integration with Lists

Shopping list derivation follows an **event-driven integration** pattern.

When a shopping list is requested from a meal plan:

1. Meal Planning emits `ShoppingListRequested` carrying the consolidated ingredient list.
2. The Lists context reacts by creating a `List` of kind `shopping`.
3. Each ingredient becomes a `ListItem` with optional quantity.
4. Subsequent purchase tracking (checking off items) belongs entirely to the Lists context.

**Meal Planning does not own the derived list after creation.**  
The generated shopping list is a `List` from the moment it is created.  
Meal Planning retains a reference to the `shoppingListId` for traceability.

Derivation is **idempotent per `shoppingListVersion`**:

- re-requesting derivation increments `shoppingListVersion` and generates a new `List`
- the previous list is not deleted or mutated
- Meal Planning saves `lastDerivedAt` and the current `shoppingListId` reference (points to most recent)

---

## Integration with Tasks

Meal Planning does not automatically create tasks.

Explicit household action is required to create related tasks such as:

- "prepare Thursday dinner"
- "defrost chicken"
- "pick up groceries"

When tasks are desired, a household member explicitly creates them in the Tasks context, optionally referencing the meal plan for context.

No implicit task creation or automation occurs.

---

## Integration with Responsibilities

A `MealPlan` may carry a soft reference to a `responsibilityDomainId` (e.g., the `food` area).

This is optional and informational only. It has no behavioral effect within the Meal Planning context.

Meal Planning must not modify responsibility assignments.  
Responsibility context retains full ownership of accountability structure.

---

## Agenda Projection

`MealSlot` entries carry temporal information (day-of-week within a specific plan week).

Projection rules:

- meal slots are **derived temporal projections** — they are not Calendar Events
- projected meal slots appear in the Agenda surface as non-timed, household-scoped entries
- projected entries are read-only from the Agenda's perspective
- projected meal slots are editable only through the Meal Planning surface
- projected entries must be visually distinguishable from Calendar Events and Tasks
- slots with `mealSourceType = Unplanned` and no notes are not projected (nothing to show)

---

## Commands

Write intents (one per slice):

- `CreateMealPlan` — create a new weekly plan for the household; initializes full slot grid (all days × all meal types) in `Unplanned` state
- `UpdateMealSlot` — assign or clear a slot (by source type, recipe, free text, or marking as external/leftovers)
- `ApplyWeeklyTemplate` — create a MealPlan pre-populated from a template (slots copied as snapshot)
- `CopyMealPlanFromPreviousWeek` — create a MealPlan by cloning slot structure and assignments from the previous week's plan; does not carry over shopping list reference
- `RequestShoppingList` — trigger shopping list derivation from the current plan; idempotent per version
- `CreateRecipe` — add a recipe to the household library
- `UpdateRecipe` — modify an existing recipe
- `DeleteRecipe` — remove a recipe (only if not referenced by any active meal plan slot)
- `CreateWeeklyTemplate` — create a new reusable weekly template
- `UpdateWeeklyTemplate` — modify an existing template

---

## Queries

- `GetCurrentMealPlan` — retrieve the active or most recent meal plan for the household
- `GetMealPlanDetail` — retrieve the full plan with meal slots and assigned recipes
- `GetMealPlanForWeek` — retrieve the plan for a specific week
- `GetRecipeDetail` — retrieve a recipe with its ingredient list
- `GetFamilyRecipes` — list all recipes in the household library
- `GetWeeklyTemplateDetail` — retrieve a full template
- `GetFamilyWeeklyTemplates` — list all templates for the household

---

## Read Models

### MealPlanSummary

- planId
- weekStart
- weekEnd
- familyId
- status
- appliedTemplateId (optional)
- slotCount
- assignedSlotCount
- shoppingListId (optional)

### MealPlanDetail

- planId
- weekStart
- weekEnd
- familyId
- status
- slots (ordered by day from household's configured first day of week, then meal type Breakfast→Dinner)
  - dayOfWeek
  - mealType
  - mealSourceType
  - recipeName (optional)
  - freeText (optional)
  - notes (optional)
  - isOptional
  - isLocked
- appliedTemplateId (optional)
- shoppingListId (optional)
- shoppingListVersion
- lastDerivedAt (optional)

### RecipeSummary

- recipeId
- name
- prepTimeMinutes
- totalTimeMinutes
- servings
- ingredientCount
- tags
- isFavorite
- allowedMealTypes

### WeeklyTemplateSummary

- templateId
- name
- slotCount

---

## Context Scope Limits

The following are explicitly out of scope for the Meal Planning context:

- dietary constraints and nutritional analysis
- per-member dietary preferences
- automatic task generation
- recipe import from external sources (future integration only)
- cost tracking and budgeting
- inventory management (pantry state)

Some of these (pantry, inventory) may become part of future adjacent contexts in V3+.
