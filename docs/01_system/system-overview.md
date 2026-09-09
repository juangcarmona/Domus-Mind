# DomusMind - System Overview

## Definition

DomusMind is a **household operating system**.

It maintains the structured operational state of a family and the environment in which that family lives.

The system models people, responsibilities, time, activities, resources, assets, and obligations so that household coordination does not depend on the memory of a single individual.

DomusMind becomes the **shared operational memory of the family**.

---

## Core System Model

At system level, DomusMind is organized around a small set of core bounded contexts that together define the shared household model.

The current core system shape includes:

- Family
- Responsibilities
- Calendar
- Tasks
- Shared Lists

Together, these contexts make household structure, ownership, time, execution, and lightweight shared coordination visible in one system.

---

## Core Capabilities

### Family

Family provides household identity and member structure.

It defines who belongs to the household and supplies the identifiers used by the other contexts.

### Responsibilities

Responsibilities provides explicit accountability for household domains.

It makes ownership visible so household coordination does not depend on assumptions or memory.

### Calendar

Calendar provides time, schedules, participants, reminders, and timeline visibility.

It represents plans and time-bound commitments in the life of the household.

### Tasks

Tasks provides execution through tasks, routines, assignment, and completion.

It captures actionable work and recurring operational activity.

### Shared Lists

Shared Lists provides the household execution container: reusable, flexible lists whose items range from simple memory to actionable, time-aware entries.

It captures what the household needs to remember, buy, check, prepare, or act on — grouped by context or purpose.

List items support a progressive capability model: base state (name, checked, quantity, note), importance, and temporal fields (due date, reminder, repeat).
List items with temporal fields project into the Agenda surface as a distinct entry type.

List items are not tasks. A task carries structured execution semantics (assignment, lifecycle state) that list items do not require.
A list linked to a calendar event retains list semantics — linking does not cause items to inherit scheduling behavior.

Shared Lists is the household's primary capture and flexible execution layer.

---

## Agenda as Unified Temporal Surface

Agenda is the temporal read surface for the household.

It gathers temporal entries from all relevant write contexts into a single projected view:

| Source context | Entry type in Agenda |
| -------------- | -------------------- |
| Calendar | Plans (Events), all-day and timed |
| Tasks | Tasks (due-date bearing) |
| Tasks | Routines (projected occurrences) |
| Shared Lists | Temporal list items (due date, reminder, or repeat) |
| External integration | External calendar entries (member scope only, read-only) |

**Write model is divided. Read model is unified.**

Each context retains full ownership of its aggregate:

- Calendar owns Event. Agenda does not own Event.
- Tasks owns Task and Routine. Agenda does not own them.
- Shared Lists owns SharedListItem. Agenda does not own list items.
- External calendar entries are read-only integration state. Agenda projects them, not owns them.

Agenda does not collapse these entities. It projects them together into a coherent temporal view.
No aggregate crosses a context boundary. No entity is merged.

Projection is a read concern. It is not an ownership transfer.

---

## Shared Temporal Vocabulary

Contexts may share temporal vocabulary — due date, reminder, repeat — while owning separate entities.

- Calendar defines what a scheduled time means for an Event.
- Tasks defines what a due date means for a Task or Routine.
- Shared Lists carries temporal references on list items. Calendar gives those dates household meaning.

Shared vocabulary does not imply shared entities.
A due date on a list item and a due date on a task are the same concept, held by different owners.

---

## Context Collaboration

The collaboration model is intentionally simple.

- Family is the upstream identity provider.
- Responsibilities, Calendar, Tasks, and Shared Lists depend on Family identifiers.
- Tasks may react to Calendar and Responsibilities events.
- Shared Lists may reference responsibilities or calendar entities when contextually useful.
- Shared Lists does not own time. Items may carry temporal references (due date, reminder, repeat) which project into Agenda. Modifying temporal fields on an item does not create Calendar or Task records.
- Agenda is not a bounded context. It is the unified temporal read surface assembled from Calendar, Tasks, Shared Lists, and external integration data.

Contexts collaborate through identifiers and domain events.

No context modifies another context's aggregates directly.

---

## System Boundaries

The current documented system scope is limited to the core contexts listed above.

For the current product scope, the system does not define separate bounded contexts for:

- finance
- property management
- document management
- AI automation
- external integrations

Those areas may become relevant later, but they are not part of the current core system definition.

---

## Outcome

DomusMind turns household coordination into a structured shared system.

In the current documented system shape, that means:

- household identity is explicit
- responsibility is explicit
- time is explicit
- execution is explicit
- shared household execution containers (lists) are explicit, with progressive item capabilities including temporal projection into Agenda
- the unified temporal surface (Agenda) gathers entries from all contexts without merging their write models

The result is a clearer and more coherent operational model for the household.