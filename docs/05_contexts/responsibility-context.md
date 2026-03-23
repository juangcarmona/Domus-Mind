# DomusMind — Areas

## Purpose

Areas provide a lightweight way to classify and visually organize household activity.

They are used to:

- group related items
- improve readability of the timeline
- enable visual differentiation (e.g., colors)
- support simple filtering and recognition

Areas are optional and require no configuration.

---

## Concept

An Area represents a mental category used by the household.

Examples:

- house
- pets
- family
- leisure
- admin

Areas are not strict domains and do not enforce any behavior.

They exist only to improve clarity.

---

## Characteristics

Areas are:

- optional
- flat (no hierarchy)
- lightweight
- user-driven or inferred
- non-authoritative

Areas must never:

- require configuration before usage
- enforce ownership or accountability
- drive business rules
- introduce workflow constraints

---

## Usage

Areas are attached to timeline entries:

- Call vet → pets
- Fix sink → house
- Review ski offers → leisure

They may be used for:

- grouping items in the UI
- assigning default colors
- filtering views
- improving recognition

---

## Data Model (Minimal)

Area
- id
- name
- color
- familyId

TimelineEntry
- areaId? (optional)

---

## Behavior Rules

- area is optional for every entry
- users can create areas implicitly while creating entries
- system may suggest areas based on previous usage
- no dedicated management interface is required for V1

---

## Design Intent

Areas exist to reduce cognitive load by making household activity easier to scan and understand.

They do not model responsibility, ownership, or execution.

---

## Summary

Areas are a visual and cognitive aid, not a structural domain.

They improve clarity without adding friction or maintenance overhead.
