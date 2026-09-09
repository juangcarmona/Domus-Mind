---
applyTo: "docs/**/*.md,specs/**/*.md,README.md"
---

# Docs and Specs Instructions

## Role

Docs and specs are the source of truth for behavior, terminology, and architecture.

## Rules

- use existing terminology; do not invent new terms
- keep wording precise and unambiguous
- keep structure consistent with nearby documents

## Consistency

- do not introduce conflicting explanations
- if something changes in code, update the relevant docs/specs
- if docs disagree, align them instead of adding a third version

## Style

- keep sections short and structured
- prefer bullet points over long paragraphs

## Canonical precedence

When working on UX, surfaces, layout, or interaction behavior, treat these as canonical and upstream:

- docs/00_product/strategy.md
- docs/00_product/experience.md
- docs/00_product/surface-system.md
- 00_product/surfaces/*.md

Do not let implementation files, old comments, or outdated docs override these documents.
If code conflicts with them, update code or explicitly document the blocker.

## Maintenance Rules

To prevent future drift and maintain documentation quality:

1. If a surface is replaced, the old spec must be retired in the same PR
2. If terminology changes, update ubiquitous language and system spec in the same PR  
3. No new instruction file may reference retired specs
4. No feature spec may keep old terminology without an explicit migration note