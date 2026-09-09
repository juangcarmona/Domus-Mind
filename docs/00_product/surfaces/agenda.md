Status: Canonical
Audience: Product / Design / Engineering
Scope: V1 and current surface-system-reboot
Owns: Unified Agenda surface — scope, time modes, views, interaction grammar, CRUD flows
Depends on:
  - docs/00_product/experience.md
  - docs/00_product/surface-system.md
  - docs/04_contexts/calendar.md
  - docs/04_contexts/tasks.md
  - docs/04_contexts/shared-lists.md
Replaces:
  - 00_product/surfaces/today.md
  - 00_product/surfaces/planning.md
  - 00_product/surfaces/member-agenda.md

---

# Surface Spec — Agenda

## Purpose

Provide the household's unified temporal surface.

Agenda answers:

- what is happening in this household
- when it is happening
- who is involved
- what needs attention
- where load is concentrated
- what is coming next

It answers these questions for the household as a whole and for any individual member, across day, week, and month time windows.

---

## Role

Agenda is DomusMind's primary temporal surface.

It is:

- temporal first
- household-first by default
- scope-switchable to any member
- write-capable in-place
- inspection-friendly

It is not:

- a personal productivity tool
- a separate app per person
- a reporting screen
- a decorative calendar page
- three surfaces with a nav entry each

---

## Default Entry State

Agenda opens at:

- Scope: Household
- Mode: Day
- Date: today

This is the primary household operational state — the answer to "what matters today?" without navigating anywhere.

A user moving to Week stays in Household scope unless they explicitly switch.
A user tapping a member switches to that member's scope, preserving the current mode and date.

---

## Shell

Agenda uses the standard product shell.

Desktop:

- left navigation rail
- compact page header
- main content canvas
- right contextual inspector panel

Mobile:

- top header
- primary content
- contextual detail via bottom sheet
- FAB-driven create flows

---

## Header

The Agenda header is compact and contains everything needed to orient and navigate.

Header contains:

- page label: `Agenda`
- scope selector: Household | [member names or avatars]
- mode toggle: Day | Week | Month
- date navigation: previous / today / next
- current date label (day name, date, or range label depending on mode)
- primary action: Add (opens create modal with current scope and date as defaults)

The scope selector and mode toggle must remain accessible without scrolling.

On mobile, the header compresses. Date navigation becomes swipe-based on the canvas. The primary action becomes a FAB.

---

## Scope

### Household

Shows the household as a single coordinated unit.

Contains:

- shared row: household-level plans and unassigned tasks
- one row or section per member

Household scope is coordination-oriented:

- who has what today
- where is load concentrated
- what is unassigned
- what is shared

### Member

Shows one specific member centered.

Contains:

- that person's tasks, plans, and routines
- household plans the person participates in
- household routines the person is responsible for
- imported external calendar entries owned by that member's selected calendar connections when relevant to the active date window

Member scope is individual-clarity-oriented:

- what does this person's day, week, or month look like
- where are conflicts or overload
- what needs attention for this person specifically

Shared household state only appears in member scope when it materially affects that person.

---

## Time Modes

### Day

Day shows a single day.

The display variant depends on scope:

**Household + Day = Board**

Layout:

- shared row at top (household plans, unassigned tasks)
- one member row per member
- items in priority order: overdue → due today → plans → routines → completed

Member row in collapsed state:

- shows max 2 items
- priority order preserved
- remaining items summarized as `+N`
- tapping expands in place (one expanded at a time)

Item grammar:

```
!   overdue task
□   task
●   hh:mm plan
⟳   routine
☆   projected list item (with importance)
◇   projected list item (without importance)
✓   completed
```

No legends. No explanatory labels.

Board is optimized for scanning the entire household instantly. It is not a timeline.

---

**Member + Day = Timeline**

Layout:

