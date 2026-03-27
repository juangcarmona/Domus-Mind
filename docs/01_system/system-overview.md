# DomusMind - System Overview

## Definition

DomusMind is a **household operating system**.

It maintains the structured operational state of a family and the environment in which that family lives.

The system models people, responsibilities, time, resources, assets, and obligations so that household coordination does not depend on the memory of a single individual.

DomusMind becomes the **shared operational memory of the family**.

---

# Core System Model

DomusMind maintains a continuously evolving model of:

- people
- relationships
- responsibilities
- time
- activities
- assets
- properties
- obligations
DomusMind is a household coordination system.

At system level, DomusMind is organized around a small set of core bounded contexts that together define the shared household model.

The current core system shape includes:

- Family
- Responsibilities
- Calendar
- Tasks
- Shared Lists

# Core Capabilities

## Family Modeling

These five contexts provide the current household coordination model:

- Family provides household identity and member structure.
- Responsibilities provides explicit accountability for household domains.
- Calendar provides time, schedules, participants, reminders, and the family timeline.
- Tasks provides execution through tasks, routines, assignment, and completion.
- Shared Lists provides reusable household lists, checklist state, and lightweight shared capture.

Together they make household structure, ownership, time, execution, and list-based coordination visible in one system.

---

# Context Collaboration

The collaboration model is intentionally simple:
- finances
- Family is the upstream identity provider.
- Responsibilities, Calendar, Tasks, and Shared Lists all depend on Family identifiers.
- Tasks may react to Calendar and Responsibilities events.
- Shared Lists may reference Responsibilities domains and may optionally link to Calendar entities.
- Shared Lists remains independent from task execution and scheduling semantics.
- maintenance
Contexts collaborate through identifiers and domain events.
- logistics
No context modifies another context's aggregates directly.
Each domain may include:
---
Responsibilities distribute cognitive ownership across the family.
# Core Capabilities
---
At system level, the core capabilities are:
## Unified Timeline
- family structure management
- responsibility ownership
- event scheduling and timeline visibility
- task and routine execution
- persistent shared checklist management
DomusMind maintains a **family timeline**.
These capabilities are defined in more detail in [docs/02_system/core-capabilities.md](core-capabilities.md).
The timeline aggregates:
---

# System Boundaries

The current system scope is limited to the documented core contexts.

It does not expand V1 system documentation into separate contexts for properties, documents, finance, AI automation, or external integrations.

Types:

# Outcome
- recurring routines
DomusMind turns household coordination into a structured shared system.
- tasks generated from responsibilities
In the current documented system shape, that means:
Examples:
- household identity is explicit
- responsibility is explicit
- time is explicit
- execution is explicit
- shared household list state is explicit

The result is a clearer and more coherent system description for the current product scope.
- document renewals


