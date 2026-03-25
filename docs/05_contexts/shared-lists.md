# Shared Lists Context

## Purpose

The Shared Lists context defines persistent, reusable, shared checklists within a household.

It represents **stateful coordination**, not time-based execution.

This context answers:

* what items should be remembered for future actions
* what needs to be bought, checked, or prepared next time a situation occurs
* what shared state exists across the household

It does not represent:

* scheduled commitments (Calendar)
* executable work (Tasks)
* responsibility ownership (Responsibilities)

---

## Responsibilities

The Shared Lists context is responsible for:

* creating and managing shared lists
* managing list items
* maintaining checked/unchecked state
* supporting persistent reuse of lists over time
* enabling real-time shared updates across family members
* optionally linking lists to other household entities

---

## Aggregate Roots

### SharedList

Represents a persistent shared checklist.

Owns:

* identity
* name
* type/kind
* optional area association
* optional ownership
* optional linkage to another entity
* collection of items

---

### SharedListItem

Represents a single item within a list.

Owns:

* name
* checked state
* optional quantity
* optional note
* order
* last update metadata

---

## Value Objects (suggested)

* SharedListId
* SharedListItemId
* SharedListName
* SharedListItemName
* SharedListKind
* ListItemOrder

---

## Invariants

### SharedList

* must belong to exactly one family
* must have a stable identifier
* name cannot be empty

### SharedListItem

* must belong to exactly one list
* name cannot be empty
* order must be unique within a list

---

## Commands

* CreateSharedList

* RenameSharedList

* DeleteSharedList

* LinkSharedList

* UnlinkSharedList

* AddItemToSharedList

* UpdateSharedListItem

* RemoveSharedListItem

* ToggleSharedListItem

* ReorderSharedListItems

---

## Queries

* GetFamilySharedLists
* GetSharedListDetail

---

## Domain Events (emitted)

* SharedListCreated

* SharedListRenamed

* SharedListDeleted

* SharedListLinked

* SharedListUnlinked

* SharedListItemAdded

* SharedListItemUpdated

* SharedListItemRemoved

* SharedListItemToggled

---

## Domain Events (consumed)

From Family:

* FamilyCreated
* MemberAdded
* MemberRemoved

From other contexts (optional linkage only):

* EventScheduled

---

## Read Models

### SharedListSummary

* id
* name
* kind
* areaId
* linkedEntity
* itemCount
* uncheckedCount

### SharedListDetail

* id
* name
* kind
* items[]

---

## Boundaries With Other Contexts

### Family

Provides identity. Shared Lists depend on Family but do not modify it.

### Responsibilities

Lists may optionally reference an area but do not enforce ownership logic.

### Calendar

Lists may be linked to events but do not affect scheduling.

### Tasks

Lists do not generate tasks and are not part of task execution.

---

## Design Notes

Shared Lists represent **persistent shared state**, not execution or time.

Key properties:

* reusable
* toggle-based state
* no completion lifecycle
* no scheduling

---

# Initial Feature Specs

## create-shared-list

Creates a new shared list.

Input:

* familyId
* name
* kind
* optional areaId

Output:

* listId

---

## add-item-to-shared-list

Adds a new item.

Input:

* listId
* name

Output:

* itemId

---

## toggle-shared-list-item

Toggles checked state.

Input:

* itemId

Output:

* updated state

---

## get-family-shared-lists

Returns all lists for a family.

Input:

* familyId

Output:

* SharedListSummary[]

---

## get-shared-list-detail

Returns full list with items.

Input:

* listId

Output:

* SharedListDetail