- hour-slot timeline from earliest relevant time to end of day (or 06:00–23:00 minimum)
- plans positioned by start time as blocks with duration
- all-day lane above the timeline
- tasks and routines in a compact non-timed section above the timeline or alongside it

Timeline is optimized for individual timing clarity: gaps, conflicts, load distribution for one person.

---

### Week

Week shows a 7-day window.

The window starts from the household's configured first day of week.

Display is consistent regardless of scope, with scope determining which data is shown:

- **Household + Week**: all members, with columns per day, group rows or swimlanes
- **Member + Week**: single member view, plans as time blocks, tasks/routines in compact lanes

Week is the default coordination view for when Day is not enough.

Structure:

- days as columns
- plans as positioned blocks (timed) or day-lane items (untimed)
- routines in a compact recurring lane
- tasks in a compact task lane
- overloaded days should be visually scannable
- selected item opens inspector (desktop) or bottom sheet (mobile)

Mobile Week:

- horizontal strip of date buttons above the canvas
- tapping a date switches to Day mode for that date
- or swipeable narrow week layout if layout budget allows

---

### Month

Month shows a calendar grid of the current month.

Purpose:

- high-level load awareness
- date navigation
- density scanning
- switching to a specific day or week

Structure:

- grid of weeks, rows of days
- each day cell shows:
  - event count or compact event titles
  - dot or accent for days with tasks or routines
- tapping a day in Month mode switches to Day mode for that date
- today is always visually distinguished

Month is a navigation and awareness surface, not a primary editing surface.

---

## Data

### What Agenda may show

Agenda is the unified temporal read surface. It gathers from five source categories:

| Source | Entry type | Write owner |
| ------ | ---------- | ----------- |
| Calendar events (plans) | Plan | Calendar |
| Tasks | Task | Tasks |
| Routines | Routine (projected occurrence) | Tasks |
| Temporal list items | List Item projection | Shared Lists |
| External calendar entries | Imported entry (member scope only) | External integration |

**The write model is divided. Agenda unifies only the read surface.**

- plans (Events in domain language): timed and untimed
- tasks: due on the selected date, overdue, or assigned to a member
- routines: projected occurrences for the selected date or range
- imported external calendar entries in Member scope only when they fall inside the selected date window and an active sync horizon
- **projected list items**: list items with temporal fields (due date, reminder, or repeat) that fall within the selected date window
- completed items: present but de-emphasized
- unavailability blocks: where relevant and available

### Projected List Items

Shared List items carrying temporal fields (due date, reminder, repeat) project into Agenda as a distinct entry type.

**Agenda does not own list items. Agenda projects them.**

The write model is divided: Calendar owns Event, Tasks owns Task and Routine, Shared Lists owns SharedListItem.
The read model is unified: Agenda shows all of them together in a single temporal surface.
This is the intended architecture. No entity crosses a context boundary.

Rules:

