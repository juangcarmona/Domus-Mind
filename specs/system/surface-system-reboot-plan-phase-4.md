---
Status: Phase 4 Output — Today
Audience: Engineering / Product
Produced: 2026-04-04
Depends on:
  - docs/00_product/surface-system.md
  - specs/system/surface-system-reboot-plan.md
  - specs/system/surface-system-reboot-plan-phase-0.md
  - specs/system/surface-system-reboot-plan-phase-1.md
  - specs/system/surface-system-reboot-plan-phase-2.md
  - specs/system/surface-system-reboot-plan-phase-3.md
  - specs/surfaces/today.md
---

# Phase 4 — Today

This document records the Phase 4 Today migration.

It refines and corrects the existing Today implementation rather than rebuilding it from scratch.

It does not redesign Planning, Shared Lists, Member Agenda, or Areas.

---

## Goal

Make `/` the real Today surface:

- household-first daily snapshot
- dense, calm, low-navigation
- remove the mid-term calendar/timeline burden that Planning now owns
- make member expansion work in place
- wire up inspector/bottom-sheet for item detail

---

## Scope

### In Scope

- removal of mid-term week/month section from Today
- removal of `TimelineRuler` from Today
- cleanup of dead `coord-*` CSS naming in Today code/styles
- in-place member expansion (single expanded at a time)
- `+N` overflow trigger for expansion
- item detail via `InspectorPanel` on desktop, `BottomSheetDetail` on mobile
- `l-surface` layout alignment with the Phase 1 shell
- Today CSS composition cleanup

### Out of Scope

- Planning, Shared Lists, Member Agenda, Areas redesign
- new dashboard metrics or analytics
- unscheduled work / `No date (N)` — backend gap, not fabricated
- full editor redesign (`EditEntityModal` remains, accessed via inspector/sheet)
- drag-and-drop complexity
- domain/read-model changes
- shared temporal primitive relocation

---

## What Changed

### Replaced / Reworked

```text
src/web/app/src/features/today/pages/TodayPage.tsx
src/web/app/src/features/today/components/board/TodayBoard.tsx
src/web/app/src/features/today/components/board/TodayMemberCell.tsx
src/web/app/src/features/today/today-shell.css
src/web/app/src/features/today/today-board.css
src/web/app/src/features/today/today-month.css
```

### Updated (translation keys added)

```text
src/web/app/src/i18n/locales/en/today.ts
src/web/app/src/i18n/locales/es/today.ts
src/web/app/src/i18n/locales/fr/today.ts
src/web/app/src/i18n/locales/de/today.ts
src/web/app/src/i18n/locales/it/today.ts
src/web/app/src/i18n/locales/ja/today.ts
src/web/app/src/i18n/locales/zh/today.ts
specs/system/surface-system-reboot-plan.md
```

### Preserved Unchanged

```text
src/web/app/src/features/today/utils/todayPanelHelpers.ts
src/web/app/src/features/today/utils/calendarEntry.ts
src/web/app/src/features/today/utils/calendarEntry.test.ts
src/web/app/src/features/today/utils/todayPanelHelpers.test.ts
src/web/app/src/features/today/components/shared/CalendarEntryItem.tsx
src/web/app/src/features/today/components/MonthView.tsx
src/web/app/src/features/today/components/grid/WeeklyHouseholdGrid.tsx
src/web/app/src/features/today/components/timeline/TimelineRuler.tsx
src/web/app/src/features/today/api/weekApi.ts
src/web/app/src/features/today/utils/dateUtils.ts
src/web/app/src/features/today/hooks/useMonthGridCache.ts
src/web/app/src/features/today/today-week.css
src/web/app/src/features/today/today-ruler.css
```

---

## Result

`/` is now the real Today surface.

- `TodayPage` is much smaller and focused on day-only data and interaction
- Mid-term week/month canvas is gone from Today; Planning owns it
- `TimelineRuler` is gone from Today's active route
- `TodayBoard` remains the center of the surface
- Member rows expand in place; only one expanded at a time
- `+N` badge triggers expansion
- Left zone (avatar + name) still navigates to Member Agenda
- Tapping an item opens the inspector (desktop) or bottom sheet (mobile)
- From inspector/sheet, user can click Edit to open `EditEntityModal`
- Layout uses `l-surface` / `l-surface-body` / `l-surface-content` aligned with the Phase 1 shell
- Dead `coord-*` CSS classes removed or renamed to `today-board-*`

---

## Key Implementation Decisions

