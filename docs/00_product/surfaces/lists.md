# Surface Spec — Lists

## Purpose

Provide a fast, dense, household execution surface for captured items.

This surface answers:

- what lists exist
- which list matters now
- what needs to be done, bought, or remembered
- which items have urgency or timing
- what should be added, checked, updated, or scheduled next

Lists are household execution containers supporting a spectrum from memory to action.

They are:

- reusable
- toggle-based
- shared by default, optionally private
- persistent across uses

They may contain:

- plain memory items (name only)
- important items (starred)
- time-aware items (due date, reminder, repeat)

They are not:

- a full task management system
- a calendar view
- an assignment and ownership model

---

## Depends On

- `docs/00_product/experience.md`
- `docs/00_product/surface-system.md`
- `docs/04_contexts/shared-lists.md`

---

## Entry Points

- main navigation → `Lists`
- contextual reference from Agenda when a plan has a related list
- deep link to a specific list

---

## Conceptual Model

### What a List Is

A list is a named household execution container for grouped items.

A list exists to answer: *"What should be remembered, bought, checked, prepared, or done next time?"*

Lists are:

- grouped by context or purpose, not only by time
- independent objects — they can exist without any link to a plan or area
- optionally contextual — they may be associated with a plan or area
- persistent across uses
- reusable by design

Items within a list may range from simple memory entries to time-aware actionable entries.

Common household examples:

- groceries
- packing for a trip
- preparation for an event
- restocking essentials
- recurring situation checklists
- school preparation items with due dates

### What a List Is Not

A list is not a task board.

A list item is not a task by default.

A list does not replace Agenda or Tasks.
Lists own **capture and flexible execution**.
Tasks own **structured execution lifecycle**.
Calendar owns **time**.

A list linked to a plan remains a list. Its items do not become scheduled tasks.

### Lifecycle

A list follows this lifecycle:

1. **created** — with a name, standalone or from context
2. **used and filled** — items are added and the list grows
3. **active** — items are checked and unchecked during use
4. **rested** — items remain, ready for next use
5. **archived** — if the list is no longer needed

Items are consumable.
Lists are often not — they are meant to be reused.

Reuse is more important than one-time completion.

### Household Boundaries

The four core axes of DomusMind are:

- **Agenda** → owns TIME
- **Tasks** → owns STRUCTURED EXECUTION LIFECYCLE
- **Areas** → owns OWNERSHIP
- **Lists** → owns HOUSEHOLD EXECUTION CONTAINER (capture → action → time reference)

No semantic collapse is permitted across these axes.

---

## Role

Lists is the household surface for reusable, flexible execution containers.

It is optimized for:

- scan speed
- quick capture
- quick toggle state
- inspector-driven depth
- low-friction switching between lists

The active list is the working surface.

---

## Core Principles

- content over chrome
- one row = one item
- quick add always visible
- detail lives in the inspector
- counts are visible before opening a list
- completed items stay accessible but compressed
- theming supports recognition, not decoration
- density is high without feeling hostile
- capture must be faster than remembering
- grouping matters more than scheduling
- reuse is more important than one-time usage
- context enriches but does not define the list
- temporal fields enrich items; they do not define the list

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
- optional links or metadata

---

## Item Capability Model

Items support a progressive capability set. Not every item uses every field.

### Base (always present)

- name
- checked state

### Extended base (common, lightweight)

- quantity
- note

### Optional capabilities

Each capability is independently optional:

| Capability  | Fields                              | Effect                                      |
| ----------- | ----------------------------------- | ------------------------------------------- |
| importance  | starred flag                        | item appears visually prioritized in the list |
| temporal    | due date, reminder, repeat          | item is eligible for Agenda projection; each field is independently optional |

Temporal field independence:

- `due date` alone: Agenda projects the item on that date
- `reminder` alone: Agenda projects the item at that datetime
- `repeat` alone: Agenda projects the item on each recurrence occurrence
- any combination of the three is valid
- no temporal field requires another as a prerequisite

Items with temporal fields may appear in the Agenda surface as projected list items.

Items do not require all capabilities.
A plain item with only a name is fully valid.
A starred item with a due date and reminder is also fully valid.

### What items do not include

- assignee — assignment belongs to Tasks
- status system beyond checked/unchecked — lifecycle belongs to Tasks
- comments — deferred
- attachments — deferred
- steps — deferred

---

## Inspector = Command Surface

The item inspector is the primary command surface for item capabilities.

It is not a modal.
It is contextual panel (desktop) or bottom sheet (mobile) that opens on item selection.

Inspector sections:

### 1. Status

- check / uncheck toggle
- visual state reflects current checked status

### 2. Title

- editable inline
- no confirmation required on change

### 3. Importance

- single star affordance
- tap to toggle starred / not-starred
- visually distinct when active

### 4. Time

Three sub-fields, each independently optional:

- **Due date** — date picker; clearing removes temporal eligibility from Agenda if no reminder is set
- **Reminder** — time-aware alert; clearing removes temporal eligibility from Agenda if no due date is set
- **Repeat** — repeat rule; independently optional and Agenda-eligible without due date

Clearing all temporal fields removes the item from Agenda projection.

