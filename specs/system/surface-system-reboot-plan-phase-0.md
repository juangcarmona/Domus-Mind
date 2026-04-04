---
Status: Phase 0 Output — Audit and Mapping
Audience: Engineering / Product
Produced: 2026-04-02
Depends on:
  - docs/00_product/surface-system.md
  - specs/system/surface-system-reboot-plan.md
  - specs/surfaces/*.md
---

# Phase 0 — Audit and Mapping

This document is the required output of Phase 0 from the surface-system reboot plan.

It maps the current frontend implementation against the target surface specs.
It does not implement anything.
It identifies what is reusable, what must change, and what must be deleted.

---

## 1. Audit Summary

The frontend is structurally misaligned with the target surface system in several important ways.

Key findings:

- **App shell is a top header bar**, not a left navigation rail. No right inspector panel exists anywhere.
- **Planning (`/planning`) is not a calendar.** It is a three-tab list manager (routines, tasks, plans). No temporal canvas. No date navigation. No week/day/month views.
- **The actual temporal calendar infrastructure lives in Today (`/`), not in Planning.** TodayPage renders the week grid and month view. The spec says those belong in Planning.
- **Areas (`/areas`) is not a top-level surface.** Route redirects to `/settings`. `AreasPage.tsx` exists but is unused (dead code in the router).
- **No InspectorPanel exists.** No surface uses a right contextual panel on desktop. Every detail flow navigates to a full separate page.
- **No BottomSheetDetail component exists.** Mobile detail flows use modals or navigation.
- **SharedListDetailPage is a full separate page**, not a split-pane + inspector pattern.
- **AreaDetailPage is a full separate page**, not an inspector.
- **Today page has scope bloat**: it contains the mid-term week grid and month view, which belong in Planning.
- **Layout uses narrow centered containers** (`max-width: 960px`, `max-width: 680px`). This is the "giant centered island" anti-pattern. Desktop split views require full-width shell composition.

What works and can be reused:

- Token system (`styles/tokens.css`) — solid foundation, needs targeted extension.
- Primitives stylesheet (`components/styles/primitives.css`) — buttons, cards, modals, item lists.
- `AgendaMiniCalendar.tsx` — compact month grid with range highlighting. Abstract to shared.
- `AgendaHeader.tsx` — compact 3-row surface header. Good pattern. Generalize.
- `HourTimeline.tsx` — 48-slot day timeline. Solid. Move to shared primitives.
- `WeeklyHouseholdGrid.tsx` + `today-week.css` — week grid. Decouple from Today; move to shared temporal primitives.
- `TodayMemberCell.tsx`, `CalendarEntryItem.tsx` — entry row display components. Keep.
- `SortableItemRow.tsx`, `ItemRow.tsx` — list item row components. Keep and refine.
- `EntityCard.tsx` — shared clickable card shell. Keep.
- i18n structure — preserve entirely.
- Redux store slices — preserve. No domain or data-layer changes required.

---

## 2. Surface-by-Surface Mapping

---

### 2.1 Planning

**Target spec:** `specs/surfaces/planning.md`
**Target:** temporal workbench, Week default, Day and Month as first-class views, calendar canvas as hero, right inspector on desktop, compact header.

**Current route:** `/planning`
**Current page:** `src/web/app/src/features/planning/pages/PlanningPage.tsx`

#### Current state

`PlanningPage.tsx` is a three-tab list manager:

- Tab 1: Routines (`RoutinesTab.tsx`)
- Tab 2: Tasks (`TasksTab.tsx`)
- Tab 3: Plans (`PlansTab.tsx`) — list of plans using `EntityCard`, grouped by today/upcoming/later

There is a `PlanningOverview` strip showing stat counts at the top.

No calendar. No temporal canvas. No week/day/month views. No date navigation.

The actual calendar infrastructure is in `features/today`:

- `WeeklyHouseholdGrid.tsx` — 7-column week grid
- `MonthView.tsx` — month calendar
- `TimelineRuler.tsx` — 24h ruler

#### Mapping

| Current | Fate |
|---------|------|
| `PlanningPage.tsx` | Replace entirely. New Planning is a temporal workbench, not a list manager. |
| `PlansTab.tsx` | Delete. Plan list logic moves to the Planning calendar canvas + inspector. |
| `RoutinesTab.tsx` | Keep as reference, but routines belong as contextual overlays in the calendar or as a lightweight dedicated section, not a parallel tab. |
| `TasksTab.tsx` | Keep as reference. Tasks surface needs separate design; not part of Planning reboot scope. |
| `PlanningAddModal.tsx` | Keep and extend. Creation modal works well. |
| `AssignTaskModal.tsx` | Keep. |
| `planning.css` | Replace. Largely tied to list/tab patterns, not a calendar. |

#### Reusable from Today (move to Planning)

| Component | Current location | Proposed target |
|-----------|-----------------|-----------------|
| `WeeklyHouseholdGrid.tsx` | `features/today/components/grid/` | New shared temporal primitives or moved under planning |
| `MonthView.tsx` | `features/today/components/` | Shared temporal primitives |
| `TimelineRuler.tsx` | `features/today/components/timeline/` | Shared temporal primitives |
| `today-week.css` | `features/today/` | Shared temporal styles |

#### Blockers

- Today currently owns all temporal display infrastructure. Decoupling requires careful migration to avoid breaking Today before it is rebuilt.
- Planning has no existing week/day calendar structure to extend — it must be built net-new. Extract from Today in parallel or after Today reboot.
- `PlanningAddModal` is used from both Planning and AreaDetailPage — decouple carefully.

---

### 2.2 Shared Lists

**Target spec:** `specs/surfaces/shared-lists.md`
**Target:** split-pane desktop (list switcher + active list + optional inspector), row-based density, quick add persistent, completed items collapsed, no full-page navigation on selection.

**Current routes:**
- `/lists` → `SharedListsPage.tsx` (index)
- `/lists/:listId` → `SharedListDetailPage.tsx` (detail — separate full page)

#### Current state

`SharedListsPage.tsx`:

- Uses `class="page-wrap"` (max-width 680px centered) — centered island anti-pattern
- Renders lists as `item-card` rows in an `item-list`
- Shows unchecked count badge (good)
- Navigates to `/lists/:listId` on row click — full page nav, wrong

`SharedListDetailPage.tsx`:

- Full separate page
- Has inline quick-add bar (input + submit) — close to correct but not a shared primitive
- Has `SortableItemRow` / `ItemRow` — row-based with @dnd-kit
- Has completed items collapsed behind `checkedCollapsed` toggle — correct behavior exists
- No inspector panel
- Rename is inline (input on the title) — acceptable
- Delete flow uses existing `ConfirmDialog` — reusable

#### Mapping

| Current | Fate |
|---------|------|
| `SharedListsPage.tsx` | Refactor. Becomes the left list-switcher pane, not a standalone index page. The page wrapper splits into switcher + active list. |
| `SharedListDetailPage.tsx` | Refactor. Active list content moved into the main pane. No full-page navigation. |
| `ItemRow.tsx` | Keep and refine into `DenseListRow` primitive. |
| `SortableItemRow.tsx` | Keep. Reusable sortable row. |
| `CreateSharedListModal.tsx` | Keep. |
| `AttachChecklistSelector.tsx` | Keep (used for planning+list linking). |
| `EventChecklistSection.tsx` | Keep. |
| `shared-lists.css` | Extend. Core row styles are sound; add switcher pane, active list header, quick-add bar styles. |

#### Missing primitives needed

- `ListSwitcherPane` — new desktop pane showing list index with unchecked counts
- `QuickAddBar` — new reusable component (currently inline in detail page)
- `InspectorPanel` — needed for item detail on desktop
- `BottomSheetDetail` — needed for item detail on mobile

#### Blockers

- `SharedListDetailPage` currently receives `listId` from route params. After refactor, active list state lives in the shared-lists page, not a separate route. Need to decide route strategy (keep `/lists/:listId` as a direct link with the pane pre-selected, or make it layout-aware).
- @dnd-kit drag state is tightly coupled to the current page component. Must decouple when restructuring into a pane model.

---

### 2.3 Today

**Target spec:** `specs/surfaces/today.md`
**Target:** household-first read surface, compact date header, household row + member rows, dense item grammar (max 2 collapsed, +N), one expanded member at a time, inspector on desktop, bottom sheet on mobile, no mid-term calendar inside Today.

**Current route:** `/`
**Current page:** `src/web/app/src/features/today/pages/TodayPage.tsx`

#### Current state

`TodayPage.tsx` renders:

1. `TodayBoard` — day view with member rows (closest to spec)
2. `WeeklyHouseholdGrid` (mid-term week view) — WRONG for Today
3. `MonthView` (mid-term month view) — WRONG for Today

The page has a `midTermView` toggle (week/month) and a `coord-midterm-section` with tab bar. This is the biggest scope problem: Today is carrying Planning's calendar.

TodayBoard:
- Renders household row and member rows (correct)
- `TodayMemberCell` shows collapsed items — partial alignment with spec
- Navigation: prev/next day buttons (correct)
- Mobile: swipe gesture (correct)
- CSS class names: `.coord-page`, `.coord-day-panel`, `.coord-day-header` — legacy naming from when Today was called "Coordination"

`today-shell.css`, `today-board.css`, `today-week.css`, `today-month.css`, `today-ruler.css` — 5 separate CSS files for one surface (Today) that also owns the week grid used by Planning.

Missing from Today vs spec:
- No inspector panel for item detail
- No bottom sheet for mobile item detail — uses modal (`EditEntityModal`)
- Item expansion: TodayMemberCell has some expand logic but unclear if it matches spec exactly (max 2, +N, one expanded at a time)
- `No date (N)` secondary entry — present as a concept in timeline but unclear if surfaced correctly

#### Mapping

| Current | Fate |
|---------|------|
| `TodayPage.tsx` | Rewrite. Remove mid-term calendar sections. Keep day view structure. |
| `TodayBoard.tsx` | Refactor. Keep core member row structure. Fix class names (remove coord-*). Add inspector wire-up. |
| `TodayMemberCell.tsx` | Keep and refine. Verify collapse rule (max 2, +N, one expanded). |
| `CalendarEntryItem.tsx` | Keep. Good entry display primitive. |
| `WeeklyHouseholdGrid.tsx` | Move. Belongs in Planning canvas, not in Today. |
| `WeekHeader.tsx`, `WeeklyGridRow.tsx`, etc. | Move with the grid to shared temporal primitives or Planning. |
| `MonthView.tsx` | Move. Belongs in Planning. |
| `TimelineRuler.tsx` | Move. Used by Agenda and potentially Planning. Shared temporal primitive. |
| `today-shell.css` | Refactor. Many `.coord-*` classes are obsolete. Rename to fit Today surface only. |
| `today-week.css`, `today-month.css`, `today-ruler.css` | Move to shared temporal styles when calendar primitives are extracted. |
| `today-board.css` | Refactor. Remove `.coord-*` overrides that survive from old naming. |

#### Blockers

- Today currently shares the weekly grid data fetch (`weekApi.getWeeklyGrid`) with Member Agenda. After Planning takes over the calendar, Today still needs its day-view data. The grid API delivers weekly data — Today only needs one day slice. This is fine; the API shape does not need to change.
- The `midTermView` state in TodayPage (week/month toggle) must be removed entirely from Today, not just hidden. It currently drives data fetching too.

---

### 2.4 Member Agenda

**Target spec:** `specs/surfaces/member-agenda.md`
**Target:** person-focused temporal surface, Day default, Week + Month first-class, compact person/date header, central time canvas, right inspector on desktop, bottom sheet on mobile.

**Current routes:**
- `/agenda/members/:memberId` → `MemberAgendaPage` (wrapper that renders `AgendaPage` with memberId)
- `/agenda/shared` → `AgendaPage` (shared/collective view)

**Current page:** `src/web/app/src/features/agenda/pages/MemberAgendaPage.tsx` (`AgendaPage` component)

#### Current state

This is the most spec-aligned surface in the app.

`AgendaPage`:
- `AgendaHeader.tsx` — compact 3-row header (identity row, date nav row, view switch row) — very close to spec
- `AgendaMiniCalendar.tsx` — compact mini calendar with week-range highlighting — solid
- `MemberDayView.tsx` — day view wrapping `HourTimeline` and `SelectedDateCard`
- `MemberWeekView.tsx` — week view
- `MemberMonthView.tsx` — month view
- `HourTimeline.tsx` — 48-slot timeline with absolute-positioned duration blocks
- `SelectedDateCard.tsx` — sidebar card showing untimed/overdue entries — this is the embryonic inspector

What is missing vs spec:
- No `InspectorPanel` — `SelectedDateCard` is hardcoded next to the timeline, not a true inspector slot
- Mobile detail uses `EditEntityModal` (full modal), not bottom sheet
- `AgendaMiniCalendar` is rendered inside the body of `AgendaPage`, not in a dedicated inspector slot — on desktop it should be in the right panel
- Day is the default view — correct per spec
- Week and Month exist — correct

#### Mapping

| Current | Fate |
|---------|------|
| `MemberAgendaPage.tsx` | Keep. Light wrapper. |
| `AgendaPage.tsx` | Refactor when InspectorPanel is available. Current layout is functional; needs shell wiring. |
| `AgendaHeader.tsx` | Keep and generalize. This is nearly the `DateNavigator` + `ViewSwitch` + identity row primitive already. |
| `AgendaMiniCalendar.tsx` | Keep and promote to shared primitive at `components/calendar/MiniCalendar.tsx`. |
| `HourTimeline.tsx` | Keep and promote to shared primitive at `components/calendar/HourTimeline.tsx`. |
| `MemberDayView.tsx` | Keep and refine. |
| `MemberWeekView.tsx` | Keep and refine. |
| `MemberMonthView.tsx` | Keep and refine. |
| `SelectedDateCard.tsx` | Refactor into `InspectorPanel` content once `InspectorPanel` exists. Remove standalone `.agenda-date-card` pattern. |
| `SharedDayView.tsx`, `SharedWeekView.tsx` | Keep. These handle the shared/household agenda — review against spec. |
| `agenda.css` | Refactor. Many good patterns; strip overdue-card standalone styles into shared primitives. |

#### Blockers

- `AgendaPage` uses `weekApi.getWeeklyGrid` from `features/today/api/weekApi` — cross-feature import. When Today is decoupled, move grid API to `api/` or `features/calendar/api/`. Non-blocking for now.
- Members Agenda currently has `/agenda/shared` for a shared view — this is not in the surface spec as a named surface. Keep but ensure it does not conflict with Planning's role.

---

### 2.5 Areas

**Target spec:** `specs/surfaces/areas.md`
**Target:** compact ownership surface, dense list-first, unowned highlighted, right inspector on desktop, bottom sheet on mobile, lightweight add/assign flows, not settings.

**Current routes:**
- `/areas` → **redirects to `/settings`** — BROKEN for this reboot
- `/areas/:areaId` → `AreaDetailPage.tsx` (full separate page)

**Current pages:**
- `AreasPage.tsx` — exists but is NOT wired in the router (dead code)
- `AreaDetailPage.tsx` — full-page detail

#### Current state

`AreasPage.tsx` (dead/unreachable):
- Has grouped areas: unowned + owned
- Uses `AreaRow` with inline select for owner assignment
- Navigates to `/areas/:areaId` on row click — should open inspector instead
- Uses `item-card` with `item-card-body` / `item-card-actions` — correct row primitives

`AreaDetailPage.tsx` (accessible via direct link):
- Full page with header, owner section, supporter section, related work section
- Has color picker, rename, add/create modal
- Loads plans, routines, timeline tasks all at once — heavy
- `PlanningAddModal` used for creation — shared

The core blocker is the routing: `/areas` goes to settings. Areas is not a primary navigation surface.

#### Mapping

| Current | Fate |
|---------|------|
| `AreasPage.tsx` | Resurrect. Fix router to restore `/areas` route. Refactor layout to use shell + compact header + dense list. |
| `AreaRow` component (inside AreasPage) | Extract as standalone component. |
| `AreaDetailPage.tsx` | Keep. Refactor ownership sections to work within InspectorPanel when available. |
| `AreaDetailHeader.tsx` | Keep. Refactor to use shared compact header pattern. |
| `AreaOwnerSection.tsx` | Keep. |
| `AreaRelatedWorkSection.tsx` | Keep. |
| `AssignOwnerModal.tsx` | Keep. |
| `CreateAreaModal.tsx` | Keep. |
| `areas.css` | Keep and extend. Color picker CSS is solid. Area row styles are minimal and fine. |
| Router in `App.tsx` | Fix: restore `/areas` to render `AreasPage`. Remove redirect to `/settings`. Add Areas to `NAV_ITEMS`. |

#### Blockers

- **Navigation blocker**: Areas must be added to `NAV_ITEMS` in `AppShell.tsx` and to the route table in `App.tsx`. The redirect must be removed.
- **Shell blocker**: AreaDetailPage cannot use an inspector until InspectorPanel exists. In the interim, the full-page detail is acceptable.
- `AreasPage` does not import styles from `areas.css` — verify import is present. (The CSS file exists but may not be imported in the page.)

---

## 3. App Shell Audit

**Current:** `src/web/app/src/components/AppShell.tsx` + `AppShell.css`

**Current structure:**
- Top horizontal header bar (`site-header`)
- Brand (logo + household name)
- Horizontal nav links (Today, Planning, Lists only)
- Avatar dropdown in header-end
- Mobile: hamburger + left drawer overlay

**What the spec requires:**
- Left navigation rail (desktop)
- Compact page header per surface
- Central content canvas
- Optional right contextual inspector
- Mobile: top header + content + bottom sheet/drawer

**Gap analysis:**

| Aspect | Current | Required | Gap |
|--------|---------|----------|-----|
| Navigation position | Top horizontal bar | Left rail (desktop) | Full structural change |
| Navigation items | Today, Planning, Lists | Today, Planning, Lists, Areas | Areas missing |
| Inspector panel | None | Right contextual inspector | Does not exist |
| Mobile navigation | Hamburger + left drawer | Drawer or compact nav | Acceptable on mobile |
| Content max-width | 960px centered (`app-main`) | Full-width shell | Centered island |
| Layout model | `flex-direction: column` stack | Shell with rail + content | Needs structural rework |

**Files:**

| File | Fate |
|------|------|
| `AppShell.tsx` | Rewrite. New shell: left nav rail + content area + optional right inspector slot. Mobile: keep drawer pattern, collapse rail. |
| `AppShell.css` | Rewrite alongside component. |
| `styles/layout.css` | Rewrite `.app-main` / `.l-page`. Remove max-width centering. Add split-pane layout classes. |
| `styles/tokens.css` | Extend. Add structural tokens (see Section 4). |

---

## 4. Token and Style Gap Analysis

### Current token state (`styles/tokens.css`)

**Present and correct:**
- Color system: `--bg`, `--surface`, `--text`, `--muted`, `--primary`, `--secondary`, `--accent`, `--border`, `--danger`, `--success`
- Semantic aliases: `--color-bg-page`, `--color-bg-surface`, etc.
- Spacing: `--space-1` through `--space-6` (0.25rem to 1.5rem)
- Header height: `--header-height: 3.5rem`
- Basic radii: `--radius-sm: 2px, --radius-md: 2px, --radius-lg: 3px` — flat/minimal, consistent
- Shadows: `--shadow-sm: none, --shadow-md: ...` — quiet, correct
- Z-index scale: `--z-header` through `--z-modal`
- Dark theme: defined on `[data-theme="dark"]`

**Missing structural tokens:**

```css
/* Navigation and layout */
--rail-width: 14rem;           /* left nav rail width */
--rail-width-collapsed: 3rem;  /* optional collapsed rail */
--inspector-width: 20rem;      /* right contextual inspector */
--toolbar-height: 2.75rem;     /* compact toolbar/controls bar */
--list-switcher-width: 16rem;  /* list pane (Shared Lists) */

