Status: Working Plan
Audience: Product / Design / Engineering
Scope: Execution plan for the current UX reboot
Owns: Migration order, implementation phases, reusable primitives, acceptance gates, and page-by-page transition strategy
Depends on:
- docs/00_product/strategy.md
- docs/00_product/experience.md
- docs/00_product/surface-system.md
- specs/surfaces/planning.md
- specs/surfaces/shared-lists.md
- specs/surfaces/today.md
- specs/surfaces/member-agenda.md
- specs/surfaces/areas.md

## Phase status

| Phase | Status | Output |
|-------|--------|--------|
| Phase 0 — Audit and Mapping | **Complete** | `specs/system/surface-system-reboot-plan-phase-0.md` |
| Phase 1 — Shell Foundation | **Complete** | `specs/system/surface-system-reboot-plan-phase-1.md` |
| Phase 2 — Planning | **Complete** | `specs/system/surface-system-reboot-plan-phase-2.md` |
| Phase 3 — Shared Lists | **Complete** | `specs/system/surface-system-reboot-plan-phase-3.md` |
| Phase 4 — Today | **Complete** | `specs/system/surface-system-reboot-plan-phase-4.md` |
| Phase 5 — Member Agenda | Not started | — |
| Phase 6 — Areas | Not started | — |

# DomusMind - Surface System Reboot Plan

This document translates the new product and UX documentation into an execution plan.

It exists to prevent the reboot from degrading into isolated screen edits, local styling fixes, or page-by-page design drift.

The goal is not to make individual pages prettier.

The goal is to turn DomusMind into one coherent product shell with multiple operational surfaces inside it.

---

# Purpose

The reboot exists to move DomusMind toward:

- one coherent app shell
- stronger layout discipline
- denser information
- quieter styling
- desktop-first split views
- inspector/sidebar over modal chaos
- real hierarchy over floating cards
- neutral base with restrained accents
- mobile collapse of the same product, not a redesign

This plan defines what gets built first, what gets replaced, what gets deleted, and how progress is measured.

---

# Product and UX Doctrine

The implementation must preserve the current product truth:

- household-first, not person-first
- action-first
- timeline-first
- one product, one language
- capture must stay easier than remembering
- dense operational surfaces
- content dominates chrome

The reboot must also preserve the shared surface doctrine:

## Surface Mantra

1. The content is the hero.
2. One row = one stateful thing.
3. Capture is always available.
4. Detail is secondary.
5. Counts are visible.
6. Themes create attachment.
7. Density is high without feeling hostile.

## Shell Principles

1. App shell first.
2. Strong layout discipline.
3. Dense information.
4. Quieter styling.
5. Desktop-first split views.
6. Inspector/sidebar over modal chaos.
7. Real hierarchy over floating cards.
8. Neutral base with restrained accents.
9. Mobile is a collapse of the same product, not a redesign.

These points are not optional style preferences.
They are the operating rules of the reboot.

---

# Scope

This reboot applies to the major operational surfaces:

- Planning
- Shared Lists
- Today
- Member Agenda
- Areas

It also applies to the shared cross-surface shell and interaction system used by those surfaces.

This plan does not redefine domain behavior.
Domain truth remains in the context docs and feature/read-model specs.

---

# Non-Goals

This reboot does not aim to:

- redesign the product strategy
- change household language
- invent new core domains
- turn Lists into a task manager
- turn Planning into a generic calendar app
- turn Today into a dashboard
- create separate desktop and mobile product models
- fix every old UI issue through local patches

This is a structural reboot, not a cosmetic pass.

---

# Execution Strategy

The work must proceed in this order:

1. shell and primitives
2. planning
3. shared lists
4. today
5. member agenda
6. areas

This order is intentional.

It starts with the shared UX system, proves the composition model on the richest temporal surface, then extends it to the other surfaces using the same grammar.

---

# Phase 0 - Audit and Mapping

