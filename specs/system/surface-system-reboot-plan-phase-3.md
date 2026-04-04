---
Status: Phase 3 Output — Shared Lists
Audience: Engineering / Product
Produced: 2026-04-02
Depends on:
  - docs/00_product/surface-system.md
  - specs/system/surface-system-reboot-plan.md
  - specs/system/surface-system-reboot-plan-phase-0.md
  - specs/system/surface-system-reboot-plan-phase-1.md
  - specs/system/surface-system-reboot-plan-phase-2.md
  - specs/surfaces/shared-lists.md
---

# Phase 3 — Shared Lists

This document records the Phase 3 Shared Lists migration.

It replaces the old index/detail navigation model with a single low-navigation working surface.

It also includes the refinement pass that improved pane proportions, hierarchy, density, and overall surface cohesion.

---

## Goal

Turn `/lists` into the dense shared-lists surface defined in the spec.

Required outcomes:

- list switcher pane on desktop
- active list as the hero
- quick add always visible
- dense row-based item model
- completed items compressed
- inspector on desktop
- bottom-sheet detail on mobile
- `/lists/:listId` behaves as deep-link entry into the same surface

---

## Scope

### In Scope

- Shared Lists surface rebuild
- list switcher pane
- active list pane
- route unification for `/lists` and `/lists/:listId`
- desktop inspector usage
- mobile switcher/detail behavior
- styling refinement pass for hierarchy and density

### Out of Scope

- Today redesign
- Member Agenda redesign
- Areas redesign
- due dates or reminder semantics for list items
- task-board behavior
- broad editor redesign
- deletion of all legacy files immediately after routing change

---

## What Changed

### Created

```text
src/web/app/src/features/shared-lists/components/ListSwitcherPane.tsx
````

### Replaced / Reworked

```text id="z7q13b"
src/web/app/src/features/shared-lists/pages/SharedListsPage.tsx
src/web/app/src/features/shared-lists/shared-lists.css
```

### Updated

```text id="9g6v2m"
src/web/app/src/App.tsx
specs/system/surface-system-reboot-plan.md
```

---

## Result

Shared Lists is no longer an index page that navigates to a separate detail page.

The route now behaves as one working surface:

* `/lists` opens the Lists surface and auto-selects the active/first list
* `/lists/:listId` deep-links into the same surface with that list preselected
* desktop uses switcher + active list + inspector
* mobile uses active list first, switcher via sheet/trigger, detail via bottom sheet
* quick add stays local to the active list
* completed items remain available but compressed

---

## Current Surface Structure

### Desktop

* left nav rail
* list switcher pane
* active list pane
* inspector

### Mobile

* active list first
* compact switcher trigger
* list switching through sheet
* item detail through bottom sheet

---

## Key Implementation Decisions

### Route model unified

`/lists` and `/lists/:listId` now resolve to the same surface component.

`SharedListsPage` owns:

* optional route-param preselection
* active list state
* active detail fetch
* switcher + list body composition

This removes full-page detail navigation as the default working model.

### Existing row logic reused

`ItemRow.tsx` and `SortableItemRow.tsx` were reused rather than rewritten.

That kept:

* toggle behavior
* reorder behavior
* row interaction patterns

stable while the page-level composition changed.

### Legacy detail page not primary anymore

`SharedListDetailPage` is no longer the route target for `/lists/:listId`.

It may remain temporarily in the repo for safety, but it is no longer the active product model.

---

## Refinement Pass Summary

After the structural migration, the surface was refined to improve hierarchy and reduce the “three bordered rectangles” feel.

### Switcher improvements

* narrowed to give the center pane more space
* removed redundant `LISTS` label
* denser row spacing
* quieter inactive count badges
* clearer active-state styling with left accent indicator

### Active list improvements

* active list title now clearly owns the center pane
* header actions were quieted
* delete action was visually de-emphasized
* content pane padding reduced
* outer card feeling removed from the list body
* rows feel more like a continuous working surface

### Inspector improvements

* static generic title removed
* selected item name used as title when relevant
* empty state replaced by compact contextual hint/stat summary
* inspector feels calmer and less dead

### Surface cohesion improvements

* better pane proportions
* less decorative boxing
* stronger center-pane dominance
* lower wasted space
* calmer overall composition

---

## Acceptance Check

### `/lists` is the canonical working surface

Met.

### `/lists/:listId` deep-links into the same surface cleanly

Met.

### Active list is the hero

Met.

### Quick add is obvious and local

Met.

### Completed items are compressed by default

Met.

### Desktop detail no longer depends on full-page navigation by default

Met.

### Mobile preserves the same Lists logic in collapsed form

Met.

### No unrelated surface was redesigned

Met.

### Build remains clean

Met.

Result:
**Phase 3 is complete.**

---

## Known Debt

### Legacy detail file may still exist temporarily

The old detail page may remain in the repo until cleanup is confirmed safe.

### Item inspector depth is still lightweight

The inspector is structurally correct, but item-detail behavior can still evolve in later polish passes without changing the surface model.

### Shared list primitives are still feature-local

The Lists surface is now correct structurally, but some primitives may still deserve future promotion only if reuse pressure appears.

---

## Next Phase Dependency

Phase 4 can now begin.

Today can build on:

* the shared shell
* the proven dense-surface model
* inspector and bottom-sheet patterns
* the lower-chrome, higher-density rhythm already validated in Planning and Lists

---

## Summary

Phase 3 completed the Shared Lists migration.

The product now has:

* a real Lists working surface
* unified `/lists` and `/lists/:listId` behavior
* desktop switcher + active list + inspector composition
* mobile compressed equivalent
* dense row-based list interaction
* local quick add
* compressed completed state
* a calmer, more coherent three-zone layout after refinement

The Lists surface is now structurally aligned with the shared-lists spec and the reboot plan.
