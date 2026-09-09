# Web App Specification

## Purpose

The DomusMind web app is the primary household operational interface. It presents one shared product shell containing multiple operational surfaces: Agenda, Lists, Areas, and Settings, with Meal Planning as an additional surface (see Notes).

The web app makes the household understandable — what is happening, what needs attention, who owns what, and what should be remembered — without requiring members to coordinate across separate tools.

All surfaces share the same shell, layout grammar, visual tone, and interaction model. The product must feel like one system, not a collection of disconnected screens.

---

## Requirements

### Requirement: App Shell and Navigation

The web app SHALL present a persistent shell that is consistent across all surfaces.

The shell has the following zones:
- left navigation rail (desktop)
- compact page header
- main content canvas
- optional right contextual inspector

On mobile, the left rail collapses into a compact navigation pattern (drawer or bottom strip). The main content fills the screen. Contextual detail is presented as a bottom sheet or pushed detail section.

The navigation gives access to each primary surface. The current surface is indicated in the navigation.

#### Scenario: User navigates from Agenda to Lists

- GIVEN the user is on Agenda
- WHEN the user selects Lists in the navigation
- THEN the Lists surface opens in the main content canvas
- AND the shell (navigation, header) remains visible and consistent

---

### Requirement: Surface Axis Separation

The four primary surfaces SHALL maintain strict ownership boundaries. No surface owns another surface's entities, and no semantic collapse is permitted between them.

| Surface | Owns |
|---|---|
| Agenda | Time — the unified temporal read surface |
| Lists | Household execution containers — capture, flexible execution, time reference |
| Areas | Ownership — who is accountable for what |
| Settings | Configuration — member preferences and integrations |

Tasks own a structured execution lifecycle. Areas own accountability. Lists own capture. Agenda reads from all sources but writes to none of them.

---

### Requirement: Agenda Default Entry State

Agenda SHALL open with a defined default state that answers the primary operational question: "what matters today?"

Default entry state:
- Scope: Household
- Mode: Day
- Date: today

Switching to Week mode preserves the current scope. Switching to a member scope preserves the current mode and date.

#### Scenario: User opens the app on a weekday

- GIVEN the app is freshly opened
- WHEN Agenda loads
- THEN the surface shows the Household scope, Day mode, and today's date
- AND no user configuration is required to reach this state

---

### Requirement: Agenda Scope

Agenda SHALL support two scopes: Household and Member.

**Household scope** shows the coordinated household picture — all members together, shared plans, household routines, household-level list projections, and unassigned tasks.

**Member scope** shows one specific member's temporal reality — their tasks, plans, and routines; household plans they participate in; household routines they are responsible for; and their imported external calendar entries when within the active date window.

Switching to a member scope is triggered by clicking a member identity (avatar or name). The current date and mode are preserved when switching scope.

#### Scenario: User switches from Household to a member scope

- GIVEN the user is in Household scope, Week mode
- WHEN the user clicks a member's avatar
- THEN the surface switches to that member's scope
- AND the mode remains Week and the date window is unchanged

---

### Requirement: Agenda Time Modes

Agenda SHALL support three time modes: Day, Week, and Month.

**Day mode** presents different layouts depending on scope:
- Household + Day = Board: a shared household row plus one row per member, optimized for scanning the full household at a glance
- Member + Day = Timeline: an hour-slot timeline for one member, with plans positioned by time and tasks/routines in a compact non-timed section

**Week mode** shows a 7-day window beginning on the household's configured first day of week. Plans appear as time blocks (timed) or day-lane items (untimed). Routines appear in a recurring lane. Tasks appear in a compact task lane.

**Month mode** shows a calendar grid. Each day cell shows entry counts and presence indicators. Month is a navigation and load-awareness surface, not a primary editing surface. Tapping a day cell switches to Day mode for that date.

The mode toggle is accessible without scrolling, on both desktop and mobile.

#### Scenario: User taps a date in Month mode

- GIVEN the user is in Month mode
- WHEN the user taps a day cell
- THEN the surface switches to Day mode for that date
- AND the scope is preserved

---

### Requirement: Agenda Item Priority Ordering