### Mid-term burden removed

`TodayPage` previously rendered `WeeklyHouseholdGrid` (week view), `MonthView` (month view), and `TimelineRuler` as sections 2 and 3 below the day board.

These are all removed. The components remain in the repo for Planning's use.

Related state removed: `midTermView`, `monthAnchor`, `useMonthGridCache`, `fetchTimeline`, timeline selectors, `handleDaySelect`, `handlePrevWeek`, `handleNextWeek`, `weekNavLabel`.

### In-place expansion with single-expanded invariant

`TodayBoard` now holds `expandedMemberId` state.

`handleMemberToggle` collapses the previously expanded member before expanding the new one.

`TodayMemberCell` receives `isExpanded` and `onToggle` props.

Collapsed state: shows `visibleCollapsed` (max 2 active items) + `+N` overflow badge.
Expanded state: shows all `activeItems`, then `completedItems` at low emphasis below a separator.

The `splitForDisplay` behavioral contract in `todayPanelHelpers.ts` was not changed.

### Item-tap vs row-tap distinction

Tapping an entry item (`.wg-item`) calls `onItemClick` (opens inspector/sheet).  
Tapping empty space in the right zone calls `onToggle` (expands/collapses).  
Tapping `+N` also calls `onToggle`.

The right-zone click handler uses `event.target.closest('.wg-item')` to distinguish item taps from zone taps.

### Pragmatic inspector/sheet wiring

`handleItemClick` in `TodayPage` calls `findGridItem` to locate the selected item.

Desktop: `InspectorPanel` appears only when `selectedItem` is set. On close or after edit, it hides.  
Mobile: `BottomSheetDetail` opens when `selectedItem` is set.

`TodayItemDetail` (defined in `TodayPage.tsx`) shows glyph + type, title, date/time, status, subtitle, and an Edit button.

Clicking Edit opens `EditEntityModal`. After save, the grid refetches and `selectedItem` is cleared.

This is the pragmatic intermediate. The inspector stays lightweight; it does not replace the editor.

### CSS cleanup

`today-shell.css` was rewritten to replace `coord-*` names with `today-board-*` names and remove all dead mid-term section styles.

`today-board.css` removed: `.coord-page`, `.coord-controls`, `.coord-view-tabs`, `.coord-tab`, `.coord-date-nav`, `.coord-date-label`, `.coord-today-btn`, `.coord-content`, `.coord-day-view`, `.coord-day-header`, `.coord-day-title`, `.coord-day-sections`, `.coord-day-member-*`, `.coord-day-empty-member`.

`today-month.css` removed: the `coord-midterm-section--mobile` override block (dead code after removal of mid-term section from Today).

`today-week.css` and `today-ruler.css` were not changed (still used by Planning components).

---

## Known Debt

### Unscheduled work not implemented

The `No date (N)` secondary entry from the surface spec requires backend support not currently available.

This is a pre-existing gap documented in `todayPanelHelpers.ts`. No fake data was introduced.

### Editor is still modal-based

`EditEntityModal` is still the editor for Today items. The inspector/sheet acts as a summary + entry point.

A full inline editor or deeper inspector would be a later pass, not Phase 4 scope.

### `today-week.css` `today-summary-*` classes are now orphaned for TodayBoard

`TodayBoard` no longer uses `today-summary` or `today-summary-header` class names. These styles remain in `today-week.css` but are unused by Today. They are left in place because `today-week.css` is a shared file; a later cleanup pass could remove them.

---

## Acceptance Check

### Today no longer renders mid-term calendar content

Met. `WeeklyHouseholdGrid`, `MonthView`, and `TimelineRuler` are gone from the active Today route.

### Member expansion works in place

Met. Single-expanded invariant is enforced in `TodayBoard`.

### Max 2 collapsed items with `+N` expansion

Met. `splitForDisplay` drives collapsed display. `+N` badge triggers `onToggle`.

### Item detail does not require full-page navigation

Met. Inspector on desktop, bottom-sheet on mobile.

### Desktop and mobile preserve the same Today logic

Met.

### No unrelated surface was redesigned

Met.

### Build remains clean

Met. TypeScript and Vite build pass without errors.

Result:
**Phase 4 is complete.**

---

## Next Phase Dependency

Phase 5 can now begin.

Member Agenda can build on:

- the shared shell
- the inspector and bottom-sheet patterns
- the dense surface model proven in Planning, Lists, and Today
- temporal primitives already available from Today and Planning components
