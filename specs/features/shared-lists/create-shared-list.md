# Spec — Create Shared List

## Purpose

Create a new persistent shared list owned by a family.

---

## Context

* Module: Shared Lists
* Aggregate: `SharedList`
* Slice: `create-shared-list`
* Command: `CreateSharedList`

---

## Inputs

Required:

* `familyId`
* `name`
* `kind`

Optional:

* `areaId`
* `primaryOwnerId`
* `supportOwnerId`
* `linkedEntityType`
* `linkedEntityId`

---

## Preconditions

* family must exist
* name must be valid and non-empty
* optional owners must belong to the family
* linkage must be structurally valid

---

## Behavior

* create new `SharedList` aggregate
* assign identity and ownership
* initialize with empty item collection
* apply optional metadata (area, owners, linkage)

---

## Result

* list ID
* name
* metadata

---

## Failure

* invalid family
* invalid name
* invalid ownership
* invalid linkage

---

## Notes

List creation must be low-friction and support immediate usage.
