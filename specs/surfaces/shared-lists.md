# Surface Spec - Shared Lists

## Purpose

Provide a fast, dense, reusable household list surface for shared checklist state.

This surface answers:

- what lists exist
- which list matters now
- what remains unchecked
- what should be added, checked, or updated next

Lists represent persistent shared household state.

They are:

- reusable
- toggle-based
- shared
- unscheduled by default

They are not:

- a task board
- a calendar
- a reminder system

---

## Depends On

- `docs/00_product/experience.md`
- `docs/00_product/surface-system.md`
- `docs/04_contexts/shared-lists.md`

---

## Entry Points

- main navigation → `Lists`
- contextual link from Today or Planning when a list is relevant
- deep link to a specific list

---

## Role

Shared Lists is the household surface for reusable checklists.

It is optimized for:

- scan speed
- quick capture
- quick toggle state
- low-friction inspection
- low-friction switching between lists

The active list is the hero.

---

## Core Principles

- content over chrome
- one row = one list item
- quick add always visible
- detail is secondary
- counts are visible before opening a list
- completed items stay accessible but compressed
- theming supports recognition, not decoration
- density is high without feeling hostile

---

## Surface Structure

Desktop default:

1. app navigation rail
2. list switcher pane
3. active list pane
4. optional inspector

Mobile default:

1. active list
2. list switcher via drawer/sheet
3. item detail via bottom sheet

This surface must use the shared product shell.

---

## Layout Zones

### 1. List Switcher

Shows available lists for the household.

Each row should show:

- list icon or color marker
- list name
- unchecked count
- optional area or linked-plan cue

The switcher must be compact and scannable.

### 2. Active List Header

Shows:

- list name
- unchecked count
- optional area chip
- optional linked plan chip
- search when needed
- compact overflow actions

The header must stay compact.
No oversized banner.

### 3. Active List Body

Shows the items of the selected list.

Default structure:

- unchecked items
- completed items collapsed behind a compact section

The body must prioritize fast scan and toggle.

### 4. Quick Add Bar

Always visible in the current working context.

Used to add a new item directly into the active list.

### 5. Optional Inspector

Used for secondary item detail without leaving the list.

Typical inspector content:

- item title
- note
- quantity
- order context
- optional links or metadata

---

## List Switcher Behavior

The switcher is the index of household lists.

It should answer immediately:

- which lists exist
- which have remaining work
- which list is active

Rules:

- active list is visually clear
- unchecked count is always visible
- long list names truncate cleanly
- empty lists remain visible
- switching lists preserves shell context

Do not use card previews.

---

## Active List Behavior

The selected list fills the main working area.

The list should feel like a working surface, not a showcase page.

Rules:

- unchecked items appear first
- completed items are de-emphasized
- row density stays high
- scrolling is smooth and uninterrupted
- selection does not navigate away by default

---

## Row Model

Each item row represents one shared list item.

Default row content:

- toggle control
- item title
- optional secondary metadata
- optional quick affordance

Examples of secondary metadata:

- quantity
- short note
- small contextual hint

Rules:

- one row = one item
- row height stays compact
- second line only when useful
- metadata never dominates the title
- row tap opens detail
- toggle is immediate

Avoid card-per-item layout.

---

## Completed Items

Completed items must remain available but compressed.

Default pattern:

- collapse under `Completed (N)`

Rules:

- completed items do not dominate the main view
- expanding completed items stays lightweight
- completed items preserve order and state
- bulk visibility is possible without cluttering the default view

---

## Quick Add

Quick add is mandatory.

Rules:

- always visible in the active list context
- supports fast sequential entry
- requires minimal fields by default
- adding an item does not open a full modal
- focus returns cleanly for repeated capture

Quick add should optimize for speed over metadata completeness.

---

## Detail Interaction

Detail is secondary.

Desktop:

- open in inspector

Mobile:

- open in bottom sheet or pushed contextual section

Detail may support:

- rename item
- add/edit note
- set quantity
- remove item
- inspect linked metadata

Detail must not break list context unnecessarily.

---

## Search and Filtering

Search is optional but should be compact when present.

Use it to filter the active list only.

Do not turn Lists into a multi-layered search product.

List-level sort/filter options may exist in overflow or compact controls.

They must remain secondary to the list itself.

---

## Theming

Lists may have lightweight identity through:

- accent color
- icon
- optional subtle image or theme treatment

Rules:

- theme supports recognition
- theme must not inflate header height
- theme must not fragment the product language
- identity is welcome
- decoration is not the goal

---

## Relationship With Other Surfaces

### Today

Today may surface list relevance when explicitly connected to current household reality.

Examples:

- a list linked to something happening today
- a list recently active and still incomplete

Today should not turn list items into tasks.

### Planning

Planning may link to lists.

A plan may reference a list for preparation or shared memory.

That linkage does not make the list a schedule.

### Areas

Lists may optionally reference an area.

Area association provides context and recognition, not heavy ownership workflow.

---

## Desktop Behavior

Desktop should prefer split-view composition.

Default pattern:

- app navigation rail visible
- list switcher visible
- active list central
- inspector on demand

Rules:

- switching lists is fast
- detail stays contextual
- quick add remains local
- content density remains high

---

## Mobile Behavior

Mobile should preserve the same logic in compressed form.

Default pattern:

- active list first
- list switcher through drawer or sheet
- item detail through bottom sheet

Rules:

- quick add remains obvious
- list switching remains low-friction
- no modal chain for simple item editing
- no separate mobile-specific product model

---

## Empty States

### No Lists

Show a clear, compact empty state with one primary action:

- create first list

### Empty Active List

Show the list identity and quick add immediately.

Do not use large decorative empty states.

The empty state should encourage capture, not consume space.

---

## Non-Goals

- no due-date semantics for list items by default
- no reminder semantics for list items by default
- no task-board behavior
- no calendar-grid behavior
- no heavy admin workflow inside the main surface
- no card gallery of lists
- no modal-first editing model on desktop

---

## Success Criteria

The surface is successful when:

- the active list is understandable instantly
- list switching is fast
- unchecked state is obvious
- adding five items feels trivial
- checking items feels frictionless
- completed state does not clutter the view
- desktop and mobile feel like the same product
- the surface feels dense, calm, and useful