/* Component height tokens (density rules) */
--row-height-compact: 2.25rem;  /* dense list row */
--row-height-default: 2.75rem;  /* standard row */
--row-height-relaxed: 3.25rem;  /* info-rich row */

/* Spacing extension */
--space-7: 1.75rem;
--space-8: 2rem;

/* Surface levels */
--surface-alt: color-mix(in srgb, var(--primary) 3%, var(--surface));
--surface-raised: var(--surface); /* cards/inspector panels */
```

**Missing base styles:**

- `--font-size-xs`, `--font-size-sm`, `--font-size-base`, `--font-size-md` — typography scale not tokenized
- No explicit `--line-height-dense` for compact rows vs default body text

**Typography hierarchy:**
Currently undeclared. `h1` etc. use browser defaults via `base.css`. Need explicit type scale tokens for surface headers (`0.72rem` section labels already used ad-hoc across multiple files).

**Dense component sizing:**
The `primitives.css` item-card padding (`0.75rem 1rem`) is generous for a density-first system. Needs a `.item-card--compact` modifier or reduced default.

---

## 5. Anti-Pattern Inventory

Listed by severity.

### Critical

**1. Giant centered islands**
- `styles/layout.css`: `.app-main { max-width: 960px; margin: 0 auto; }` and `.page-wrap { max-width: 680px; margin: 0 auto; }`
- Affects: all surfaces on desktop
- Every surface renders as a narrow column. Desktop split views are impossible.
- Must be removed before any surface migration makes sense.

**2. Planning is not a calendar**
- `features/planning/pages/PlanningPage.tsx`: three-tab list manager
- Spec requires a temporal workbench with a calendar canvas
- Full replacement required

**3. Areas not in navigation**
- `App.tsx`: `/areas` → `<Navigate to="/settings" replace />`
- `components/AppShell.tsx`: NAV_ITEMS has no Areas entry
- Areas is unreachable as a primary surface
- Must fix the router and nav before any Areas work

**4. No InspectorPanel anywhere**
- Every "detail" flow navigates to a full separate page or opens a modal
- `SharedListDetailPage`, `AreaDetailPage` — both full separate pages
- Desktop inspector pattern does not exist anywhere in the codebase

### High

**5. Today owns the calendar**
- `TodayPage.tsx` renders `WeeklyHouseholdGrid` and `MonthView`
- These belong in Planning
- Until Planning has its own calendar canvas, Today must carry this — creates coupling

**6. Full-page navigation for list detail**
- `SharedListsPage` navigates to `/lists/:listId` on row click
- Spec: list switcher pane + active list in the same surface, no navigation

**7. Full-page navigation for area detail**
- `AreasPage` navigates to `/areas/:areaId` on row click
- Spec: inspector on desktop, bottom sheet on mobile

**8. Modal for item inspection on desktop**
- `EditEntityModal` is used for plan/task/routine detail on both Today and Agenda
- On desktop this should be inspector-based
- Modals are appropriate for destructive actions or short-flow interrupts only

### Medium

**9. Legacy CSS class naming**
- `TodayBoard.tsx` and related CSS: `.coord-page`, `.coord-day-panel`, `.coord-day-header`, `.coord-day-header-label` — "coord" is a old name for Today
- `today-shell.css` contains `.coord-*` styles mixed with `today-*` styles
- Confusing, impedes refactor

**10. Cross-feature imports**
- `AgendaPage` imports `weekApi` from `features/today/api/weekApi`
- `AgendaPage` imports `buildMemberEntries` etc. from `features/today/utils/`
- Calendar utilities and API should live in a shared location

**11. `empty-state` with decorative text padding**
- `SharedListsPage`, `AreasPage`, etc. use `.empty-state` with `padding: 2.5rem 1rem; text-align: center`
- Creates large decorative empty blocks on desktop
- Should be compact per spec

**12. `item-card` padding is 0.75rem 1rem**
- For a density-first system this is generous
- All lists feel slightly loose

**13. Inline owner select in AreaRow**
- A `<select>` widget inside a list row for owner assignment is awkward on mobile
- Spec says ownership editing should be in inspector/lightweight flow

---

## 6. Shared Primitives Backlog

### Already exists — abstract and formalize

| Proposed primitive | Current location | File(s) to extract from |
|--------------------|-----------------|------------------------|
| `MiniCalendar` | `features/agenda/components/AgendaMiniCalendar.tsx` | Move to `components/calendar/MiniCalendar.tsx` |
| `HourTimeline` | `features/agenda/components/HourTimeline.tsx` | Move to `components/calendar/HourTimeline.tsx` |
| `WeekGrid` | `features/today/components/grid/WeeklyHouseholdGrid.tsx` + related | Move to `components/calendar/WeekGrid/` |
| `MonthGrid` | `features/today/components/MonthView.tsx` | Move to `components/calendar/MonthGrid.tsx` |
| `DateNavigator` | `AgendaHeader.tsx` (rows 2+3) | Extract from AgendaHeader |
| `ViewSwitch` | `AgendaHeader.tsx` (row 3) | Extract from AgendaHeader |
| `DenseListRow` | `features/shared-lists/components/ItemRow.tsx` (base) | Generalize and move to `components/` |
| `CalendarEntryItem` | `features/today/components/shared/CalendarEntryItem.tsx` | Stays in shared, review path |

### Must be created net-new

| Primitive | Owner location | Purpose |
|-----------|---------------|---------|
| `NavRail` | `components/NavRail.tsx` + `.css` | Left navigation rail replacing the current top-header nav |
| `InspectorPanel` | `components/InspectorPanel.tsx` + `.css` | Right contextual panel for desktop detail |
| `BottomSheetDetail` | `components/BottomSheetDetail.tsx` + `.css` | Mobile detail overlay (slide-up sheet) |
| `PageHeader` | `components/PageHeader.tsx` + `.css` | Compact per-surface header replacing ad-hoc `page-header` divs |
| `CompactToolbar` | `components/CompactToolbar.tsx` + `.css` | Controls strip for filters, view switches, date nav |
| `QuickAddBar` | `components/QuickAddBar.tsx` + `.css` | Always-visible inline capture (currently inline in SharedListDetailPage) |
| `EmptyStateCompact` | `components/EmptyStateCompact.tsx` + `.css` | Compact empty state replacing the full centred `.empty-state` |
| `ListSwitcherPane` | `features/shared-lists/components/ListSwitcherPane.tsx` | Left pane for list index in Shared Lists |
| `ContextChip` | `components/ContextChip.tsx` + `.css` | Small chip for showing area/plan context links |
| `CountBadge` | `components/CountBadge.tsx` | Unchecked count, task count, etc. Exists partially as `.pill` / `.shared-list-card-count` |
| `CollapsedSection` | `components/CollapsedSection.tsx` | Collapsed items pattern (checked/completed items) |

### Placement of CSS for new primitives

All net-new shared primitives:
- Component code: `src/web/app/src/components/<Name>.tsx`
- Component styles: colocated CSS `src/web/app/src/components/<Name>.css` or extended in `components/styles/primitives.css`

Do not add to `index.css` or `App.css`.

---

## 7. Recommended Migration Order

Aligned with `specs/system/surface-system-reboot-plan.md`.

```
Phase 1 — Shell Foundation
  1a. Token extension (add structural tokens to tokens.css)
  1b. NavRail (replaces horizontal top nav on desktop)
  1c. InspectorPanel (new right-panel slot in AppShell)
  1d. BottomSheetDetail (new mobile detail pattern)
  1e. PageHeader (shared compact header)
  1f. CompactToolbar (controls strip)
  1g. Layout rewrite (remove max-width centering in layout.css)
  1h. AppShell rewrite (left rail + content + inspector slot)
  1i. Fix Areas route (restore /areas, add to nav)
  1j. EmptyStateCompact, CountBadge, ContextChip, QuickAddBar