Agenda SHALL display items within a day or cell in a defined priority order.

Order within any day or cell:
1. Overdue items
2. Tasks due on this date
3. Projected list items due on this date (unchecked, with importance)
4. Plans (by start time ascending; untimed plans after timed)
5. Routines
6. Projected list items due on this date (unchecked, without importance)
7. Completed and checked items

Completed and checked items remain visible but de-emphasized, not removed.

---

### Requirement: Agenda Selection and Inspection

Selecting any item in Agenda SHALL open its detail without navigating away from the surface.

- Desktop: the item opens in the right inspector panel. The surrounding content remains visible.
- Mobile: the item opens in a bottom sheet.

Deselecting closes the inspector or bottom sheet without any navigation.

Agenda does not navigate to a separate page for item inspection or creation.

#### Scenario: User selects a plan on desktop

- GIVEN the user is viewing Agenda in Day mode on desktop
- WHEN the user clicks a plan entry
- THEN the plan detail opens in the right inspector panel
- AND the day board or timeline remains visible behind the inspector

#### Scenario: User deselects an item

- GIVEN an item is selected and the inspector is open
- WHEN the user dismisses the inspector
- THEN the inspector closes
- AND the Agenda view is unchanged

---

### Requirement: Agenda Projected List Items

List items with temporal fields (due date, reminder, or repeat) SHALL project into Agenda as a distinct entry type.

Agenda does not own list items. Agenda projects them. The write owner is always Lists.

Projected list items are visually distinguishable from tasks and plans in all Agenda views. Each projected item carries a list-origin cue (list name or list icon). This cue must be visible even in collapsed row states.

Projected list items:
- are not editable from Agenda
- open in a read-only inspector with a single `Open in Lists` action
- appear in both Household and Member scope (list items are household-scoped in V1)
- in Household + Day: appear in the shared household row, not in individual member rows
- in Member scope: appear in the non-timed section alongside tasks and routines

#### Scenario: User selects a projected list item in Agenda

- GIVEN a list item has a due date and appears in today's Agenda
- WHEN the user selects it
- THEN a read-only inspector opens showing the item's title, due date, list origin, and checked state
- AND a single `Open in Lists` action is available
- AND no edit controls are present

#### Scenario: User attempts to edit a projected list item in Agenda

- GIVEN a projected list item is selected in the Agenda inspector
- WHEN the user uses the `Open in Lists` action
- THEN the user is navigated to the item in the Lists surface
- AND the item is editable there

---

### Requirement: Agenda External Calendar Entries

Imported external calendar entries SHALL appear in Agenda in Member scope only.

External entries (Outlook) appear read-only. They carry a visible source cue (e.g., `Outlook`). Selecting an external entry opens read-only detail. The detail may offer an `Open in Outlook` action.

External entries are never converted to editable household plans.

External entries are omitted from Agenda when they fall outside the selected date window, even if cached locally.

#### Scenario: External entry appears in Member scope

- GIVEN a member has an active Outlook connection with imported entries
- WHEN the user views that member's Day timeline
- THEN imported Outlook entries appear with a visible source cue
- AND they are not presented with edit affordances

#### Scenario: External entry is absent from Household scope

- GIVEN a member has imported Outlook entries
- WHEN the user views Agenda in Household scope
- THEN no imported Outlook entries are shown

---

### Requirement: Agenda Plan–List Reference Cue

When a plan has an associated list, Agenda SHALL surface a compact reference cue on the plan entry.

The cue shows the list name and the current unchecked item count. Selecting the cue navigates to the full list in the Lists surface.

Agenda does not expand list items inline as plan content. Agenda does not convert list items to tasks. The cue and the temporal projection of list items are two independent mechanisms.

---

### Requirement: Lists Surface Structure

The Lists surface SHALL provide a persistent split layout: a list switcher and an active list working area.

The list switcher shows all household lists with their name, unchecked count, and optional area or plan context cue. Switching lists preserves shell context. The active list is visually clear in the switcher.

The active list shows unchecked items first, completed items collapsed and accessible. A quick add bar is always visible. Item selection opens the inspector (desktop) or bottom sheet (mobile).

