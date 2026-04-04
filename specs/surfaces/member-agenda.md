# Surface Spec - Member Agenda

## Purpose

Provide a focused temporal view for one person inside the household.

It answers:

- what this person has today
- what is coming this week
- where overload or conflicts exist
- what needs attention without losing household context

Member Agenda is the deep individual surface.
Today remains the household snapshot.

---

## Depends On

- `docs/00_product/experience.md`
- `docs/00_product/surface-system.md`
- `docs/04_contexts/calendar.md`
- `docs/04_contexts/tasks.md`

---

## Entry Points

- tap a person from Today
- tap a person from Planning
- direct navigation (future)

---

## Role

Member Agenda is:

- person-focused
- time-based
- inspection-first
- planning-capable
- part of the same product shell as Today and Planning

It is not:

- a separate app
- a personal productivity workspace
- a detached calendar tool

---

## Shell

Member Agenda uses the standard product shell.

Desktop:

- left navigation rail
- compact page header
- central work canvas
- right contextual inspector

Mobile:

- same product logic
- collapsed layout
- detail through bottom sheet or pushed section

---

## Default View

Day is the default entry view.

It is the clearest first state for person-level inspection because it supports:

- exact timing
- same-day clarity
- conflict visibility
- low-friction drill-in from Today

Week and Month remain first-class views for planning awareness and navigation.

---

## Header

The header should show:

- person identity
- selected date or range
- view switch
- previous / next navigation
- primary action: `Add entry`

The header must stay compact.

---

## Data

Member Agenda may show:

- overdue tasks
- tasks due on the selected date
- timed tasks
- plans involving the person
- routines affecting the person
- completed items in low emphasis
- unavailability blocks where relevant

The surface must stay focused on the selected person.

Shared household state should only appear when it materially affects that person.

---

## Views

### Day

Use when the user needs:

- exact timing
- detail inspection
- conflict visibility
- same-day planning

#### Structure

Day view uses a vertical time layout.

Top section:

- overdue tasks
- tasks without time
- compact carry-over items

Main section:

- hourly timeline
- timed plans
- timed tasks
- timed routine projections
- unavailability blocks

#### Behavior

- timed items appear in the timeline
- date-only tasks appear in the top section
- completed items remain visible with low emphasis
- overlapping items must be readable
- the current time marker is visible when relevant

#### Priorities

Day view must optimize for:

- fast scan
- clear order
- conflict visibility
- low-friction inspection

### Week

Use when the user needs:

- temporal load across the week
- day-to-day distribution
- upcoming pressure
- fast scanning

#### Structure

Week view shows:

- one column per day
- compact time-based or density-based layout
- lightweight blocks or markers for plans, tasks, and routines

#### Behavior

- days should be easy to compare
- overloaded days should be visually apparent
- switching day selection should be fast
- the selected day should update the inspector and related context

#### Priorities

Week view must optimize for:

- planning awareness
- load comparison
- next-few-days clarity

### Month

Use when the user needs:

- pattern scanning
- busy vs quiet days
- fast jump to a date

#### Structure

Month view uses a calendar grid.

Each day should show:

- density cues
- selected-state clarity
- minimal markers for relevant items

#### Behavior

- selecting a date updates the active context
- month view is navigation-first, not detail-first
- month view must stay readable even when density is high

#### Priorities

Month view must optimize for:

- pattern scanning
- quick navigation
- calm overview

---

## Interaction

Supported interactions:

- select date
- switch Day / Week / Month
- previous / next navigation
- tap item to inspect
- tap time slot to add entry
- tap `Add entry` from header
- inspect conflict or overlap
- navigate back to shared surfaces

Future interactions may include:

- drag or reschedule
- richer inline editing

These are not required for the initial reboot.

---

## Inspector

Desktop should prefer an inspector for selected item detail.

The inspector may show:

- title
- type
- time
- status
- notes
- participants
- related area
- linked list or related context when relevant

The inspector should support lightweight editing without breaking context.

Mobile should use a bottom sheet or pushed detail section.

---

## Visual Rules

Member Agenda must follow the shared surface system:

- dense content
- compact controls
- restrained chrome
- low wasted space
- strong hierarchy
- no floating-card composition
- no large decorative headers

The time surface is the hero.

---

## Responsive Rules

### Desktop

Prefer:

- central timeline or grid
- right inspector
- compact header controls
- visible date navigation

### Mobile

Prefer:

- stacked header and controls
- content-first layout
- inspector as bottom sheet
- preserved Day / Week / Month model
- same product logic in a reduced frame

---

## Relationship With Other Surfaces

### Today

- Today = household snapshot
- Member Agenda = one-person deep view

### Planning

- Planning = shared temporal coordination
- Member Agenda = individual temporal inspection

### Lists

Lists are not the main object here.
Only show linked or relevant list context when it helps the selected person's understanding.

### Areas

Areas may appear as supporting context.
They do not become the primary organizing surface here.

---

## Non-Goals

- no multi-person editing surface
- no separate personal-productivity mode
- no giant modal workflows
- no detached calendar product behavior
- no dashboard metrics
- no heavy configuration layer

---

## Success Criteria

The surface is successful when:

- a user understands one person's day in seconds
- the week can be scanned without cognitive effort
- conflicts are visible without hunting
- switching views feels natural
- the person-level view still feels like DomusMind, not a separate app
- detail is available without losing context