Phase 2 — Planning Reboot
  2a. Extract calendar primitives from Today: WeekGrid, MonthGrid, MiniCalendar, HourTimeline
  2b. Build new PlanningPage as temporal workbench
  2c. Week view (primary) using shared WeekGrid
  2d. Day view using shared HourTimeline
  2e. Month view using shared MonthGrid
  2f. Selected plan → inspector flow
  2g. PlanningAddModal wire-up
  2h. Remove WeeklyHouseholdGrid and MonthView from TodayPage

Phase 3 — Shared Lists Reboot
  3a. ListSwitcherPane (new left pane)
  3b. QuickAddBar (shared primitive)
  3c. DenseListRow refinement
  3d. Restructure SharedListsPage into split-pane layout
  3e. Remove SharedListDetailPage full-page navigation
  3f. Wire item detail to InspectorPanel (desktop) / BottomSheetDetail (mobile)
  3g. Completed items → CollapsedSection

Phase 4 — Today Reboot
  4a. Strip TodayPage of mid-term calendar sections (WeekGrid, MonthView, coord-midterm)
  4b. Rewrite TodayBoard into the Today surface spec (date header, household row, member rows)
  4c. Rename all .coord-* CSS classes to .today-*
  4d. Wire item selection to InspectorPanel (desktop) / BottomSheetDetail (mobile)
  4e. Implement +N expand/collapse rule (max 2 visible, one expanded)

