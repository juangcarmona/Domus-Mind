---
Status: Phase 1 Output — Shell Foundation
Audience: Engineering / Product
Produced: 2026-04-02
Depends on:
  - docs/00_product/surface-system.md
  - specs/system/surface-system-reboot-plan.md
  - specs/system/surface-system-reboot-plan-phase-0.md
---

# Phase 1 — Shell Foundation

This document is the required output of Phase 1 from the surface-system reboot plan.

It records the shell, layout, token, navigation, and primitive work that establishes the shared UX foundation for later surface migrations.

It does not claim that Planning, Today, Shared Lists, Member Agenda, or Areas have been fully rebooted.

It establishes the structural base that those later phases must inherit.

---

## 1. Phase Goal

Phase 1 exists to establish the shared product shell and the cross-surface primitives required by the reboot.

The goal of this phase is to make the new product structure real before any major surface redesign begins.

Target outcomes:

- a real desktop shell with left navigation rail
- a mobile shell that preserves the same product logic in collapsed form
- layout foundations that allow split-view composition
- shared primitives for inspector, bottom sheet, header, toolbar, compact empty states, badges, and contextual chips
- routing/navigation compatibility with the target product shell
- at least one surface rendering inside the new shell without pretending its full reboot is already complete

---

## 2. Scope of Phase 1

### In Scope

- app shell rewrite
- desktop left nav rail
- mobile header continuity
- layout rewrite to remove centered-island composition
- structural token extension
- creation of shared shell and interaction primitives
- restoration of `/areas` as a real route
- restoration of `Areas` as a primary nav item
- shell-level validation that at least one surface renders cleanly inside the new shell

### Out of Scope

- Planning temporal workbench implementation
- Today surface reboot
- Shared Lists split-pane redesign
- Member Agenda shell/integration redesign
- Areas inspector/detail redesign
- temporal primitive extraction
- calendar API relocation
- domain/read-model changes
- deletion of legacy tabs/pages that remain live until later phases

---

## 3. Summary of What Phase 1 Changed

Phase 1 made the product shell real.

The previous shell model used:

- top navigation header as the main desktop nav
- centered desktop pages constrained by max-width rules
- no inspector slot
- no bottom-sheet primitive
- no consistent structural support for dense split-view surfaces

Phase 1 changed that to:

- desktop left navigation rail
- mobile-only top header
- full-width shell-ready content area
- inspector primitive available for desktop detail flows
- bottom-sheet primitive available for mobile detail flows
- structural tokens and layout helpers ready for denser operational surfaces
- `/areas` restored as a first-class route and nav destination

This phase does not claim that the shell is already fully exploited across all surfaces.
It claims only that the shell now exists and later phases can build on it.

---

## 4. Files Created

### Shared shell and interaction primitives