Before implementation begins, create a current-to-target mapping for the existing app.

For each major route or page, record:

- current route
- current page/component
- target surface spec
- keep / refactor / replace / delete
- shared primitive dependencies
- open domain/read-model dependency
- notes

## Required Mapping Targets

### Planning
Map all current planning/calendar pages, view switches, date navigation controls, detail flows, and creation flows.

### Shared Lists
Map all list index pages, list detail pages, quick-add flows, toggle interactions, and any existing modal chains.

### Today
Map all current home/today blocks, household summary sections, member cards, item interactions, and date navigation.

### Member Agenda
Map all person-specific timeline/calendar views, day/week/month modes, and item detail flows.

### Areas
Map all area/responsibility screens, list/grid views, owner assignment flows, and related-item navigation.

## Audit Output

The audit must answer:

- what is reusable
- what must be rewritten
- what must be deleted
- what should be replaced by shared primitives
- what old composition patterns must stop appearing

No implementation should begin until this mapping exists.

---

# Phase 1 - Shell Foundation

## Goal

Establish the shared product shell and cross-surface primitives.

## Deliverables

### 1. App Shell
Build the canonical shell with:

- left navigation rail
- compact page header
- central content canvas
- optional right contextual inspector

### 2. Mobile Shell Behavior
Build the collapsed mobile form with:

- top header
- primary content first
- compact controls
- contextual detail through bottom sheet or pushed section
- drawer or compact navigation where needed

### 3. Layout Primitives
Create reusable layout primitives such as:

- AppShell
- PageHeader
- CompactToolbar
- InspectorPanel
- BottomSheetDetail
- SurfaceSection
- SurfaceEmptyStateCompact

### 4. Navigation and Header Primitives
Create shared controls such as:

- NavRail
- DateNavigator
- ViewSwitch
- SearchFieldCompact
- FilterTabsCompact
- ContextChip
- PrimaryActionButton

### 5. Density and Visual Tokens
Lock the shared visual base:

- spacing scale
- component heights
- border rules
- radius rules
- neutral backgrounds
- accent usage
- typography hierarchy
- shadow rules

## Acceptance Criteria

Phase 1 is complete when:

- the app shell exists as a real reusable implementation
- at least one page can render inside it cleanly
- inspector and bottom sheet patterns are implemented
- layout tokens are centralized
- pages no longer need to invent their own structural frame

---

# Phase 2 - Planning Reboot

## Why Planning First

Planning is the best first proof of the new system because it exercises:

- shell composition
- dense toolbars
- date navigation
- view switching
- central temporal canvas
- mini calendar
- inspector-based detail
- desktop/mobile adaptation

Planning is the temporal workbench.
If the shell fails here, it will fail everywhere.

## Planning Targets

Implement Planning to match the surface spec:

- Week as default
- Day and Month as first-class views
- compact page header
- calendar canvas as hero
- right inspector for selected plan/date context
- bottom sheet or pushed detail on mobile
- compact controls
- no floating islands
- no decorative calendar wrapper

## Shared Primitives Proven Here

Planning should prove:

- AppShell
- PageHeader
- DateNavigator
- ViewSwitch
- InspectorPanel
- MiniCalendar
- calendar canvas layout
- selection -> inspector update flow

## Acceptance Criteria

Planning is complete when:

- the week view works inside the new shell
- day and month align with the same layout grammar
- selected plan detail does not require full page navigation
- the calendar dominates the page visually
- desktop feels efficient and calm
- mobile feels like the same product in a smaller frame

---

# Phase 3 - Shared Lists Reboot

## Why Shared Lists Second

Lists are the cleanest proof of:

- content over chrome
- row-based density
- quick add
- low-friction state change
- list switching
- contextual detail without navigation loss

They are structurally simpler than Planning but equally important to the reboot.

## Shared Lists Targets

Implement Shared Lists to match the surface spec:

