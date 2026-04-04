# Surface Spec - Today

## Purpose

Show the household day with minimum navigation and maximum truth.

Today must answer:

- what matters today
- who has something today
- what is overdue
- what is shared

---

## Depends On

- `docs/00_product/experience.md`
- `docs/00_product/surface-system.md`
- `docs/04_contexts/calendar.md`
- `docs/04_contexts/tasks.md`

---

## Role

Today is the primary read-first household surface.

It is:

- household-first
- dense
- truth-focused
- low-navigation

It is not:

- a dashboard
- a calendar editor
- a landing page
- a large-card showcase

---

## Shell

Today uses the standard product shell from `docs/00_product/surface-system.md`.

Desktop:

- left navigation rail
- compact page header
- main content canvas
- right inspector for selected item

Mobile:

- same surface logic
- stacked content
- detail in bottom sheet

---

## Layout

Today has three parts:

1. date header
2. household row
3. member rows

No hero section.
No metrics strip.
No decorative blocks.

---

## Data

### Household row

Contains only shared state:

- shared tasks
- unassigned tasks
- shared plans

### Member rows

May contain:

- overdue tasks
- tasks due today
- plans today
- routines today
- completed today in low emphasis

### Secondary entry

- `No date (N)` for unscheduled tasks

Unscheduled work must stay outside the main day flow.

Completed items are part of Today data, but low-priority and usually hidden in collapsed view.

---

## Item Grammar

Use compact row grammar:

- `!` overdue
- `□` task
- `● HH:mm` plan
- `⟳` routine
- `✓` completed

No legends.
No explanatory labels.

---

## Priority Order

Always show items in this order:

1. overdue
2. tasks due today
3. plans
4. routines
5. completed

---

## Default View

Each member appears as one compact row or block.

Collapsed state:

- show max 2 items
- preserve priority order
- summarize remaining items as `+N`

Examples:

```text
Juan
! □ Pay bill · ● 19:30 Dentist · +2
````

```text
Lucía
⟳ Trash
```

```text
Mateo
Nothing today
```

Completed items stay out of collapsed view unless they are the only visible state.

---

## Expanded View

Tap expands one member in place.

Expanded state shows the full ordered list.

Rules:

* only one member expanded at a time
* expand in place
* no navigation
* no modal
* collapse restores scan rhythm

---

## Interaction

* tap member row → expand/collapse
* tap item → open inspector on desktop
* tap item → open bottom sheet on mobile
* tap `+N` → expand
* tap `No date` → open unscheduled work
* change date → keep same surface structure

---

## Non-Goals

Today does not handle:

* full plan editing
* complex drag and drop
* multi-select workflows
* heavy inline editing
* analytics dashboards

---

## Success Criteria

Today succeeds when:

* the day is understandable in under 3 seconds
* no important item is hidden
* pressure appears before neutral information
* the page is dense without feeling noisy
* item detail does not break context
