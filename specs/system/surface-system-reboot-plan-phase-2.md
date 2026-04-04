---
Status: Phase 2 Output — Planning
Audience: Engineering / Product
Produced: 2026-04-02
Depends on:
  - docs/00_product/surface-system.md
  - specs/system/surface-system-reboot-plan.md
  - specs/system/surface-system-reboot-plan-phase-0.md
  - specs/system/surface-system-reboot-plan-phase-1.md
  - specs/surfaces/planning.md
---

# Phase 2 — Planning

This document records the Phase 2 Planning migration.

It replaces the old Planning list-manager route with the new temporal workbench structure.

It does not claim that shared temporal primitives have been fully relocated yet.

---

## Goal

Turn `/planning` into the household temporal workbench.

Required outcomes:

- Week as default
- Day and Month as first-class views
- calendar canvas as the hero
- compact header and controls
- inspector on desktop
- bottom-sheet detail on mobile
- old tabbed list-manager removed from the route

---

## Scope

### In Scope

- new Planning page structure
- Planning header
- Planning day canvas
- Planning inspector content
- Planning CSS rewrite
- reuse of existing temporal components where needed
- desktop/mobile Planning behavior inside the Phase 1 shell

### Out of Scope

- Today cleanup
- Member Agenda redesign
- Shared temporal primitive relocation
- Shared Lists redesign
- Areas redesign
- full modal/editor redesign

---

## What Changed

### Replaced

```text
src/web/app/src/features/planning/pages/PlanningPage.tsx
src/web/app/src/features/planning/planning.css
````

### Created

```text
src/web/app/src/features/planning/components/PlanningHeader.tsx
src/web/app/src/features/planning/components/PlanningDayCanvas.tsx
src/web/app/src/features/planning/components/PlanningInspectorContent.tsx
```

### Updated

```text
specs/system/surface-system-reboot-plan.md
```

---

## Result

`/planning` is no longer a three-tab list manager.

It now behaves as a temporal workbench inside the new shell:

* Week view as default
* Day view using hourly timeline
* Month view for density/navigation
* compact Planning header
* desktop inspector with mini calendar + selected item detail
* mobile bottom-sheet detail
* existing add/edit flows reused where needed

---

## Reused Existing Temporal Pieces

Phase 2 reused existing working temporal pieces directly rather than physically moving them yet.

Reused from current locations:

* `today/api/weekApi.ts`
* `today/utils/dateUtils.ts`
* `today/hooks/useMonthGridCache.ts`
* `today/components/grid/WeeklyHouseholdGrid.tsx`
* `today/components/MonthView.tsx`
* `today/utils/todayPanelHelpers.ts`
* `agenda/components/AgendaMiniCalendar.tsx`
* `agenda/components/HourTimeline.tsx`
* `agenda/utils/agendaDateGrid.ts`

This keeps Phase 2 scoped and build-safe, but leaves shared-temporal cleanup for a later phase.

---

## Current Planning Structure

### Header

`PlanningHeader`

Contains:

* title
* add action
* date navigation
* today jump
* Week / Day / Month switch

### Main Canvas

* Week → `WeeklyHouseholdGrid`
* Day → `PlanningDayCanvas` using `HourTimeline`
* Month → `MonthView`

### Inspector

`PlanningInspectorContent`

Shows:

* mini calendar
* selected item detail

Desktop uses `InspectorPanel`.
Mobile uses `BottomSheetDetail`.

---

## Legacy Removed From Route

The old `/planning` tabbed composition is no longer the active Planning surface.

Legacy components remain in repo for now:

* `PlansTab.tsx`
* `RoutinesTab.tsx`
* `TasksTab.tsx`

They are not part of the new Planning route.

---

## Acceptance Check

### Week is the default view

Met.

### Day and Month exist as first-class views

Met.

### Calendar canvas is the hero

Met structurally.

### Selected detail is available without full-page navigation by default

Met.

### Desktop feels like a temporal workbench inside the new shell

Met at structural level.

### Mobile preserves the same Planning logic in collapsed form

Met.

### No unrelated surface was redesigned

Met.

### Build remains clean

Met.

Result:
**Phase 2 is complete.**

---

## Known Debt

### Shared temporal primitives are still not relocated

Planning now imports working temporal pieces from Today and Agenda.

This is acceptable for now, but still debt.

### Today still carries redundant calendar responsibility

Planning now owns the temporal workbench role.
Today cleanup is still pending for Phase 4.

### Member Agenda still uses feature-local temporal pieces

That formal cleanup remains for Phase 5 or a dedicated shared-temporal consolidation pass.

---

## Next Phase Dependency

Phase 3 can now begin.

Shared Lists can build on:

* the new shell
* the compact header/control model
* inspector + bottom-sheet patterns
* denser page composition already proven by Planning

---

## Summary

Phase 2 completed the Planning migration.

The product now has:

* a real Planning workbench
* week/day/month temporal structure
* compact header and controls
* inspector-based desktop detail
* mobile contextual detail
* removal of the old Planning tab-manager from the active route

The route is structurally aligned with the Planning surface spec.
The remaining debt is mostly about temporal primitive consolidation, not about the Planning surface direction itself.
