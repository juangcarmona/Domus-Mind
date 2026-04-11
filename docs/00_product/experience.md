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
- **Unified temporal surface.** Plans, routines, tasks, temporal list items, and relevant external calendar entries should become legible together in a single surface (Agenda). The write model stays divided — each context owns its entities. The read model is unified — Agenda gathers all temporal entries into one surface.
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

## Agenda

Agenda is the unified household temporal surface.

It answers:

> What is happening in this household — today, this week, this month — for everyone or for one person?

It operates in two scopes:

- **Household**: shows the shared household picture — all members, shared plans, owned tasks, routines
- **Member**: shows one person's temporal reality in depth

And in three time modes:

- **Day**: household board (compact, all members) or individual timeline (hour-positioned, one member)
- **Week**: 7-day coordination view
- **Month**: density and navigation overview

Default entry: Household scope, Day mode, today's date — the operational "what matters today" question.

Agenda must be:

- dense
- fast to scan
- truthful
- low-navigation
- write-capable in-place

## Lists

Lists are reusable household execution containers.

They answer:

> What should be remembered, bought, checked, prepared, or done next time?

Lists own household capture and flexible execution.
They are not a full task management system and do not replace Calendar.

A list item is not a task.
A list item may carry importance and temporal fields (due date, reminder, repeat).
Items with any temporal field project into the Agenda surface as a distinct entry type.
Temporal fields are independently optional — repeat does not require a due date.
A list linked to a plan remains a list. Linking does not cause all items to project.

Lists must be:

- grouped by context or purpose
- independent objects — able to exist without links to plans or areas
- persistent across uses
- reusable by design

Lists exist for:

- groceries
- packing
- preparation
- restocking
- school preparation with due dates
- any recurring household collection

The four axes of DomusMind must remain strictly separate:

| Surface | Owns |
| ------- | ---- |
| Agenda | Time (source of truth) |
| Tasks | Structured execution lifecycle |
| Areas | Ownership |
| Lists | Household execution container (capture → action → time reference) |

They should feel:

- fast
- compact
- row-based
- low-ceremony
- shared

## Areas

Areas are the ownership surface.

They answer:

> Which parts of household life exist, and who owns what?

They should make ownership visible without becoming administrative.

## Settings

Settings is the low-frequency configuration surface.

It answers:

> How do we manage people details, household preferences, and personal integrations without polluting operational surfaces?

It should feel:

- compact
- calm
- explicit
- secondary to operational surfaces

Settings is where member-scoped external calendar connections are managed in phase 1.
Agenda consumes the resulting read-only projections, but does not own connection setup.

---

# Agenda and the Household Timeline

DomusMind does not require the user to navigate between surfaces to understand household time.

The Agenda surface is the single temporal entry point.

Scope (Household or Member) and time mode (Day, Week, Month) are selections within one surface, not separate navigation destinations.

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
5. Show Agenda immediately in its default household day state.

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

- the household can understand Agenda's default day state in seconds
- Agenda week planning feels calm and fast
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