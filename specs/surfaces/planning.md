# Surface Spec - Planning

## Purpose

Provide the household's primary write-heavy temporal coordination surface.

Planning answers:

- what is happening
- when it happens
- who is involved
- what needs adjustment
- where the week or day is overloaded

It is the main place to create, inspect, and adjust plans in time.

---

## Depends On

- `docs/00_product/experience.md`
- `docs/00_product/surface-system.md`
- `docs/04_contexts/calendar.md`

---

## Entry Points

- main navigation → `Planning`
- deep link from Today
- deep link from Member Agenda
- date jump from other temporal surfaces

---

## Role

Planning is the household's temporal workbench.

It is:

- temporal first
- shared
- inspection-friendly
- adjustment-oriented

It is not:

- a decorative calendar page
- a separate personal planner
- a dense task board
- a reporting screen

Planning is the surface for time-bound household coordination.
It does not become the main surface for task execution, list management, or area administration.

---

## Shell

Planning uses the standard product shell.

Desktop:

- left navigation rail
- compact page header
- central calendar canvas
- right contextual inspector

Mobile:

- same product logic
- collapsed layout
- detail through bottom sheet or pushed section

---

## Default View

Week is the default Planning view.

It is the primary overview for household coordination because it balances:

- temporal clarity
- load visibility
- fast scanning
- lightweight adjustment

Day and Month remain first-class views, but Week is the default entry state.

---

## Layout

### Header

Contains:

- page title
- current date range
- compact date navigation
- view switch
- primary action: `Add plan`
- search when needed

### Main Canvas

Contains the selected temporal view:

- Week
- Day
- Month

The calendar canvas is the hero.

### Inspector

Shows the currently selected plan or date context.

Used for:

- plan detail
- participant summary
- reminder summary
- recurrence summary
- lightweight editing
- related context

---

## Data

Planning may show:

- plans
- participants
- reminders
- recurrence
- unavailability blocks
- lightweight coordination cues from tasks or routines where relevant

Planning does not become the canonical editing surface for:

- shared lists
- area ownership
- task-only management

These may appear as contextual cues, not as the main content model.

---

## Views

### Week

Purpose:

- understand the week's shape quickly
- inspect distribution of household commitments
- spot overloaded days and coordination pressure

Structure:

- days as columns
- time as rows
- plans as blocks
- optional all-day lane
- optional lightweight coordination cues
- selected plan opens in inspector

Week is the default coordination view.

### Day

Purpose:

- inspect one day in time
- place and inspect plans precisely
- understand gaps and collisions

Structure:

- hourly vertical timeline
- all-day section at top
- timed plans in the main grid
- current-time indicator
- optional subtle contextual cues
- selected plan opens in inspector

Day should feel like a quiet scheduling workbench.

### Month

Purpose:

- scan temporal density
- understand pattern and distribution
- jump quickly into a target day or week

Structure:

- compact calendar grid
- density indicators
- selected day context
- day or plan selection updates inspector

Month is navigation-first, not detail-first.

---

## Interaction

### Primary Actions

- add a new plan
- select a plan
- move in time
- switch view
- inspect detail
- edit lightweight fields
- open full edit flow when needed

### Navigation

- previous / next date range
- jump to today
- switch Week / Day / Month
- select date from mini calendar where present

### Selection

- selecting a plan updates the inspector
- selecting empty time may prepare creation state
- selection should not destroy context

### Creation

Desktop:

- add from primary action
- add from empty slot
- add from selected date

Mobile:

- add from compact primary action
- add from selected date or slot where appropriate

### Editing

Lightweight edits may happen in the inspector.

Examples:

- rename
- reschedule
- adjust reminder
- change participants

Larger edit flows may open a dedicated sheet or page.

---

## Visual Rules

Planning should use:

- neutral base surfaces
- subtle borders
- restrained accent usage
- dense toolbars
- compact date controls
- strong typographic hierarchy

Planning should avoid:

- oversized empty headers
- decorative cards around the grid
- floating islands on large screens
- loud color usage without meaning
- modal-first desktop interaction

---

## Mobile Behavior

Mobile keeps the same product logic with collapsed layout.

Structure:

- top header
- compact controls
- main calendar content
- detail through bottom sheet or pushed section

Rules:

- content first
- detail second
- no persistent desktop-style right inspector
- no redesign into a different product concept

Week, Day, and Month remain the same conceptual views.

---

## Relationship With Other Surfaces

### Today

- Today = read-first household snapshot
- Planning = write-heavy temporal workbench

### Member Agenda

- Member Agenda = one person's temporal surface
- Planning = shared household temporal surface

### Lists

- Lists may connect contextually
- Lists do not become first-class scheduling items here

### Areas

- Areas may inform context
- Areas do not dominate planning layout

---

## Non-Goals

- no project-management complexity
- no reporting dashboard
- no list-centric editing surface
- no full task board inside Planning
- no decorative calendar shell
- no fragmented desktop/mobile behavior

---

## Success Criteria

Planning is successful when:

- the household can understand the week quickly
- adding or adjusting a plan feels direct
- selected details are visible without losing context
- the calendar canvas dominates the surface
- desktop feels efficient
- mobile feels like the same product
- the surface stays dense, calm, and readable