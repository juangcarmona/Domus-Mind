# Spec — Toggle Shared List Item

## Purpose

Toggle the checked state of an item in a shared list.

---

## Context

- Module: Shared Lists
- Aggregate: `SharedList`
- Slice: `toggle-shared-list-item`
- Command: `ToggleSharedListItem`

---

## Inputs

Required:

- `sharedListId`
- `itemId`

Optional:

- `updatedByMemberId`

---

## Preconditions

- shared list must exist
- item must exist in the target list

---

## Behavior

The system toggles the checked state of the target item.

Semantics:

- unchecked → checked means the item has been handled for now
- checked → unchecked means the item is relevant again for the next use of the list

The item update must also refresh item update metadata.

---

## Result

Return:

- `itemId`
- `checked`
- `updatedAtUtc`
- `updatedByMemberId` (if provided)
- `uncheckedCount`

---

## Failure Cases

- shared list not found
- item not found

---

## Notes

This operation must feel instantaneous and support real-time shared usage across multiple family members.