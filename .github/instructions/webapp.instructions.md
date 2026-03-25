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