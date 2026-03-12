---
title: "Architecture"
description: "Domain-centric and API-first technical architecture."
---

## Domain-driven architecture

DomusMind is domain-centric: technology and interfaces are secondary to household invariants and business concepts.

## Bounded contexts

Core contexts include Family, Responsibilities, Calendar, Tasks, Properties, Food, Pets, and Administration.

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
