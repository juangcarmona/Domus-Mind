# DomusMind - System Spec

## Purpose

This document defines the functional scope of DomusMind V1 at system level.

It links:

- bounded contexts
- feature specifications
- core capabilities

It is the entry point for executable product scope.

---

## V1 Scope

DomusMind V1 includes five core bounded contexts:

- Family
- Responsibilities
- Calendar
- Tasks
- Shared Lists

These contexts provide the minimum viable household operating model.

Shared Lists provides the **household execution container** layer of the household.

It supports a spectrum from lightweight memory through actionable, temporally-enriched items that project into the Agenda surface.

It complements the other core contexts by modeling:

- shopping lists and reusable checklists
- preparation lists tied to plans or areas
- items with importance flags and optional temporal fields (due date, reminder, repeat)
- time-aware items that appear in the Agenda surface alongside tasks and events

Shared Lists items with temporal fields project into Agenda as a distinct entry type.
Shared Lists does not own time — Calendar remains the source of truth for time.
Shared Lists items are never automatically converted to Tasks.

**Agenda is the unified temporal read surface.**
It gathers from five sources: Calendar Events, Tasks, Routines, temporal list items, and (member scope only) external calendar entries.
The write model is divided — each context owns its aggregates.
The read model is unified — Agenda projects all temporal entry types together.
No entity crosses a context boundary.

---

## Core Capabilities

V1 supports the following capability groups:

- family structure management
- responsibility ownership
- event scheduling
- external calendar ingestion for member-scoped read-only agenda projection
- task execution
- routine management
- persistent shared household execution container (lists)
- list item progressive capability model (base, importance, temporal)
- list item Agenda projection
- unified family timeline

---

## Context Map

### Family
Owns household identity and structure.

### Responsibilities
Owns accountability and ownership of household domains.

### Calendar
Owns events, schedules, participants, and reminders.

### Tasks
Owns tasks, routines, assignment, and completion.

### Shared Lists
Owns the household execution container for captured items.

Items support a progressive capability model: base state (name, checked, quantity, note), importance, and temporal fields (due date, reminder, repeat).

Shared Lists items with temporal fields project into Agenda as a distinct entry type.
Shared Lists does not overlap with:

- task execution lifecycle (Tasks)
- event scheduling and time ownership (Calendar)
- accountability semantics (Responsibilities)

---

## Context Dependencies

- Responsibilities depends on Family
- Calendar depends on Family
- Tasks depends on Family
- Shared Lists depends on Family

- Tasks may react to Calendar events
- Tasks may reference Responsibility domains

- Shared Lists may reference Responsibility domains
- Shared Lists may optionally link to Calendar entities

- Shared Lists remains behaviorally independent from Tasks.
- Shared Lists may carry item-level temporal fields (due date, reminder, repeat); these project into Agenda without creating Calendar or Task records.
- Repeat on a list item may be set independently of due date. Repeat is itself a temporal anchor sufficient for Agenda projection.

---

## V1 Feature Set

### Family
- create-family
- add-member

### Responsibilities
- create-responsibility-domain
- assign-primary-owner
- assign-secondary-owner
- transfer-responsibility

### Calendar
- schedule-event
- reschedule-event
- cancel-event
- add-event-participant
- remove-event-participant
- add-reminder
- remove-reminder
- view-family-timeline
- connect-outlook-account
- configure-external-calendar-connection
- sync-external-calendar-connection
- refresh-external-calendar-feeds
- disconnect-external-calendar-connection

### Tasks
- create-task
- assign-task
- reassign-task
- complete-task
- cancel-task
- reschedule-task
- create-routine
- update-routine
- pause-routine
- resume-routine

### Shared Lists (V1.1)
### Shared Lists
- create-shared-list
- rename-shared-list
- delete-shared-list
- add-item-to-shared-list
- update-shared-list-item
- remove-shared-list-item
- toggle-shared-list-item
- reorder-shared-list-items
- set-item-importance
- set-item-temporal
- clear-item-temporal
- get-family-shared-lists
- get-shared-list-detail
- get-temporal-items-for-agenda (projection query)

---

## Deferred to V1.1

The following were considered for V1 but are deferred to V1.1.
They have no blocking dependency on V1 completion.

### Family
- assign-relationship - relationship semantics between members are modeled in the domain but the capability is not exposed via API or UI in V1
- remove-member - member removal requires validating open task assignments and participant references; deferred to avoid cascading complexity in V1

---

## Out of Scope for V1

The following are explicitly outside V1:

- properties
- documents
- food and meal planning
- inventory automation (stock tracking, consumption models)
- pets as separate operational context
- finance
- AI automation
- external integrations beyond phase 1 Outlook calendar ingestion

---

## External Calendar Ingestion - Phase 1

DomusMind V1 includes a bounded external calendar ingestion capability under the Calendar module.

Phase 1 shape:

- provider: Microsoft Outlook only
- access model: Microsoft Graph delegated auth
- scopes: `Calendars.Read` and `offline_access`
- connection model: one member may own zero to many Outlook connections
- feed model: each connection may select zero to many provider calendars
- default horizon: now - 1 day to now + 90 days
- allowed forward horizons: 30, 90, 180, 365 days
- ingestion pattern: bounded `calendarView` load plus incremental `delta` refresh
- sync modes: manual sync and hourly scheduled refresh
- storage model: external connection, feed, and entry records separate from native `Event` aggregates
- surface rule: imported entries project into Agenda member scope only in phase 1
- behavior rule: imported entries remain read-only and keep an `Open in Outlook` action

Phase 1 does not include:

- bidirectional sync
- Outlook write-back
- attendee mutation
- reminder write-back into DomusMind reminder behavior
- unbounded history import
- conversion of imported entries into native household plans
- webhook subscriptions

---

## Implementation Rule

Every aggregate-changing feature spec must map to:

- one bounded context
- one aggregate
- one vertical slice

No feature may bypass aggregate boundaries.

Background workflows may orchestrate repeated execution of aggregate-scoped capabilities, but they must not collapse multiple aggregate mutations into one implicit command.

---

## Success Criteria

DomusMind V1 is complete when:

- household identity can be created
- members can be managed
- responsibilities can be assigned
- events can be scheduled
- Outlook accounts can be connected for read-only member-scoped calendar ingestion
- imported external calendar entries can be refreshed manually and on schedule
- tasks can be executed
- routines can be maintained
- shared lists can be created and reused
- temporal list items (with due date, reminder, or repeat) project into the Agenda surface as a distinct `list-item` entry type
- the Agenda surface shows a unified view of plans, tasks, routines, temporal list items, and (member scope) external calendar entries
- no projected entry type is ever collapsed into another type in the read model
- list items can be added, updated, reordered, and toggled
- list items may carry importance flags and temporal fields
- temporally-enriched list items project into Agenda as a distinct entry type
- timeline can be queried
- Agenda in Member scope can show imported external calendar entries without converting them into native plans
- projected list items in Agenda are distinguishable from tasks and events
- list items are never automatically converted to tasks or calendar events