### 5. Metadata

- **Quantity** — numeric or text
- **Note** — freeform, multi-line

### 6. Actions

- remove item
- (future: move to list, duplicate)

---

## Creation Model

Creating a list must be frictionless.

A list can be created with a name only.
No required metadata at creation time.

A list can be:

- created standalone from the Lists surface
- created from context (from an event detail, from an area)
- linked later to an existing plan or area

Linking is always optional and never required.

Items are added inline.
No modal is required for basic item capture.
Quick add must support sequential entry without interruption.

---

## Relationship with Agenda

**Temporal list items are first-class Agenda sources.**

An item with any temporal field (due date, reminder, or repeat) projects into the Agenda surface as a distinct `list-item` entry. This is not incidental or secondary behavior — it is a defined cross-surface capability.

The write model stays divided: Shared Lists owns the item. Agenda projects it.
Agenda does not own, edit, or convert list items.

Rules:

- projected items appear alongside tasks and events in Agenda
- projected items carry a visual list-origin cue (list name or icon)
- projected items are distinguishable from Tasks and Calendar events in Agenda
- projected items are not editable from Agenda — edits must go through the Lists surface
- selecting a projected item in Agenda navigates to its list context
- Agenda does not expand list content beyond projected temporal items
- a list linked to a plan retains list semantics; the link does not cause all items to project

Repeat rule:

- a repeat rule may be set on a list item without a due date
- the repeat rule alone is sufficient to make the item Agenda-eligible
- when both `repeat` and `dueDate` are set, `dueDate` acts as the anchor date for the current recurrence

When a plan in Agenda has a related list:

- Agenda shows a compact reference cue — the list name and unchecked count
- selecting the cue navigates to the full list in the Lists surface
- Agenda does not expand list items inline as plan content
- Agenda does not convert list items to tasks

---

## Relationship with Areas

An area may have one or more associated lists.

When a list is associated with an area:

- the list appears as contextual memory for that area
- it represents what is remembered or tracked in the context of that area
- it is not a workload view
- it is not an execution view
- it does not affect ownership or responsibility assignment

An area-linked list is navigable from the Areas inspector but lives and behaves the same as any other list.

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

Each item row represents one list item.

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

Detail is accessed through the inspector.

Desktop:

- selecting an item opens it in the inline inspector panel
- detail stays within the list context
- editing is in place — no modal required for name or simple fields
- inspector sections are contextually collapsed when empty (e.g. no temporal fields shown as empty, but accessible)

Mobile:

- selecting an item opens a bottom sheet
- bottom sheet stays compact
- close returns focus to the list

The inspector is the command surface for all item capabilities.
All item fields (importance, temporal, quantity, note) are accessible through the inspector.
Quick add does not require opening the inspector.

---

## Anti-Patterns

The following behaviors are explicitly rejected for the Lists surface.

**Semantic drift:**

- turning list items into tasks automatically
- treating a temporal item as if it belongs to the Tasks context
- treating a list linked to a plan as a task batch for that plan
- displaying all list items in the Agenda timeline (only temporally-enriched items project)

**Layout drift:**

- card-based layout for list items
- oversized rows with too much whitespace
- hero sections or decorative headers above the list

**Interaction drift:**

- requiring a modal to add a basic item
- opening a full separate page to view a list item
- requiring complex metadata before capture
- hiding the inspector affordance

**Model drift:**

- making lists person-centric instead of household-centric
- treating lists as disposable task batches
- forcing context linkage as a required step
- importing full assignment and status lifecycle from Tasks
- exposing steps, comments, or attachments (deferred)

---

## UX Grammar — Lists Surface (Locked)

This section locks the visual grammar for list item states. All implementations must conform.

### Row states

| State                             | Visual treatment                          |
| --------------------------------- | ----------------------------------------- |
| Base (name only)                  | title, toggle circle                      |
| Importance = true                 | star icon on trailing edge, filled        |
| Importance = false                | star icon on trailing edge, unfilled      |
| Has due date (not overdue)        | small date cue below title                |
| Has due date (overdue)            | date cue in accent warning color          |
| Has reminder                      | small bell icon alongside date cue        |
| Checked                           | title struck through, row de-emphasized   |

Row height must remain compact in all states.
No state causes a row to expand without user selection.

### Icon for temporal state in row

Use a minimal inline cue below the title for temporal fields.
Do not use large badge or chip layout.
Use secondary text style.

Example cue: `Apr 12 · 09:00 ⏰`

### Inspector icons

| Section    | Icon                        |
| ---------- | --------------------------- |
| Importance | ☆ / ★ (star, toggle)        |
| Due date   | calendar                    |
| Reminder   | bell                        |
| Repeat     | refresh/cycle               |
| Quantity   | hash / number               |
| Note       | text block                  |
| Remove     | trash                       |

### Edit path

**The only way to edit a list item is through the Lists surface.**

A projected list item appearing in Agenda:

- opens a read-only inspector or bottom sheet when selected
- shows title, due date, checked state, list name
- provides a single action: `Open in Lists`
- does NOT provide an edit affordance in Agenda
- does NOT expand into the full inspector

This path is non-negotiable. Violating it breaks the ownership model.