- list switcher pane on desktop
- active list as the hero
- compact list header
- unchecked count visible
- completed items collapsed
- quick add always visible
- optional inspector for secondary item detail
- drawer/sheet-based switcher on mobile
- bottom-sheet detail on mobile

## Shared Primitives Proven Here

Shared Lists should prove:

- DenseListRow
- QuickAddBar
- ListSwitcher
- EmptyStateCompact
- InspectorPanel for item detail
- row toggle interaction
- completed-section compression pattern

## Acceptance Criteria

Shared Lists is complete when:

- list switching is fast
- adding multiple items feels trivial
- checking items is frictionless
- completed state does not dominate the view
- the active list feels like a working surface, not a showcase page
- desktop and mobile preserve the same product logic

---

# Phase 4 - Today Reboot

## Why Today Third

Today is the primary household read-first surface.

It should be rebuilt only after the shell, density rules, and inspector pattern are already stable.

Today must stop behaving like a dashboard or a large-card landing page.

## Today Targets

Implement Today to match the surface spec:

- standard shell
- compact date header
- household row
- member rows
- dense item grammar
- strict item ordering
- max 2 items in collapsed state
- `+N` expansion
- one expanded member at a time
- inspector on desktop
- bottom sheet on mobile
- `No date (N)` as secondary entry

## Shared Primitives Proven Here

Today should prove:

- dense summary rows/blocks
- expansion in place
- selected-item inspection without navigation loss
- low-noise high-density read layout
- date navigation consistency

## Acceptance Criteria

Today is complete when:

- the day is understandable in under a few seconds
- the page feels dense without feeling hostile
- no giant decorative blocks remain
- member expansion preserves context
- detail stays secondary
- the surface clearly feels like the same product as Planning and Lists

---

# Phase 5 - Member Agenda Reboot

## Why Member Agenda Fourth

Member Agenda should inherit the shell and temporal patterns already proven in Planning, while narrowing the scope to one person.

This avoids reinventing temporal layout logic in a second place.

## Member Agenda Targets

Implement Member Agenda to match the surface spec:

- standard shell
- compact person/date header
- Day default
- Week and Month aligned with the same grammar
- central time canvas/grid
- right inspector on desktop
- bottom sheet or pushed detail on mobile
- person-focused context
- no detached personal-productivity styling

## Shared Primitives Proven Here

Member Agenda should reuse:

- DateNavigator
- ViewSwitch
- InspectorPanel
- MiniCalendar
- timeline/grid primitives
- compact surface header
- item selection patterns

## Acceptance Criteria

Member Agenda is complete when:

- one person’s day is understandable instantly
- week and month preserve the same product logic as Planning
- item detail remains contextual
- the surface feels like DomusMind, not a separate app
- temporal density stays calm and readable

---

# Phase 6 - Areas Reboot

## Why Areas Last

Areas is structurally simpler and should benefit from the primitives already proven across the more demanding surfaces.

This keeps the reboot focused on the highest-leverage surfaces first.

## Areas Targets

Implement Areas to match the surface spec:

- standard shell
- compact page header
- dense list-first ownership surface
- clear owner/support display
- ownership-gap cues
- right inspector on desktop
- bottom-sheet or pushed detail on mobile
- no admin-console feeling
- no card dashboard as default

## Shared Primitives Proven Here

Areas should reuse:

- DenseListRow or dense summary row pattern
- InspectorPanel
- compact filters
- search placement
- EmptyStateCompact
- context chip patterns
- row selection patterns

## Acceptance Criteria

Areas is complete when:

- ownership gaps are obvious at a glance
- assign/change flows feel lightweight
- the surface stays calm and operational
- it looks like part of the same product family as Today, Planning, Lists, and Member Agenda

---

# Shared Primitive Backlog

The reboot should explicitly track reusable primitives.

## Core Shell
- AppShell
- NavRail
- PageHeader
- CompactToolbar
- InspectorPanel
- BottomSheetDetail

