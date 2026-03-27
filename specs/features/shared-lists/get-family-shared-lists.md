# Spec - Get Family Shared Lists

## Purpose

Retrieve all shared lists belonging to a family.

---

## Context

- Module: Shared Lists
- Read Model: `SharedListSummary`
- Slice: `get-family-shared-lists`
- Query: `GetFamilySharedLists`

---

## Inputs

Required:

- `familyId`

---

## Preconditions

- family must exist

---

## Query Behavior

The system retrieves all active shared lists for the given family.

Lists are returned as summaries, not full detail.

---

## Result Structure

Each list includes:

- `id`
- `name`
- `kind`
- `areaId`
- `linkedEntityType`
- `linkedEntityId`
- `itemCount`
- `uncheckedCount`

---

## Success Result

Return:

- list of shared list summaries

---

## Failure Cases

- family not found

---

## Notes

This is the primary entry point for accessing shared lists at family level.

It must be optimized for fast loading and frequent access.