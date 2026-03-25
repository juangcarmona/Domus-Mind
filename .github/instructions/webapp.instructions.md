---
applyTo: "src/web/app/src/**/*.{ts,tsx}"
---

# Web App Instructions

## Structure

- follow existing feature-based structure
- reuse existing API and state patterns
- do not introduce parallel data-fetching or state systems

## Components

- keep components small and focused
- split components when they mix data fetching, state, and rendering
- avoid large page components

## Rules

- do not duplicate forms or logic across features
- reuse shared components and editors when available
- keep types strict (avoid `any`)

## Changes

- prefer extending existing features over creating new patterns
- keep UI logic simple and predictable

## Style Architecture

### Web app style structure

- global foundation styles live in src/web/app/src/styles
- shared reusable component styles live with src/web/app/src/components
- feature-specific styles live inside the owning feature folder under src/web/app/src/features
- never default to appending styles to a catch-all stylesheet

### Styling decision order

- before adding styles, decide ownership in this order: tokens, base, layout, shared component style, feature style

### Reuse rules

- reuse shared primitives before creating feature-local variants
- extend existing button/card/badge/modal/form/section-header patterns before inventing new classes
- keep hover/focus/selected/today/overdue semantics consistent

### Naming rules

- use c- for shared component classes
- use l- for layout object classes
- use u- for utility classes
- use is- and has- for state classes
- use feature-prefixed names only for genuine feature-specific styles

### Product language rule

- use product-facing household language in UI-facing labels, class names, and style grouping
- preferred terms: Household, Person, Task, Plan, Area
- avoid backend/internal terminology in UI naming

### File discipline

- keep files small and split by responsibility
- colocate styles with ownership
- avoid duplicate styling systems for the same UI primitive

### Agent behavior rule

- “When adding styles, never append to a catch-all stylesheet by default. First decide whether the change belongs to tokens, base, layout, shared component styles, or a feature stylesheet. Reuse existing interaction and surface patterns before creating new ones.”