# Surface Spec - Areas

## Purpose

Provide a clear, compact surface for understanding household ownership.

Areas answer:

- which household areas exist
- who owns each one
- who supports each one
- where ownership is missing

This is an ownership surface, not a task board and not an admin console.

---

## Depends On

- `docs/00_product/experience.md`
- `docs/00_product/surface-system.md`
- `docs/04_contexts/responsibilities.md`

---

## Entry Points

- main navigation → `Areas`
- contextual links from Today, Planning, Lists, or item detail
- onboarding follow-up flow

---

## Role

Areas is the household ownership surface.

It exists to make accountability visible quickly without forcing configuration-heavy behavior.

It should feel:

- calm
- dense
- legible
- operational

It is not:

- settings
- permissions management
- enterprise administration
- a decorative dashboard

---

## Shell

Areas uses the standard product shell.

Desktop:

- left navigation rail
- compact page header
- central content canvas
- right contextual inspector on selection

Mobile:

- same surface logic
- collapsed layout
- detail through bottom sheet or pushed section

---

## Default View

Default view is a dense list of area summaries.

Each area row should show:

- area name
- primary owner
- support members if present
- optional linked counts:
  - plans
  - tasks
  - lists
- status cue if ownership is missing or partial

Participant detail does not belong in the default row.
If relevant, it belongs in the inspector or secondary detail.

Grid presentation is not the default for V1.
If introduced later, it is a secondary view, not the canonical layout.

---

## Layout

### Header

Contains:

- title: `Areas`
- primary action: `Add area`
- search
- optional compact filters

The header must stay compact.
No hero section.
No oversized intro block.

### Main List

The main content is a dense list of area rows.

Default ordering:

1. unowned areas
2. partially assigned areas
3. fully assigned areas
4. archived areas if shown

Secondary ordering within groups:

- manual order if supported
- otherwise alphabetical

This keeps gaps visible first.

### Inspector

The inspector is the default desktop detail pattern.

It may show:

- area name
- primary owner
- support members
- participants if relevant
- linked plans
- linked tasks
- linked lists
- rename action
- assign or transfer actions
- archive action

The inspector should support quick edits without breaking context.

---

## Data

Areas may show:

- area name
- owner
- support members
- linked counts for plans, tasks, or lists
- ownership-gap status

Participant detail is secondary.
Show it in the inspector or secondary metadata only when it materially helps understanding.

Search may match:

- area name
- owner name
- support member name

Compact filters may include:

- all
- unowned
- mine
- active
- archived

Optional future filter:

- by member

Counts are cues, not the main content.

---

## Interaction

Supported interactions:

- add area
- rename area
- assign primary owner
- add or remove support
- open linked items
- archive area

Adding an area should stay lightweight.

Required fields:

- area name

Optional fields:

- primary owner
- support members

Ownership editing rules:

- one clear primary owner field
- support handled separately
- no role jargon in the UI
- no permissions language
- no dense form walls

Use household language:

- owner
- support

Desktop:

- selecting an area opens the inspector

Mobile:

- selecting an area opens a bottom sheet or pushed detail section

Do not navigate to a separate full page by default.

---

## Visual Rules

Areas should use dense, restrained summaries.

Preferred patterns:

- row-based summaries
- visible names
- clear owner/support display
- subtle status cues for missing ownership

Avoid:

- card-first layouts
- big empty cards
- decorative charts
- oversized avatars
- isolated panels with little information

---

## Mobile Behavior

Mobile should preserve the same structure.

Expected behavior:

- compact list of areas
- visible `Add area` action
- filters near header
- tap area -> detail sheet or section
- no separate mobile-only product logic

---

## Relationship with Other Surfaces

### Today

- Today may show area-linked cues
- Areas owns the ownership overview

### Planning

- Planning may reference areas
- Areas does not become a calendar

### Lists

- Lists may reference areas
- Areas is not a list manager

### Tasks

- Tasks may use areas for categorization
- Areas is not a task board

Areas is the ownership surface inside the larger household system.

---

## Non-Goals

- no detailed planning
- no task execution
- no calendar management
- no analytics
- no permissions administration
- no household reporting

---

## Success Criteria

The surface succeeds when:

- a household can understand ownership gaps in seconds
- missing ownership is obvious
- assigning or changing ownership is lightweight
- the surface feels integrated with the rest of DomusMind
- areas help reduce ambiguity without adding admin burden