## Navigation and Context
- DateNavigator
- ViewSwitch
- SearchFieldCompact
- FilterTabsCompact
- ContextChip
- BreadcrumbOrBackPath

## Data Display
- DenseListRow
- DenseSummaryRow
- QuickAddBar
- EmptyStateCompact
- CollapsedSectionHeader
- CountBadge

## Temporal
- MiniCalendar
- CalendarCanvas
- DayTimeline
- WeekGrid
- MonthGrid
- CurrentTimeMarker

These should be built intentionally as cross-surface primitives.
Do not let pages hardcode variants that should be shared.

---

# Destructive Cleanup Rules

The reboot must remove bad patterns, not just layer new ones on top.

Delete or replace these when encountered:

- giant centered islands
- page-as-a-pile-of-cards composition
- cards inside cards inside cards
- oversized decorative headers
- modal chains on desktop for simple inspection
- page-specific visual languages
- excessive empty padding
- chrome heavier than content
- full-page navigation for shallow detail
- separate mobile product concepts

A screen is not “migrated” if it still depends on these patterns.

---

# Delivery Model

## Branch Strategy

Use one main branch for the reboot:

`feat/surface-system-reboot`

## Execution Style

Work in vertical milestones, but never bypass the shared shell.

The shell and primitives come first.
Pages come second.

## PR Strategy

Prefer PRs shaped like this:

1. shell/tokens/primitives
2. planning migration
3. shared lists migration
4. today migration
5. member agenda migration
6. areas migration

Do not mix unrelated domain work into these PRs unless required to make the surface function.

---

# Definition of Done

A surface is only considered migrated when:

- it uses the standard shell
- it follows the shared layout grammar
- it uses the shared visual tone
- it follows the inspector/modal/page-navigation rules
- desktop and mobile preserve the same product logic
- old anti-patterns are removed
- its surface spec acceptance criteria are met

A page with new colors inside old composition is not done.

A page with some new components but old interaction chaos is not done.

A page with a new header and the same floating-card layout is not done.

---

# Risks

## 1. Cosmetic drift
Risk:
Teams patch styling without changing structure.

Response:
Review against shell, layout, and interaction rules before merge.

## 2. Page-level reinvention
Risk:
Each surface invents local patterns.

Response:
Block this in review.
Shared primitives must come first.

## 3. Desktop/mobile divergence
Risk:
Mobile becomes a different product.

Response:
Keep the same surface logic and only collapse the frame.

## 4. Legacy coexistence for too long
Risk:
Old and new composition models live together and muddy the system.

Response:
Prefer replacing complete routes/surfaces instead of half-migrated hybrids.

## 5. Modal relapse
Risk:
Desktop keeps using modals for shallow inspection.

Response:
Use inspector by default unless the flow is truly interruptive.

---

# Milestones

## Milestone 1
Shell foundation merged.
Reusable primitives exist.
One page can render correctly inside the new shell.

## Milestone 2
Planning migrated.
Calendar workbench pattern proven.

## Milestone 3
Shared Lists migrated.
Dense list pattern and quick-add pattern proven.

## Milestone 4
Today migrated.
Primary household read-first surface proven.

## Milestone 5
Member Agenda migrated.
Person-focused temporal surface aligned with Planning.

## Milestone 6
Areas migrated.
Ownership surface aligned with the same product language.

---

# Success Criteria

The reboot is successful when:

- all major surfaces feel like one product
- the shell is consistent across modules
- content dominates chrome
- inspectors reduce navigation cost
- density improves without hostility
- desktop feels efficient and calm
- mobile feels like the same product in a smaller frame
- no surface looks like an isolated design experiment

---

# Summary

DomusMind is moving from isolated pages to one coherent operational product shell.

This reboot succeeds only if:

- the shell is shared
- the primitives are shared
- the migration order is respected
- bad patterns are actively removed
- every surface is judged against the same product and UX doctrine

The target is not prettier pages.

The target is a coherent household operating system.