Inspector sections for capabilities not set on an item are collapsed by default. Empty sections do not show placeholder fields, but are accessible when the user needs to add that capability.

On desktop: switcher pane + active list pane + optional inspector (three-column).
On mobile: active list fills the screen; list switcher is accessible via drawer or sheet.

#### Scenario: User switches between lists

- GIVEN the user is on Lists with a list selected
- WHEN the user taps another list in the switcher
- THEN the new list opens in the active list pane
- AND the switcher remains visible on desktop

---

### Requirement: List Item Capability Model

List items SHALL support a progressive capability model. Not every item requires every capability.

**Base (always present):**
- title
- checked state (toggle)

**Optional capabilities:**

| Capability | Fields | Effect |
|---|---|---|
| Importance | starred flag | item is visually prioritized in the list |
| Temporal | due date, reminder, repeat | item becomes eligible for Agenda projection |

Temporal fields are each independently optional. Due date alone, reminder alone, or repeat alone is sufficient for Agenda projection. No temporal field requires another as a prerequisite.

A plain item with a title only is fully valid. A starred item with all three temporal fields is also valid.

Items do not carry an assignee, a status system beyond checked/unchecked, comments, attachments, or steps.

---

### Requirement: Lists Temporal Projection to Agenda

List items with any temporal field SHALL project into the Agenda surface as a distinct entry type.

This is a non-negotiable cross-surface capability. The write model stays divided: Lists owns the item. Agenda projects it. No entity crosses a context boundary.

Clearing all temporal fields from an item removes it from Agenda projection immediately.

When a repeat rule is set alongside a due date, the due date acts as the anchor for the current recurrence.

---

### Requirement: List Creation

Creating a list SHALL require only a name. No other metadata is required at creation time.

A list may be created from the Lists surface, from the detail of a plan, or from the context of an Area. Linking to a plan or Area is always optional and may be done after creation.

Item capture SHALL not require a modal. Quick add must support sequential entry without interruption. Focus returns for repeated capture.

#### Scenario: User creates a list with a name only

- GIVEN the user is on the Lists surface
- WHEN the user creates a new list with a name and no other fields
- THEN the list is created and becomes available in the switcher
- AND the user can immediately begin adding items

---

### Requirement: List Lifecycle

A list follows a defined lifecycle: created → active use → rested → archived.

Items are consumable. Lists are designed for reuse across uses.

Completed items are de-emphasized behind a collapse toggle (`Completed (N)`) but remain accessible. They are not removed.

Archived lists are no longer in active use but are retained.

---

### Requirement: Areas Surface

The Areas surface SHALL display household ownership structure as a dense, scannable list ordered to make gaps visible first.

Default ordering:
1. Unowned Areas (no primary owner)
2. Partially assigned Areas (primary owner present, no secondary owner)
3. Fully assigned Areas
4. Archived Areas (only shown when the archived filter is applied)

Each Area row shows: Area name, primary owner or a gap indicator if unowned, and support members if any.

Selecting an Area opens the inspector (desktop) or bottom sheet/pushed section (mobile). There is no separate full-detail page for normal area inspection on desktop.

The inspector shows: Area identity (name and color cue), owner with inline change affordance, support members with add/remove affordance, related work counts (open tasks, plans, routines, linked lists), and explicit creation entry points (New task, New routine, New plan — each pre-filled with the current Area context).

Clicking an owner or supporter name in the inspector navigates to that person's Agenda context.

There is no separate full-detail page for Areas. A direct URL to an area detail page redirects to the Areas surface with that area's selection restored.

#### Scenario: User views Areas with an unowned Area

- GIVEN a household has one Area with no primary owner
- AND other Areas have owners assigned
- WHEN the user opens the Areas surface
- THEN the unowned Area appears at the top of the list
- AND a gap indicator is shown where the owner would appear

#### Scenario: User selects an Area on desktop

- GIVEN the user is on the Areas surface
- WHEN the user clicks an Area row
- THEN the Area inspector opens on the right
- AND the Areas list remains visible on the left

---

### Requirement: Settings Surface

Settings SHALL be the low-frequency configuration surface. It does not duplicate operational functionality present in other surfaces.