Phase 5 — Member Agenda Reboot
  5a. Extract AgendaHeader into shared DateNavigator + ViewSwitch + surface header
  5b. Promotes AgendaMiniCalendar and HourTimeline to shared (done in Phase 2)
  5c. Restructure AgendaPage to use AppShell inspector slot
  5d. Move SelectedDateCard into InspectorPanel
  5e. Wire item selection to InspectorPanel / BottomSheetDetail

Phase 6 — Areas Reboot
  6a. Resurrect AreasPage in router (/areas)
  6b. Refactor AreasPage to standard shell + compact header + dense list
  6c. Wire area row selection to InspectorPanel (desktop) / BottomSheetDetail (mobile)
  6d. Refactor AreaDetailPage sections into InspectorPanel-compatible layout
```

---

## 8. Concrete File-Level Decisions

### Files to create (Phase 1 shell)

```
src/web/app/src/components/NavRail.tsx
src/web/app/src/components/NavRail.css
src/web/app/src/components/InspectorPanel.tsx
src/web/app/src/components/InspectorPanel.css
src/web/app/src/components/BottomSheetDetail.tsx
src/web/app/src/components/BottomSheetDetail.css
src/web/app/src/components/PageHeader.tsx
src/web/app/src/components/PageHeader.css
src/web/app/src/components/CompactToolbar.tsx
src/web/app/src/components/CompactToolbar.css
src/web/app/src/components/QuickAddBar.tsx
src/web/app/src/components/QuickAddBar.css
src/web/app/src/components/EmptyStateCompact.tsx
src/web/app/src/components/EmptyStateCompact.css
src/web/app/src/components/CountBadge.tsx
src/web/app/src/components/ContextChip.tsx
src/web/app/src/components/CollapsedSection.tsx
```

### Files to create (Phase 2 calendar extraction)

```
src/web/app/src/components/calendar/MiniCalendar.tsx       (from AgendaMiniCalendar)
src/web/app/src/components/calendar/MiniCalendar.css
src/web/app/src/components/calendar/HourTimeline.tsx       (from features/agenda/components)
src/web/app/src/components/calendar/HourTimeline.css
src/web/app/src/components/calendar/WeekGrid.tsx           (from WeeklyHouseholdGrid)
src/web/app/src/components/calendar/WeekGrid.css
src/web/app/src/components/calendar/MonthGrid.tsx          (from MonthView)
src/web/app/src/components/calendar/MonthGrid.css
src/web/app/src/api/calendarApi.ts                         (from features/today/api/weekApi.ts)
```

### Files to rewrite

```
src/web/app/src/components/AppShell.tsx        (left rail shell)
src/web/app/src/components/AppShell.css
src/web/app/src/styles/layout.css              (remove centered max-width)
src/web/app/src/styles/tokens.css              (add structural tokens)
src/web/app/src/features/planning/pages/PlanningPage.tsx    (temporal workbench)
src/web/app/src/features/planning/planning.css
```

### Files to fix (routing)

```
src/web/app/src/App.tsx
  - Remove: <Route path="/areas" element={<Navigate to="/settings" replace />} />
  - Add:    <Route path="/areas" element={<AreasPage />} />
  - Add AreasPage import
