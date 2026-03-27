---
title: "Architecture"
description: "Domain-centric and API-first technical architecture."
---

## Domain-driven architecture

DomusMind is domain-centric: technology and interfaces are secondary to household invariants and business concepts.

## Current core - V1

The following bounded contexts are implemented in V1:

- **Family** - household identity and member structure
- **Responsibilities** - ownership of household domains and areas
- **Calendar** - events, schedules, participants, and reminders
- **Tasks** - tasks, routines, assignment, and completion

## Extended model - future

The following contexts are planned for future releases:

- **Shared Lists** - persistent shared checklists (V1.1)
- **Properties** - household assets and property records
- **Food** - meal planning and ingredient tracking
- **Pets** - care schedules and records for household animals
- **Administration** - documents, contracts, and household records

## Vertical slices

Capabilities are delivered through vertical slices that include request, validation, application logic, persistence interaction, and API endpoint.

## Command and query model

Application behavior uses command/query separation to keep write-side intent and read-side views explicit.

## Domain events

Meaningful changes such as MemberAdded or RoutineTriggered are represented as events for loose coupling and collaboration.

## Modular monolith

The current execution model is modular monolith, enabling strong boundaries while keeping operational complexity manageable.

## API-first platform

Web, mobile, messaging, and automations interact through a capability-oriented API, not direct database exposure.
