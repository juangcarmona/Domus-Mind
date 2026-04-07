---
Status: Phase 5 Output — Agenda Unification
Audience: Engineering / Product
Produced: 2026-04-06
Depends on:
  - docs/00_product/surface-system.md
  - specs/system/surface-system-reboot-plan.md
  - specs/surfaces/agenda.md
---

# Phase 5 — Agenda Unification

This document defines the Phase 5 implementation work.

Phase 5 unifies Today, Planning, and Member Agenda into a single `features/agenda/` feature under a single `AgendaPage` with shared infrastructure.

It does not change the backend domain.
It does not change the weekly grid API.
It is a frontend structural consolidation with one new navigation entry.

---

## Goal

Produce a single Agenda surface that:

- replaces `features/today/`, `features/planning/`, and the current `features/agenda/` as three separate surfaces
- uses a single page component with scope and mode selectors
- defaults to Household + Day + today (what Today was)
- supports Household + Week/Day/Month and Member + Week/Day/Month
- owns the weekly grid API call and all temporal display logic once
- removes all duplication of `findGridItem`, `calendarEntry` logic, and month cache hook

---

## Scope

### In Scope

- unify `features/today/`, `features/planning/`, `features/agenda/` into a single `features/agenda/` feature
- single `AgendaPage.tsx` entry point
- scope selector: Household | member list
- mode toggle: Day | Week | Month
- Day > Household → Board layout (from `today/components/board/`)
- Day > Member → Timeline layout (from `agenda/components/MemberDayView` + `HourTimeline`)
- Week → `WeeklyHouseholdGrid` (household) or `MemberWeekView` (member)
- Month → `MonthView` (household) or `MemberMonthView` (member)
- consolidated `AgendaHeader` replacing both `AgendaHeader` and `PlanningHeader`
- single `useAgendaMonthCache` replacing `useMonthGridCache` and `useAgendaMonthCache`
- single `findGridItem` utility (pure function, lives in `features/agenda/utils/`)
- move `weekApi` from `features/today/api/` to `features/agenda/api/`
- move `calendarEntry.ts`, `todayPanelHelpers.ts`, `dateUtils.ts` to `features/agenda/utils/`
- move `CalendarEntryItem.tsx` to `features/agenda/components/shared/`
- routes: `/agenda`, `/agenda?mode=week`, `/agenda?mode=month`, `/agenda/members/:id` with optional `?date=` and `?mode=`
- legacy redirects: `/` → `/agenda`, `/planning` → `/agenda?mode=week`
- navigation rail updated: Today and Planning entries removed, Agenda added

### Out of Scope

- domain changes
- API changes
- backend changes
- new display logic not already present in the three surfaces
- drag-and-drop
- unavailability blocks (backend gap)

---

## Files Changed

### New / Replaced

```
src/web/app/src/features/agenda/pages/AgendaPage.tsx         ← single unified page
src/web/app/src/features/agenda/components/AgendaHeader.tsx  ← consolidated header
src/web/app/src/features/agenda/api/weekApi.ts               ← moved from today/api/
src/web/app/src/features/agenda/utils/calendarEntry.ts       ← moved from today/utils/
src/web/app/src/features/agenda/utils/calendarEntry.test.ts
src/web/app/src/features/agenda/utils/todayPanelHelpers.ts   ← moved from today/utils/
src/web/app/src/features/agenda/utils/todayPanelHelpers.test.ts
src/web/app/src/features/agenda/utils/dateUtils.ts           ← moved from today/utils/
src/web/app/src/features/agenda/utils/agendaGridItem.ts      ← replaces duplicate findGridItem
src/web/app/src/features/agenda/hooks/useAgendaMonthCache.ts ← replaces both cache hooks
src/web/app/src/features/agenda/components/shared/CalendarEntryItem.tsx
src/web/app/src/features/agenda/agenda.css                   ← consolidated styles
```

### Updated

```
src/web/app/src/router.tsx              ← updated routes + legacy redirects
src/web/app/src/components/NavRail.tsx  ← Agenda entry replaces Today + Planning
```

### Deprecated (do not delete until all imports drained)

```
src/web/app/src/features/today/        ← drain imports; mark as deprecated
src/web/app/src/features/planning/     ← drain imports; mark as deprecated
```

---

## Acceptance Criteria

- `/agenda` loads the Household Day Board (what Today was) for today's date
- `/agenda?mode=week` loads the Household Week grid
- `/agenda?mode=month` loads the Household Month grid
- `/agenda/members/:id` loads the Member Day Timeline for that member
- switching scope preserves current date and mode
- switching mode preserves current scope and date
- tapping a day in Month mode switches to Day mode for that date
- inline add opens with current scope + date as defaults
- inspector opens on item selection on desktop
- bottom sheet opens on item selection on mobile
- nav rail shows `Agenda`, not `Today` or `Planning`
- navigating to `/` or `/planning` redirects correctly
- existing tests in `calendarEntry.test.ts` and `todayPanelHelpers.test.ts` pass at new paths
- `npm run build` passes with no errors

---

## Implementation Notes

### Header design

The unified `AgendaHeader` replaces both `AgendaHeader` (agent-specific) and `PlanningHeader`.

It contains:
- scope selector as first row or pill
- mode toggle: Day | Week | Month
- date navigation: < [date label] >
- today shortcut
- add action

Keep the existing `AgendaHeader` component name. Remove `PlanningHeader` once replaced.

### Board variant vs Timeline variant

`AgendaPage` selects display based on a derived variable:

```
const displayMode = scope === "household"
  ? mode                    // "day" | "week" | "month"
  : `member-${mode}`        // "member-day" | "member-week" | "member-month"
```

Or simply branch on `isShared && mode`:

```
isShared + day   → <TodayBoard />
isShared + week  → <WeeklyHouseholdGrid />
isShared + month → <MonthView />
member  + day    → <MemberDayView />
member  + week   → <MemberWeekView />
member  + month  → <MemberMonthView />
```

No new display logic. These components already exist.

### URL strategy

Scope is encoded in the path: `/agenda` vs `/agenda/members/:id`.
Mode and date are query params: `?mode=week&date=2026-04-06`.

This allows deep links to work cleanly and preserves browser history for back navigation.

### i18n

Rename the `today` and `planning` i18n namespaces to `agenda`.
Merge the keys and remove duplicates.

Update the namespace reference in `AgendaPage` and all descended components.
