---
applyTo: "src/web/app/src/**/*.{ts,tsx,css}"
---

# Web Surface Reboot Instructions

## Canonical docs first

Before changing any major surface, read and follow:

- docs/00_product/strategy.md
- docs/00_product/experience.md
- docs/00_product/surface-system.md
- specs/surfaces/planning.md
- specs/surfaces/shared-lists.md
- specs/surfaces/today.md
- specs/surfaces/member-agenda.md
- specs/surfaces/areas.md
- specs/system/surface-system-reboot-plan.md

These are the source of truth for the current reboot.

## Reboot goal

DomusMind is being migrated toward one coherent product shell with multiple operational surfaces inside it.

Prioritize:
- persistent app shell
- stronger layout discipline
- denser information
- quieter styling
- desktop-first split views
- inspector/sidebar over modal chaos
- real hierarchy over floating cards
- neutral base with restrained accents
- mobile collapse of the same product, not a redesign

## Core surface rules

- content is the hero
- detail is secondary
- counts should be visible
- quick capture stays local
- completed state should compress
- controls stay compact
- dense surfaces are preferred over decorative spacing
- reuse one shared shell and interaction grammar across surfaces

## Hard anti-patterns

Avoid:
- giant centered islands
- page-as-a-pile-of-cards composition
- cards inside cards inside cards
- oversized decorative headers
- modal-first desktop interaction for shallow detail
- page-specific visual languages
- excessive empty padding
- chrome heavier than content
- full-page navigation for simple inspection
- mobile-only redesigns that change the product model

## Shell and primitive reuse

Before creating feature-local layout or interaction patterns, look for or create reusable primitives for:
- AppShell
- NavRail
- PageHeader
- CompactToolbar
- InspectorPanel
- BottomSheetDetail
- DateNavigator
- ViewSwitch
- MiniCalendar
- DenseListRow / DenseSummaryRow
- QuickAddBar
- EmptyStateCompact
- ContextChip / count badges

Do not let each surface invent local variants of these patterns unless there is a strong documented reason.

## Surface migration discipline

When touching a surface:
- align it with the shared shell
- remove legacy anti-patterns in the touched area
- keep desktop and mobile under the same product logic
- do not silently introduce new product behavior
- preserve domain semantics unless the task explicitly requires a domain change

## Styling discipline

Use styling to reinforce:
- neutral base
- compact controls
- strong hierarchy
- subtle borders
- restrained accent usage
- high information density

Do not use styling changes as a substitute for layout and interaction fixes.

## Completion rule for UX tasks

Do not report a surface task as done unless:
1. the touched surface aligns with docs/00_product/surface-system.md
2. the touched area removes legacy anti-patterns where relevant
3. desktop and mobile preserve the same core product logic
4. changed code is consistent with the target surface spec
5. relevant docs/specs are updated if implementation decisions changed