- projected list items appear alongside tasks and events
- projected list items carry a visual list-origin cue (e.g. list name or list icon)
- projected list items are distinguishable from Task entries and Calendar Events in all Agenda views
- projected list items are not editable from Agenda — edit navigates to the item's list
- selecting a projected list item opens read-detail inline (inspector/bottom sheet) with a `Open in Lists` action
- projected list items appear in both Household and Member scope (scoping follows the list's family scope — see scope placement rules below)
- projected list items follow the same priority ordering as tasks for unchecked state
- checked list items that still have a due date in the window appear de-emphasized (same as completed tasks)

### Projected List Item Scope Placement Rules

These rules define where temporal list items appear across Agenda scopes and time modes.

V1 list items are household-scoped. There is no per-member scoping for list items.

**Household scope:**

- Household + Day (Board): projected list items appear in the shared row (household section), not in individual member rows
- Household + Week: appear in a household-level lane on relevant day columns
- Household + Month: contribute to the day cell entry count on relevant days

**Member scope:**

- Member + Day (Timeline): projected list items appear in the non-timed section alongside tasks and routines, ordered by importance then due date
- Member + Week: appear in the task/non-timed lane for the relevant day
- A plan-linked temporal list item appears at the same position as any other temporal list item — independently of the linked plan's position

**Plan-linked temporal list items** project based on their own temporal fields.
They do not inherit the linked event's time slot.
The plan shows a compact list reference cue (unchecked count), separately from the item's own projection.
These are two independent appearance mechanisms.

### External calendar entry rules

- phase 1 external entries appear in Member scope only
- external entries never become editable household plans in Agenda
- external entries must show a subtle source cue such as `Outlook`
- selecting an external entry opens read-only detail
- read-only detail may offer `Open in Outlook`
- external entries should be omitted when they fall outside the selected date or mode window, even if stored locally
- stale connections may trigger a lightweight catch-up sync when Agenda opens

### Lists in Agenda

A plan may have a related list.

When it does, Agenda may surface a compact reference cue on the plan — showing the list name and unchecked item count.

Rules:

- the reference cue links to the full list in Lists surface
- the cue reflects the plan-linked list, not temporally projected items
- Agenda does not render list items inline as plan content
- Agenda does not convert list items to tasks
- selecting the list cue navigates to the Lists surface

Temporal list items that happen to be linked to the same event via their parent list appear independently via the projection mechanism above, not as a consequence of the plan link.

### What Agenda does not own

- list items (belongs to Lists)
- area ownership data (belongs to Areas)
- task-only management without temporal context

These may appear as contextual references but are not the main content model.

### Item Priority Order

Always show items in this order within a day or cell:

1. overdue
2. tasks due on this date
3. projected list items due on this date (unchecked, importance first)
4. plans (by start time ascending, untimed after timed)
5. routines
6. projected list items due on this date (unchecked, no importance)
7. completed / checked

---

## Interaction Grammar

### Selection

Tapping or clicking any item selects it.

- Desktop: selected item opens in the right inspector panel
- Mobile: selected item opens in a bottom sheet

Deselecting closes the inspector/sheet without navigation.

### Click Semantics

The following rules apply consistently across Household and Member scopes:

**Navigate:**
- clicking a member identity (avatar/name) in the household board or header switches to that member's scope, preserving current date and mode
- clicking an owner or supporter identity in the Areas inspector navigates to that person's Agenda context

**Inspect:**
- clicking an item/entry/card opens the right inspector (desktop) or bottom sheet (mobile)
- selection is reflected visually on the item
- this applies to Day and Week views; clicking any item in any day of the week grid opens the inspector

**Create:**
- clicking an empty time slot in the Timeline (Member + Day) or the Week grid opens the add modal pre-filled with the clicked date and time
- the primary `+ Add` action always opens the creation chooser modal

### Inspector / Bottom Sheet

The inspector panel shows a structured summary of the selected item without requiring Edit to understand it.

Panel structure:
1. Type / source cue (e.g. `Routine`, `Task`, `Plan`, `List Item`, or provider name for imported entries)
2. Time range (if timed)
3. Type-specific metadata:
   - **Plan**: participants, recurrence/reminder if applicable
   - **Task**: status, due date
   - **Routine**: recurrence summary, scope
   - **Projected list item**: list name, checked state, note if present, `Open in Lists` action
   - **Imported external entry**: source label, read-only state, `Open in Outlook` link
4. Edit action (native entries only; not available for projected list items or imported external entries)

The inspector does not include a second close button. The panel header owns the single close affordance.

### Editing

Edit actions launch the existing entity edit modal.

The modal receives the item type and id.
On close, the grid refreshes the current window.

### Creating

The primary action (FAB on mobile, `+ Add` on desktop) opens the add modal.

The add modal receives:

- current scope (household or member id) as default subject
- current date as default date
- current time slot if triggered from a timeline click

All three can be overridden inside the modal.

### Creating From Canvas

Clicking an empty time slot in the Timeline (Member + Day) or the Week grid opens the add modal pre-filled with the clicked date and time.

---

## Entry Points

- primary navigation → `Agenda`
- date link from any other surface
- member link from Areas or Settings → switches to Member scope for that person
- `/agenda` → default (Household, Day, today)
- `/agenda?date=YYYY-MM-DD` → household, day, specified date
- `/agenda?mode=week` → household, week, current week
- `/agenda?mode=month` → household, month, current month
- `/agenda/members/:id` → member scope, day, today
- `/agenda/members/:id?date=...` → member scope, day, specified date
- `/agenda/members/:id?mode=week` → member scope, week

### Legacy Route Redirects

- `/` or `/today` → `/agenda`
- `/planning` → `/agenda?mode=week`
- `/agenda/shared` → `/agenda`

These redirects exist to preserve existing links and bookmarks. They do not require separate pages.

---

## Mobile Behavior

Agenda on mobile must feel like the same product, not a simplified substitute.

Mobile-specific adaptations:

- header compresses: scope selector becomes avatar row or compact dropdown
- FAB replaces inline `+ Add`
- inspector becomes bottom sheet
- Week canvas: compact date strip at top, swipe to navigate dates
- Month canvas: same grid, tap navigates to Day
- no horizontal scrolling for standard views

Mobile must preserve:
- full scope switching
- full mode switching
- full CRUD capability

---

## What This Surface Replaces

| Retired Surface | Absorbed As |
|---|---|
| Today | Agenda → Household → Day (default entry state) |
| Planning | Agenda → Household → Week/Day/Month |
| Member Agenda | Agenda → Member → Day/Week/Month |

All behaviors from these three surfaces are preserved.
No capabilities are lost.
Navigation is simplified from three entries to one.

---

## Anti-Patterns

Do not:

- add a separate nav entry for "Today" as a shortcut
- put a metric strip or dashboard widget in the Agenda header
- navigate to a separate page for item creation or inspection
- apply different visual grammar between household and member scope unless the time mode genuinely requires it
- make Month the default view — it is navigation, not operational
- hide the scope selector on mobile behind an extra tap
- allow editing of a projected list item from Agenda (edit path is always via Lists)
- render a projected list item using task visual grammar (they must be visually distinguishable)
- omit the `Open in Lists` action from a projected list item inspector

---

## UX Grammar — List Items in Agenda (Locked)

This section locks the visual treatment for projected list items in all Agenda views.

### Identity markers

Projected list items must be distinguishable from tasks and plans in all views.

| Entry type        | Glyph | Distinguishing visual                   |
| ----------------- | ----- | --------------------------------------- |
| Task              | □     | checkbox affordance                     |
| Plan              | ●     | filled dot + time label                 |
| Routine           | ⟳     | cycle icon                              |
| List item (⭐)    | ☆     | star + list cue (importance = true)     |
| List item (plain) | ◇     | diamond + list cue (importance = false) |

### List-origin cue

Every projected list item must show a secondary list-origin cue:

- list name in secondary text style, OR
- small list icon

This cue must be visible even in collapsed row states.
Users must always know a projected item comes from a list, not a task.

### Inspector content for projected list items

The inspector for a selected projected list item shows:

1. Type label: `List Item`
2. List-origin cue: `From: [List Name]`
3. Title (read-only)
4. Due date or reminder (read-only)
5. Checked state (read-only indicator)
6. Importance (read-only star)
7. Note if present (read-only)
8. Single CTA: `Open in Lists` — navigates to the item in the Lists surface

The inspector must NOT include:

- edit controls
- status change controls
- date pickers
- delete action

All modifications go through Lists. This is non-negotiable.

### Checked state in Agenda

A projected list item that is checked (done) but still falls within the active date window:

- remains visible in the de-emphasized section (same as completed tasks)
- title is struck through
- glyph changes to `✓`
- does not disappear from the projection until its temporal fields are cleared or removed
