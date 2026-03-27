# Spec - Get Shared List Detail

## Purpose

Retrieve full detail of a shared list.

---

## Context

- Module: Shared Lists
- Read Model: `SharedListDetail`
- Slice: `get-shared-list-detail`
- Query: `GetSharedListDetail`

---

## Inputs

Required:

- `sharedListId`

---

## Preconditions

- shared list must exist

---

## Query Behavior

The system retrieves the target shared list and returns its full detail including ordered items.

---

## Result

Return:

- `listId`
- `name`
- `kind`
- `areaId`
- `linkedEntityType`
- `linkedEntityId`

- `items[]`:

  - `itemId`
  - `name`
  - `checked`
  - `quantity`
  - `note`
  - `order`
  - `updatedAtUtc`
  - `updatedByMemberId`

---

## Success Result

Return:

- shared list metadata
- ordered list items

---

## Failure Cases

- shared list not found

---

## Notes

This query must support real-time UI rendering and concurrent household usage.