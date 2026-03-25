# Spec — Add Item To Shared List

## Purpose

Add a new item to a shared list.

---

## Context

* Aggregate: `SharedList`
* Slice: `add-item-to-shared-list`

---

## Inputs

Required:

* `sharedListId`
* `name`

Optional:

* `quantity`
* `note`
* `addedByMemberId`

---

## Preconditions

* list must exist
* name must be valid

---

## Behavior

* append new item
* assign next order position
* default state = unchecked

---

## Result

* item ID
* updated list state

---

## Failure

* list not found
* invalid name

---

## Notes

This is the **critical path interaction**. Must be minimal and fast.