src/web/app/src/components/AppShell.tsx
  - Add Areas to NAV_ITEMS
```

### Files to delete (ultimately)

```
src/web/app/src/features/planning/components/PlansTab.tsx        (replaced by Planning calendar)
src/web/app/src/features/planning/components/RoutinesTab.tsx     (replaced; routines become calendar overlays)
src/web/app/src/features/planning/components/TasksTab.tsx        (scoped separately; not in Planning spec)
```
Note: Do not delete until new Planning page is fully functional.

### Files to move

```
src/web/app/src/features/today/api/weekApi.ts
  → src/web/app/src/api/calendarApi.ts   (shared; referenced by Today, Planning, Agenda)

src/web/app/src/features/today/utils/dateUtils.ts, todayPanelHelpers.ts, calendarEntry.ts
  → src/web/app/src/lib/calendarUtils.ts (or split)
  These utils are already imported by Agenda — confirm and consolidate

src/web/app/src/features/agenda/components/AgendaMiniCalendar.tsx
  → src/web/app/src/components/calendar/MiniCalendar.tsx

src/web/app/src/features/agenda/components/HourTimeline.tsx
  → src/web/app/src/components/calendar/HourTimeline.tsx

src/web/app/src/features/today/components/grid/ (WeeklyHouseholdGrid and related)
  → src/web/app/src/components/calendar/WeekGrid/
