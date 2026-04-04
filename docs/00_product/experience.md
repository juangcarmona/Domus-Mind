Status: Canonical
Audience: Product / Design / Engineering / Marketing
Scope: V1 with near-term UX refactor direction
Owns: Product experience, household language, major surface roles, onboarding shape, and scope guardrails
Depends on: docs/00_product/strategy.md, specs/system/system-spec.md


# DomusMind - Product Experience

This document defines how DomusMind should behave in real household use.

It does not define the visual system in detail.
Cross-surface shell, layout, density, and interaction rules belong in `surface-system.md`.

---

# Purpose

DomusMind should make the household understandable in seconds.

It should reduce:

- remembering
- reminding
- re-explaining
- renegotiating
- checking multiple tools to reconstruct what is happening

It should increase:

- shared visibility
- clarity
- ownership
- calm
- anticipation

The product should feel like one shared household system, not a collection of disconnected tools.

---

# Core Experience Principles

- Household-first. The product reflects shared household reality, not one person's private productivity space.
- Action first. The product should answer what matters now before asking for setup or interpretation.
- Timeline-first. Plans, routines, tasks, and relevant shared state should become legible together.
- Capture must stay easier than remembering. If adding or updating state becomes slow, adoption fails.
- Simplicity at the surface. Product complexity belongs in the model, not in the interface.
- One product, one language. All surfaces must feel like part of the same system.

The product should feel:

- quiet
- clear
- useful
- lightweight
- operational

---

# Household Language

DomusMind should speak in natural household terms.

Prefer:

- household
- people
- plans
- routines
- tasks
- lists
- areas
- today
- this week
- what matters
- what needs attention
- who owns what

Core translations:

| Internal Model | Household Language |
| -------------- | ------------------ |
| Family | Household |
| Member | Person |
| Event | Plan |
| Responsibility | Area |
| Shared List | List |

Task stays Task by design.

---

# Surface Roles

## Today

Today is the primary household operational surface.

It answers:

> What matters today for this household?

It should be:

- dense
- fast to scan
- truthful
- low-navigation
- shared-reality first

## Planning

Planning is the write-heavy temporal coordination surface.

It answers:

> What is coming, when is it happening, and what needs adjustment?

It should support:

- day, week, and month awareness
- plan creation and inspection
- quick date navigation
- conflict visibility
- preparation awareness

## Lists

Lists are the reusable shared-state surface.

They answer:

> What should be remembered, bought, checked, or prepared next time?

They should feel:

- fast
- compact
- row-based
- low-ceremony
- shared

Lists are not a task manager and not a calendar.

## Areas

Areas are the ownership surface.

They answer:

> Which parts of household life exist, and who owns what?

They should make ownership visible without becoming administrative.

## Member Agenda

Member Agenda is the individual deep temporal surface.

It answers:

> What does this person's day, week, or month actually look like?

It exists to inspect and plan one person's temporal load without losing shared context.

---

# Planning vs Timeline

DomusMind distinguishes between planning surfaces and the timeline.

- Planning is where the household creates or adjusts future temporal state.
- Timeline-oriented surfaces are where current household reality becomes legible.

The household should not need separate tools to understand:

- what is happening
- what needs attention
- who is involved
- what comes next

---

# Routines

Routines are recurring household work.

They should appear where they matter, not in a separate routine-management world.

Core expectations:

- routines are visible in operational surfaces
- ownership is understandable
- completion stays lightweight
- missed routine work surfaces quietly

---

# Calendar Coordination

DomusMind does not treat calendar coordination as a separate product.

The core question is:

> Who needs to be where, and when?

The product should make visible:

- what is happening
- when it happens
- who is involved
- where overload or conflict appears

Recurring fixed-time activities belong here as plans, not as tasks.

---

# Onboarding

Onboarding must create a useful household system quickly.

The flow should:

1. Start household.
2. Name the household.
3. Add people.
4. Add first useful state such as plans, routines, or lists.
5. Show Today and Planning immediately.

The result should be a working system, not an empty shell.

---

# Scope Guardrails

V1 must not drift into:

- a generic productivity app
- a calendar-first product
- an admin-heavy household manager
- a dashboard full of metrics
- a collection of unrelated page styles

Guardrails:

- one coherent product shell
- one household language
- dense operational surfaces
- visible ownership
- no separate routine manager
- no hidden work
- no floating-card composition as the product default

---

# Success Criteria

The experience direction is successful when:

- the household can understand Today in seconds
- planning the week feels calm and fast
- lists feel efficient and obvious
- ownership is visible without admin overhead
- desktop and mobile feel like the same product
- content dominates chrome

---

# Summary

DomusMind should feel like a quiet household operating system.

Its experience must be:

- household-first
- action-first
- timeline-first
- calm
- clear
- coherent

The product is one shared system with multiple operational surfaces inside it.