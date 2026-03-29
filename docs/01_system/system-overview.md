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

Shared Lists provides reusable household lists, checklist state, and lightweight shared capture.

It supports persistent shared coordination without becoming a scheduling or execution model by itself.

---

## Context Collaboration

The collaboration model is intentionally simple.

- Family is the upstream identity provider.
- Responsibilities, Calendar, Tasks, and Shared Lists depend on Family identifiers.
- Tasks may react to Calendar and Responsibilities events.
- Shared Lists may reference responsibilities or calendar entities when useful.
- Shared Lists remains independent from task execution and scheduling semantics.

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
- shared household list state is explicit

The result is a clearer and more coherent operational model for the household.