Phase 1 Settings has three primary sections:
- **Profile** — member identity, account details, calendar connections
- **Household** — household-level preferences
- **Preferences** — personal defaults

Settings opens to the Profile section by default.

Outlook calendar connections are managed in Profile. Each member manages their own connections.

Settings SHALL NOT handle: editing of native household plans, browsing of imported entries as a calendar-like surface, provider write-back, or administrative control of another member's calendar connections in phase 1.

#### Scenario: User opens Settings

- GIVEN the user navigates to Settings
- THEN the Profile section is displayed by default
- AND the calendar connections section is visible without deep navigation

---

### Requirement: Outlook Calendar Connection Management

A member SHALL be able to connect, configure, sync, and disconnect their Outlook calendar from Settings / Profile.

**Connect:** launches a provider auth flow. On return, the new connection appears and is configurable immediately (calendar selection, sync horizon).

**Sync now:** triggers manual sync for a single connection. Progress is shown inline. Success or failure is surfaced inline without leaving Settings.

**Sync calendars:** available when multiple connections exist. Dispatches sync for all connections simultaneously. Shows aggregate progress at section level without hiding per-connection failure state.

**Disconnect:** requires an explicit confirm step. Confirmation must make the outcome clear: imported Outlook entries will disappear from Agenda; native household plans are unaffected.

Each connection row shows: provider label, account email, included calendar count, sync horizon, last sync time, and current status.

#### Scenario: User disconnects an Outlook connection

- GIVEN a member has an active Outlook connection
- WHEN the member selects Disconnect
- THEN a confirmation step is shown
- AND the confirmation explains that imported Outlook entries will be removed from Agenda
- AND upon confirmation the connection is removed and imported entries are no longer shown in Agenda

---

### Requirement: Inspector and Modal Usage

The web app SHALL use consistent patterns for contextual detail across all surfaces.

**Inspector (right panel on desktop / bottom sheet on mobile):**
- used when inspecting one selected item
- used for lightweight editing
- surrounding context remains visible
- default pattern for item detail in all surfaces

**Modal:**
- used for destructive actions requiring confirmation
- used for short interruptive flows that must be completed or cancelled before continuing
- not the default for item inspection

**Full-page navigation:**
- used when moving to a distinct work context
- used when a flow requires depth or sustained focus

Contextual creation actions (add plan, add task, add list item) stay close to the current surface. Creation does not require navigating to a separate page.

---

### Requirement: Mobile Behavior

The mobile web app SHALL present the same product logic as desktop in a collapsed form.

- Left navigation rail collapses to a compact navigation pattern
- Page headers compress; date navigation on Agenda becomes swipe-based on the canvas
- Contextual detail is presented as a bottom sheet
- Creation flows use a FAB where the desktop uses a compact header action
- Content priority is highest — chrome is compressed

Mobile must feel like the same product at a smaller scale, not a different product.

---

## Notes

### Meal Planning surface

The Meal Planning surface spec (`00_product/surfaces/meal-planning.md`) is fully specified and describes a complete surface. However, the domain context document (`docs/04_contexts/meal-planning.md`) marks Meal Planning as a **V2 bounded context**, not part of the V1 core. Whether Meal Planning is included as a V1 navigation entry in the web app is not definitively resolved in the source material. It is excluded from the navigation requirements above until its V1 inclusion is confirmed.

### Tasks surface

Tasks are referenced throughout Agenda as a primary data source (tasks project into Agenda). However, no Tasks surface spec was available in the source material. Task surface behavior is unspecced. This spec does not cover a Tasks surface.

### Agenda catch-up sync timing

The agenda spec states "stale connections may trigger a lightweight catch-up sync when Agenda opens." The conditions defining a "stale" connection and the exact trigger point are not fully specified.

### Member scope availability

Member scope in Agenda requires a household with multiple members. Single-member households or the configured scope defaults for them are not specified.

---

## Source References

- `docs/00_product/surface-system.md`
- `docs/00_product/experience.md`
- `00_product/surfaces/agenda.md`
- `00_product/surfaces/areas.md`
- `00_product/surfaces/lists.md`
- `00_product/surfaces/meal-planning.md`
- `00_product/surfaces/settings.md`
