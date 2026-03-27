Status: Canonical
Audience: Product / Design / Engineering / Marketing
Scope: V1 with future-direction notes
Owns: Product experience, household language, Today and Week behavior, onboarding shape, and scope guardrails
Depends on: docs/00_product/strategy.md, specs/system/system-spec.md
Replaces: docs/00_vision/household-experience.md, docs/00_vision/household-timeline.md, docs/00_vision/household-onboarding.md, docs/00_vision/calendar-coordination.md, docs/00_vision/chore-assignment.md

# DomusMind - Product Experience

This document is the canonical product-experience file for DomusMind.

It defines how the product should feel and behave in real household use.

---

# Core UX Principles

DomusMind should feel like a quiet shared household system.

The experience is guided by these principles:

- Reduce cognitive load. The system should remove remembering, reminding, and constant negotiation work.
- Action first. The product should answer what matters today before it asks for setup or interpretation.
- No configuration barrier. A household should reach useful operation in minutes.
- Household-first. The main view reflects the household's shared reality, not one person's productivity space.
- Timeline-first. Plans, routines, tasks, lists, and reminders should become legible together.
- Simplicity at the surface. Product complexity belongs in the model, not in the interface.

The experience should feel:

- quiet
- helpful
- predictable
- lightweight

The product should feel like a smart household board, not a management tool.

---

# Household Language

DomusMind must speak in natural household concepts.

The interface should use:

- household
- people
- plans
- routines
- tasks
- lists
- areas
- today
- what matters
- what needs attention
- who owns what

Internal model terms stay out of the experience whenever household language can express the same idea more naturally.

Core translations:

| Internal Model | Household Language |
| -------------- | ------------------ |
| Family | Household |
| Member | Person |
| Event | Plan |
| Responsibility | Area |
| Routine | Routine |
| Shared List | List |

Task stays Task by design.

---

# Home / Today

The home screen is the Today view.

It is the primary operational surface of the product.

It answers one question:

> What matters today for this household?

The screen should make the household state understandable in seconds.

Typical Today information includes:

- plans happening today
- routines affecting today
- tasks needing completion
- lists that matter today when relevant
- reminders needing attention

Example:

```
Today

09:00
Lucia dentist appointment

18:00
Mateo football practice

Trash -> Juan
Laundry -> Lucia

Buy milk
```

Users should not need separate tools to understand what is happening.

---

# Week View

The week view is a primary operational surface, not a secondary calendar page.

It should feel like a smart household whiteboard for the upcoming week.

The week view helps the household understand:

- where people need to be
- when commitments occur
- what routine and task work exists around those commitments
- which days are overloaded
- what needs preparation

This view may combine information from multiple sources, but it should still feel like one shared household picture.

The goal is fast situational awareness, not detailed schedule management.

---

# Planning vs Timeline

DomusMind distinguishes between planning surfaces and the timeline.

Planning is the write-heavy surface where the household creates or edits future work.

The timeline is the read-first operational surface where the current household state becomes legible.

Anything that affects the household should appear in the timeline, including:

- plans
- routines
- tasks
- reminders
- list state where explicitly relevant

These are different kinds of entries inside one household flow, not separate applications.

The timeline replaces the need to check multiple disconnected tools to understand the day.

---

# Scope Guardrails

The V1 experience must stay inside the current system scope.

- no separate routine manager
- no personal-productivity framing
- no setup-heavy first run
- no calendar-first experience
- no hidden ownership

The V1 product surface should stay grounded in Family, Responsibilities, Calendar, Tasks, and the unified household timeline.

---

# Onboarding

Onboarding must create a functional household system in minutes.

The flow should:

- minimize user input
- avoid technical concepts
- create immediate usefulness
- establish the household model quickly

The first-run sequence is:

1. Start household.
2. Name the household.
3. Add people.
4. Add the first plans, routines, or lists.
5. Show Today and Week immediately.

Example starting flow:

- create the household
- add the people the household coordinates
- add a few first plans or routines
- show those items in Today and Week immediately

At the end of onboarding, the household should already have a working shared system rather than an empty app shell.

---

# Routines

Routines are recurring household work.

DomusMind should reduce routine negotiation rather than create a routine-management hobby.

Core expectations:

- the system can rotate routines when appropriate
- some routines may have a default owner
- some work may stay open to anyone in the household
- completion should be simple
- missed routines should surface quietly with useful correction suggestions

Routines should appear in the timeline as part of household reality.

Users should not feel that they are opening a separate routine manager.

The household should simply know what is their responsibility today.

---

# Calendar Coordination

DomusMind does not treat the calendar as a separate product to manage.

It coordinates people and time.

The core question is:

> Who needs to be where, and when?

Plans may come from different sources, but the household should not have to care where a plan originated.

Creating a plan should stay simple and participant visibility must be first-class.

The product should clearly show:

- who is involved
- who must be present
- who may be affected

Recurring fixed-time activities belong to calendar coordination, not to tasks.

Examples include:

- football practice every Tuesday
- piano class every Thursday
- school every weekday

The system should also surface conflicts between plans and responsibilities so the household can make quick adjustments.

Calendar coordination should disappear as a tool and remain visible as shared household awareness.

---

# Future Direction

DomusMind should gradually move toward household autopilot.

Future behavior may include:

- noticing missed recurring work
- suggesting reminders or reassignment
- spotting patterns in household routines
- identifying overloaded periods early
- suggesting lightweight adjustments before stress appears

This direction must remain grounded in the same product model:

- structured household state
- shared visibility
- clear responsibility
- useful suggestions instead of magic claims

The product should evolve toward anticipation without losing its quiet, lightweight feel.