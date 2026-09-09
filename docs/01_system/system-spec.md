---
Status: Canonical system scope reference
Detailed behavioral truth lives in /openspec/specs/*
---

# DomusMind — System Spec

## Purpose

This document defines the functional scope of DomusMind V1 at system level.

It covers:
- bounded context inventory
- capability groups
- context dependencies and guardrails
- explicit in-scope and out-of-scope decisions

For detailed behavioral contracts, see `/openspec/specs/*`.
For domain language and ubiquitous terms, see `docs/03_domain/`.
For architecture, see `docs/02_architecture/`.

---

## V1 Bounded Contexts

DomusMind V1 includes five core bounded contexts:

| Context | Owns |
|---|---|
| Family | Household identity and membership |
| Responsibilities | Accountability and ownership of household domains (Areas) |
| Calendar | Events, schedules, participants, reminders, external calendar ingestion |
| Tasks | Tasks, routines, assignment, completion |
| Lists | Household execution containers, list items, temporal item Agenda projection |

**Agenda** is the unified temporal read surface. It is not a bounded context. It gathers entries from Calendar, Tasks, and Lists into a single temporal view. The write model is divided; the read model is unified. No entity crosses a context boundary.

---

## Meal Planning (V2 Extension)

Meal Planning is a bounded context extension targeting the V2 domain expansion phase.

It owns: weekly meal plans, meal slots, recipes, weekly templates, and shopping list derivation.

Dependencies:
- Family (identity)
- Lists (integration via `ShoppingListRequested` domain event — Lists creates the shopping list in reaction)

Meal slot entries may project into Agenda as a read concern.

---

## Context Dependencies

- Responsibilities depends on Family
- Calendar depends on Family
- Tasks depends on Family
- Lists depends on Family

- Tasks may react to Calendar domain events
- Tasks may reference Responsibility domains for categorization
- Lists may reference Responsibility domains for contextual association
- Lists may optionally link to Calendar events as contextual anchors (informational only)
- Lists remains behaviorally independent from Tasks

- Meal Planning depends on Family for identity
- Meal Planning integrates with Lists via event-driven shopping list creation
- Meal Planning carries a soft reference to Responsibilities (food area) for context

---

## Capability Groups

### Family
Household and member management.

### Responsibilities
Responsibility domain management and ownership assignment.

### Calendar
Event lifecycle, participants, reminders, and external calendar ingestion (Phase 1: Microsoft Outlook, read-only, member-scoped Agenda projection only).

### Tasks
Task lifecycle (create, assign, complete, cancel, reschedule) and routine management (create, update, pause, resume). Routines are projected on-the-fly; they do not generate Task aggregates.

### Lists
List lifecycle, list item management, item importance, and temporal item fields (due date, reminder, repeat). Temporal list items project into Agenda as a distinct entry type. Repeat is independently sufficient for Agenda projection — a due date is not required.

### Agenda (read surface)
Unified temporal view across Calendar, Tasks, and Lists. Household scope and member scope. Day, week, and month modes. External calendar entries appear in member scope only.

---

## External Calendar Ingestion — Phase 1

- Provider: Microsoft Outlook only
- Access model: Microsoft Graph delegated auth (`Calendars.Read`, `offline_access`)
- Surface rule: imported entries appear in Agenda **member scope only**
- Behavior: read-only; no write-back; no conversion to native Events
- Sync: pull-based; manual and hourly scheduled refresh
- Supported horizons: 30, 90, 180, or 365 days forward (default: 90 days)
- Webhooks and bidirectional sync are out of scope for Phase 1

Full behavioral contract: see `/openspec/specs/calendar/spec.md`.

---

## Deferred to V1.1

- **assign-relationship** — relationship semantics between members are modeled in the domain but not exposed in V1
- **remove-member** — requires cascading validation of open task assignments and participant references; deferred to avoid complexity

---

## Out of Scope for V1

- Properties and inventory automation
- Finance
- Documents as a standalone context
- Pets as a separate operational context
- AI automation
- External integrations beyond Phase 1 Outlook calendar ingestion
- Bidirectional calendar sync or Outlook write-back