```text
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
````

### Primitive CSS extensions

Shared primitive styling for:

* CountBadge
* ContextChip
* CollapsedSection

was added to:

```text
src/web/app/src/components/styles/primitives.css
```

---

## 5. Files Updated

### Shell and layout

```text
src/web/app/src/components/AppShell.tsx
src/web/app/src/components/AppShell.css
src/web/app/src/styles/layout.css
src/web/app/src/styles/tokens.css
src/web/app/src/styles/index.css
```

### Routing / navigation

```text
src/web/app/src/App.tsx
```

### Planning docs/status tracking

```text
specs/system/surface-system-reboot-plan.md
```

---

## 6. Structural Decisions Taken

### 6.1 Desktop shell model

Desktop now uses the canonical shell direction:

* left navigation rail
* central content region
* support for contextual inspector
* no top-header desktop nav as the primary desktop structure

This aligns the repo with the surface-system direction and removes the biggest structural blocker for later split-view surfaces.

### 6.2 Mobile continuity

Mobile keeps a top header and drawer behavior rather than receiving a separate product model.

This preserves continuity while allowing desktop to move to the rail-based shell.

### 6.3 App shell responsibility

The shell provides structure.

It does not own item-level selection state for all surfaces.

The `InspectorPanel` exists as a reusable primitive, but surfaces will opt into it as they migrate.

That keeps Phase 1 structural and avoids accidentally starting later surface work.

### 6.4 Layout ownership

Global layout no longer forces narrow centered desktop pages.

The old centered-island composition was removed from shared layout rules so later surfaces can become proper dense split-view workspaces.

Surface-specific layout remains the responsibility of later phases.

### 6.5 Route/nav restoration for Areas

`/areas` was restored as a real route.
`Areas` was restored as a primary nav item.

This is shell/navigation work, not an Areas redesign.

It was necessary to make the product shell match the intended major-surface model.

---

## 7. Token and Layout Foundation Changes

### 7.1 Structural layout tokens added

The token system was extended to support the shell and dense surfaces, including categories such as:

* rail width
* collapsed rail width
* inspector width
* toolbar height
* list-switcher width
* compact/default/relaxed row heights
* spacing extensions
* surface-level tokens
* typography size tokens
* inspector/bottom-sheet z-index tokens

These tokens make later split-view and dense-surface work possible without page-level magic numbers.

### 7.2 Layout system rewrite

The global layout no longer enforces:

* `max-width: 960px`
* `max-width: 680px`
* `margin: 0 auto`

as the default desktop page model.

This is the core removal of the centered-island anti-pattern at the layout level.

New layout helpers were introduced to support surface-oriented composition and future full-width/split-view surfaces.

---

## 8. New Shared Primitives Introduced

### 8.1 NavRail

Purpose:

* desktop primary navigation
* visible major-surface switching
* stable shell anchor for later phases

Current status:

* implemented
* used by the desktop shell
* hidden on mobile

### 8.2 InspectorPanel

Purpose:

* contextual desktop detail slot
* default place for shallow inspection and lightweight editing in later phases

Current status:

* implemented as a reusable primitive
* available for later surface wiring
* not yet deeply integrated into major surfaces in this phase

### 8.3 BottomSheetDetail

Purpose:

* mobile contextual detail pattern
* preserve product logic without creating a separate mobile product model

Current status:

* implemented as a reusable primitive
* available for later phases
* not yet deeply integrated into all surfaces in this phase

### 8.4 PageHeader

Purpose:

* compact per-surface header primitive
* replace ad-hoc or oversized page-header patterns over time

Current status:

* implemented
* available for later surface adoption

### 8.5 CompactToolbar

Purpose:

* dense controls strip
* house view switches, compact filters, date navigation, or other surface-local controls

Current status:

* implemented
* available for later surface adoption

### 8.6 QuickAddBar

Purpose:

* local fast capture pattern
* supports later Shared Lists and other low-friction capture surfaces

Current status:

* implemented
* not yet broadly wired in this phase

### 8.7 EmptyStateCompact

Purpose:

* replace decorative oversized empty states
* encourage action without wasting space

Current status:

* implemented
* ready for later surface adoption

### 8.8 CountBadge

Purpose:

* small count indicator
* supports visible counts in rails, switchers, headers, or row summaries

Current status:

* implemented

### 8.9 ContextChip

Purpose:

* compact context cue
* supports area/list/plan linkage later

Current status:

* implemented

### 8.10 CollapsedSection

Purpose:

* compress completed/secondary content while preserving access

Current status:

* implemented
* ready for later use in Lists and similar surfaces

---

## 9. Routing and Navigation Changes

### 9.1 Areas route restored

The router no longer redirects `/areas` to `/settings`.

`/areas` now renders the `AreasPage` route directly.

This restores the intended major-surface map of the product shell.

### 9.2 Areas in primary navigation

Primary navigation now includes `Areas`.

This aligns the shell with the intended major operational surfaces:

* Today
* Planning
* Lists
* Areas

Member Agenda remains entry-driven rather than rail-primary at this stage.

---

## 10. Surface Proof Achieved in This Phase

Phase 1 needed to prove that the new shell was real without pretending any full surface migration had happened.

That proof surface is:

* `AreasPage`

Why Areas was used as the shell proof:

* it was structurally simple
* it needed route/nav restoration anyway
* it allowed validation of the new shell without dragging in temporal/calendar complexity
* it avoided premature redesign of Planning or Today

What this proof demonstrates:

* the shell renders real pages
* nav state works
* restored routes work
* the new structural frame is not hypothetical

What it does **not** demonstrate:

* full inspector-based detail flows
* split-pane list switching
* temporal workbench composition
* final density behavior of all surfaces

Those belong to later phases.

---

## 11. Validation Results

### 11.1 Build status

Build completed cleanly.

Confirmed:

* TypeScript build passed
* Vite build passed

The existing chunk-size warning remains but was not introduced by this phase.

### 11.2 Structural verification

Verified:

* desktop rail exists
* mobile header remains
* old desktop top-nav model is no longer the primary shell pattern
* global max-width-centered layout rules were removed
* new structural tokens exist
* `/areas` route works
* `Areas` appears in nav
* the new shared primitives exist as actual files
* there are no obvious leftover partial rewrites in the main touched shell/layout files

---

## 12. Acceptance Criteria Check

### Criterion 1

**The app has a real shared shell aligned with the docs.**

Status: **Met**

### Criterion 2

**Left nav rail exists on desktop.**

Status: **Met**

### Criterion 3

**Shell supports a right inspector slot.**

Status: **Met**

### Criterion 4

**A bottom-sheet detail primitive exists for mobile.**

Status: **Met**

### Criterion 5

**Layout/tokens support dense split-view composition.**

Status: **Met**

### Criterion 6

**Areas exists again in routing/navigation.**

Status: **Met**

### Criterion 7

**At least one surface renders inside the new shell cleanly.**

Status: **Met**

Result:
**Phase 1 is complete.**

---

## 13. Known Risks and Deferred Work

### 13.1 Desktop content padding still exists

Risk:
Desktop shell is structurally correct, but some default content padding remains generous for future full-bleed surfaces.

Impact:
Planning and Shared Lists will likely need a full-bleed or tighter content variant in their migration phases.

Deferred to:

* Phase 2
* Phase 3

### 13.2 Modal-first detail flows still exist in old surfaces

Risk:
Legacy surfaces still rely on modal or full-page detail patterns that conflict with the inspector-first direction.

Impact:
This is expected.
Phase 1 introduced the primitives but did not migrate surface behavior yet.

Deferred to:

* Phase 2 and later

### 13.3 Temporal/calendar primitives are still fragmented

Risk:
Today and Member Agenda still own calendar/timeline pieces that should become shared later.

Impact:
Planning migration depends on careful extraction rather than ad-hoc duplication.

Deferred to:

* Phase 2

### 13.4 Cross-feature imports remain

Risk:
Some temporal code still crosses feature boundaries in ways that are acceptable short-term but not the desired final structure.

Deferred to:

* Phase 2 / Phase 5

---

## 14. What Phase 1 Explicitly Did Not Do

Phase 1 did **not**:

* rebuild Planning
* remove Today’s calendar burden
* redesign Shared Lists
* redesign Member Agenda
* redesign Areas detail behavior
* fully adopt inspector-first behavior across all surfaces
* delete legacy tabs/pages that remain necessary until replacement is live

This is intentional.

Phase 1 established the shell foundation so those later phases can proceed without re-litigating layout and navigation.

---

## 15. Next Phase Dependency

Phase 2 may now begin.

Phase 2 depends on this phase being real and stable because Planning requires:

* the new shell
* layout support for split temporal composition
* inspector availability
* compact toolbar/header patterns
* clean structural tokens for dense calendar work

Without Phase 1, Planning would have had to invent another local shell.

That blocker is now removed.

---

## 16. Summary

Phase 1 made the shell foundation real.

The repo now has:

* a real desktop nav rail
* mobile continuity
* split-view-ready layout foundations
* structural tokens for dense operational surfaces
* reusable inspector/bottom-sheet/header/toolbar primitives
* `Areas` restored as a first-class route and nav destination

This phase did not finish the product reboot.

It finished the prerequisite structure that later surface migrations now inherit.

The product can now move into Phase 2 without building on top of the old centered, top-nav, no-inspector shell model.
