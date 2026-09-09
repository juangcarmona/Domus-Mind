# Surface Spec — Meal Planning

Canonical upstream: docs/04_contexts/meal-planning.md
Depends on: docs/00_product/experience.md, docs/00_product/surface-system.md

---

## Purpose

Provide the household's **weekly food coordination surface**.

This surface answers:

- what are we eating this week?
- what is already decided?
- what is still missing?
- what should we reuse from last week or a template?
- what do we need to buy?

---

## Role

Meal Planning is:

- **weekly-first** — the week is the primary unit of structure
- **structure-first** — the full week grid is always visible, even when empty
- **household-level** — meals are scoped to the household, not to individual members
- **low-variability optimized** — reuse dominates creation

It is not:

- a recipe browser
- a nutrition tool
- a calendar replacement
- a task surface

---

## Shell

Standard surface shell.

Desktop:

- left navigation
- header with week controls and surface-level actions
- main week grid
- inspector panel (right)

Mobile:

- stacked day-by-day grid
- bottom sheet for detail

---

## Default State

- current week
- household scope
- full slot grid visible
- no filtering

---

## Core Layout

### Weekly Grid

Structure:

- columns: days (Monday – Sunday)
- rows: meal types in order:
  1. Breakfast
  2. MidMorningSnack
  3. Lunch
  4. AfternoonSnack
  5. Dinner

Each cell = one `MealSlot`.

---

### Cell Content

Each cell shows:

- meal label (recipe name, free text, or source indicator)
- source indicator:
  - recipe (recipe name)
  - free text (literal label)
  - external (e.g., "School lunch", "Restaurant")
  - leftovers
- optional note indicator (subtle)
- empty affordance when `mealSourceType = Unplanned` (subtle, actionable)

Cells must be dense. No verbose empty state labels.

---

### Current Day

- full column receives a visual highlight
- day header uses stronger emphasis

---

## Interaction

### Selection

Clicking a cell opens the inspector for that slot.

---

### Quick Actions (inline, per cell)

Available without opening inspector:

- assign recipe
- set as free text (quick meal)
- mark as external
- mark as leftovers
- copy from previous day
- clear (reset to Unplanned)

No modal required for basic assignment.

---

### Inspector

Opens on cell selection.

Sections:

1. **Slot identity** — day + meal type
2. **Meal source** — toggle between:
   - recipe selector (search or browse household library)
   - free text input
   - external marker (school, restaurant, etc.)
   - leftovers
3. **Notes** — free text
4. **Flags**:
   - mark as optional
   - lock slot
5. **Actions**:
   - clear slot
   - copy slot
   - close

---

## Header

Contains:

- week navigator (previous / current / next)
- "Apply template" action
- "Copy previous week" action
- "Generate shopping list" action

---

## Relationship with Agenda

Meal slots project into the Agenda surface as:

- non-timed entries
- household scope only
- visually distinct from Calendar Events and Tasks

Read-only in the Agenda. Editable only via this surface.

Slots with `mealSourceType = Unplanned` and no notes are not projected.

---

## Relationship with Lists

After "Generate shopping list" is triggered:

- a `List` of kind `shopping` is created in the Lists context
- it appears in the Lists surface
- no inline editing of the shopping list occurs here

---

## UX Rules

- the grid must be dense and scannable
- empty state must be actionable (not decorative)
- reuse must be faster than creation
- inspector is the only surface for deep editing
- no modal-first flows
- weekend slots may appear visually lighter to communicate flexibility

---

## Anti-Patterns

Do not:

- treat meals as calendar events
- require a recipe for every slot
- force full-week planning before saving
- introduce per-member meal assignment
- add nutritional complexity or scoring
- use "No recipe" as a visible slot label

---

## Surface State Model

The Meal Planning surface operates in distinct visual states. For each state, the following is defined: what remains visible, what message appears, and which actions stay enabled.

### 1 — Week with existing plan loaded

- WeekNavigator visible
- Toolbar visible: Apply Template (disabled, "coming soon" tooltip), Generate Shopping List
- WeekGrid visible with all 35 slots
- Inspector opens on slot click
- Shopping List: button shows when no list yet; "Shopping list ready" label shows after derivation
- Actions enabled: Update Slot, Generate Shopping List (when recipe slots exist and no list yet)

### 2 — Week with no plan yet

- WeekNavigator visible
- Compact empty-state action row below header (not a full-page island):
  - "Start from scratch" button (primary)
  - "Copy from previous week" button (disabled while loading)
  - "Apply template" button (disabled, "coming soon" tooltip)
- No grid visible until a plan exists
- Actions enabled: Start from scratch, Copy from previous week

### 3 — Action in progress

- Triggering button shows loading state or is disabled
- Remainder of page stays intact
- No spinner overlay covering the surface

### 4 — Recoverable action failure

Triggered by: "no previous plan found" for copy, or network failure on create/copy action.

- Page stays in its current state (empty-week or plan-loaded)
- Compact inline notice appears near the action row:
  - "No meal plan found for the previous week." (for NoPreviousPlan)
  - Generic failure text for network errors
- All other actions remain enabled
- Notice clears on next navigation

### 5 — Domain conflict on action (plan already exists)

Triggered by: copy or apply-template when the target week already has a plan.

- Existing plan is loaded and shown in the grid (no overlay, no blocking state)
- Compact inline notice: "This week already has a meal plan."
- Grid remains fully interactive
- Notice clears on next navigation

### 6 — Template unavailable

- "Apply template" button is always visible
- Disabled with `title="Templates coming soon"`
- No hidden/visible toggling based on template count

### 7 — Shopping list already created

- "Shopping list ready" text label replaces the generate button
- No re-generate action exposed (creating a second list is not supported here)
- The generated list lives in the Lists context

---

## Success Criteria


- a full week can be planned in under 2 minutes
- reuse (template or previous week) dominates from-scratch planning
- shopping list derivation is one action
- weekends feel flexible, not forced
- the grid is scannable in seconds
