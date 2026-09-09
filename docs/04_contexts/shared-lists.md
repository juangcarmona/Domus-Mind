# Shared Lists Context

Status: Transitional
Canonical upstream: 00_product/surfaces/lists.md
Do not use this document to override canonical product docs/specs

## Purpose

The Lists context defines the **household execution container** for captured items.

It supports a spectrum from memory → action → time projection.

This context answers:

* what items should be remembered, bought, checked, or prepared
* what shared state exists across the household
* which items have temporal context and may appear in the Agenda

A list is not a dumb checklist.
An item is not a dumb entry.

Items are **polymorphic execution units**.
They may remain lightweight memory, or they may carry importance, temporal, or action semantics — all within the same container.

This context does not own:

* structured task lifecycle (Tasks)
* time as a first-class resource (Calendar)
* accountability and ownership (Responsibilities)

---

## Responsibilities

The Shared Lists context is responsible for:

* creating and managing shared lists
* managing list items and their capabilities
* maintaining checked/unchecked state
* maintaining item importance
* maintaining item temporal fields (due date, reminder, repeat)
* supporting persistent reuse of lists over time
* enabling real-time shared updates across family members
* optionally linking lists to other household entities
* providing projection data to the Agenda surface for temporally-enriched items
* receiving shopping lists generated from meal plans (as a `SharedList` of kind `shopping`)

---

## Aggregate Roots

### SharedList

Represents a persistent household execution container.

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

Represents a single polymorphic execution unit within a list.

#### Base capabilities (always present)

* name
* checked state
* optional quantity
* optional note
* order
* last update metadata

#### Optional capabilities

Items may additionally carry:

* **importance** — a flag marking the item as starred or high attention
* **temporal** — due date, reminder, and/or repeat rule

Not every item will carry all capabilities.
Capability presence or absence is part of the item's state.

Items with temporal capabilities may project into the Agenda surface.
Items without temporal capabilities remain list-only.

Items never become Tasks automatically.
No implicit task creation occurs.

---

## Value Objects (suggested)

* SharedListId
* SharedListItemId
* SharedListName
* SharedListItemName
* SharedListKind
* ListItemOrder
* ItemImportance
* ItemDueDate
* ItemReminderDefinition
* ItemRepeatRule

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
* importance is a binary flag, not a scoring system
* `repeat` may be set independently of `dueDate` — it defines a recurrence schedule that is itself a temporal anchor for Agenda projection
* if `repeat` is set and `dueDate` is also set, the repeat rule governs projected occurrences; `dueDate` sets the anchor date for the first (or next) occurrence
* if `dueDate` is cleared but `repeat` remains set, the item remains Agenda-eligible via `repeat`

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

* SetSharedListItemImportance

* SetSharedListItemTemporal (sets due date, reminder, repeat)

* ClearSharedListItemTemporal

---

## Queries

* GetFamilySharedLists
* GetSharedListDetail
* GetTemporalItemsForAgenda (projection query — items with temporal fields, scoped by date window)

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

* SharedListItemImportanceSet

* SharedListItemScheduled (emitted when an item receives its first temporal field)

---

## Domain Events (emitted by other contexts — reacted to)

From Meal Planning (V2):

* `ShoppingListRequested` — Shared Lists reacts by creating a new `SharedList` of kind `shopping` prefilled with consolidated ingredients from the meal plan. Shared Lists emits `SharedListCreated` after creation.

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
  * id
  * name
  * checked
  * quantity
  * note
  * importance
  * dueDate
  * reminder
  * repeat
  * order

### AgendaItemProjection

* itemId
* listId
* listName
* name
* checked
* importance
* dueDate
* reminder
* repeat

Used by Agenda to render temporally-enriched list items.

---

## Boundaries With Other Contexts

### Family

Provides identity. Shared Lists depend on Family but do not modify it.

### Responsibilities

Lists may optionally reference an area but do not enforce ownership logic.

### Calendar

Calendar owns time.
Lists do not own time; they **reference** time via item temporal fields.
Items with due dates and reminders **project into** the Agenda surface.
This projection is read-only from Calendar's perspective.
Lists do not affect event scheduling.

### Tasks

Lists own **lightweight, flexible execution** via items.
Tasks own **structured, explicit execution** with full lifecycle, assignment, and management.
These are distinct models.
A list item does not become a task automatically.
No command from either context crosses into the other's aggregate.

---

## Design Notes

Shared Lists represent **persistent shared state**, not execution or time.

Key properties:

* reusable
* toggle-based state
* no completion lifecycle
* no scheduling

## Meal Planning Extension Notes

Meal planning extends the Lists context to support:
* Weekly meal planning with templates
* Recipe management
* Shopping list generation
* Integration with existing Lists and Agenda surfaces

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

## create-meal-plan

Creates a new weekly meal plan.

Input:

* familyId
* weekStart (Monday date)
* optional templateId

Output:

* mealPlanId

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