```

---

## 9. Blockers and Risks

### Hard blockers

**B1 — Planning has no calendar canvas**
The current `/planning` route has no temporal infrastructure. Building Planning requires extracting calendar components from Today. This creates a migration dependency: Planning extraction must happen before or in parallel with Today cleanup.

**B2 — Areas has no primary route**
`/areas` redirects to settings. Without restoring this route, all Areas work is invisible. This is a 5-minute fix in `App.tsx` and `AppShell.tsx`. Fix first in Phase 1.

**B3 — No InspectorPanel or BottomSheetDetail**
Without these, every surface detail flow requires full-page navigation or a modal. Phases 2–6 all depend on InspectorPanel being implemented in Phase 1.

**B4 — Centered max-width layout blocks desktop split views**
`layout.css` must be rewritten before desktop split-pane composition is possible. Cannot build a NavRail + content + inspector layout without this.

### Risks

**R1 — Calendar extraction breaks Today**
Moving WeeklyHouseholdGrid and MonthView out of Today will break TodayPage if done without simultaneously providing Today's new day-only view. Migration order: build new Today first (without mid-term section) OR keep old components in place and create parallel shared versions, then clean up.

**R2 — Shared route `/lists/:listId` must survive**
After the Shared Lists reboot to a split-pane layout, direct links to `/lists/:listId` must still work (deep-linking, browser back). Ensure the list-detail route still resolves, pre-selecting the relevant list in the switcher pane.

**R3 — `EditEntityModal` is used everywhere**
`EditEntityModal` is a heavy multi-entity full-screen modal used across Planning, Today, Agenda, and Areas. On desktop this conflicts with the inspector-first model. Migration is a gradual replacement — do not try to remove it in Phase 1. Target replacement in Phase 2 (Planning) first, then extend to other surfaces.

**R4 — `PlanningAddModal` is shared**
Used by Planning, AreaDetailPage, TodayPage, AgendaPage. Any changes to it affect all surfaces. Keep stable during Phase 1. Only refactor when Planning rebuild requires it.

**R5 — CSS naming collisions**
`coord-*` class names in Today board/shell CSS survive from an older "coordination" page concept. They must be renamed to `today-*` equivalents, but only in the Today reboot phase — not before.

---

## 10. Route Table — Current vs Target

| Route | Current | Target |
|-------|---------|--------|
| `/` | TodayPage (with embedded calendar) | TodayPage (day-only, no mid-term sections) |
| `/planning` | PlanningPage (tab list) | PlanningPage (temporal workbench: week/day/month) |
| `/lists` | SharedListsPage (card index → navigate) | SharedListsPage (split pane: switcher + active list) |
| `/lists/:listId` | SharedListDetailPage (full page) | SharedListsPage (pre-selected list in switcher pane) |
| `/agenda/members/:memberId` | AgendaPage (member-focused) | Same — refine shell integration |
| `/agenda/shared` | AgendaPage (shared view) | Same — review scope |
| `/areas` | **Redirects to /settings** | AreasPage (primary ownership surface) |
| `/areas/:areaId` | AreaDetailPage (full page) | AreaDetailPage (refactored to use inspector slot) |
| `/settings` | SettingsPage | Keep as-is (members, account, preferences) |
