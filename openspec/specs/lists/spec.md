# Lists Specification

## Purpose

Lists are the household's persistent execution containers. A list answers: what should be remembered, bought, checked, prepared, or done next time?

A list is not a task board. A list item is not a task. Lists own capture and flexible execution across a spectrum: from a plain memory entry (name only) to a time-aware, importance-flagged item that projects into the Agenda surface.

Lists are reusable by design. Items are consumable within a list. The list persists across uses.

Lists are scoped to the household (family). They may be optionally associated with a household Area or a Plan as contextual anchors, but these links are informational — linking a list to a plan does not convert its items to scheduled work, and linking to an Area does not affect ownership or responsibility.

List state is shared in real time across household members — additions, toggles, and updates are visible to all without a manual refresh.

---

## Requirements

### Requirement: List Creation

A household SHALL be able to create a list with a name as the only required input.

Optional associations at creation: an Area (contextual memory for that Area), a Plan (list is used in context of that plan), and a kind (system-level classification). All optional fields may also be set or changed after creation.

A list is initialized with an empty item collection.

#### Scenario: Household creates a list with a name only

- GIVEN a family exists
- WHEN the household creates a list with a valid name and no other fields
- THEN the list is created and available for use
- AND it is initialized with no items

#### Scenario: Household creates a list linked to a plan

- GIVEN a family exists
- WHEN the household creates a list with a name and a linked plan
- THEN the list is created and associated with the plan
- AND the list retains list semantics — items are not converted to tasks or scheduled entries

---

### Requirement: List Retrieval

A household SHALL be able to retrieve a summary of all active lists for the family.

Each summary includes the list name and unchecked item count. Archived lists are excluded from the default result.

#### Scenario: Household views the list switcher

- GIVEN a family has several active lists
- WHEN the household requests the family's lists
- THEN all active lists are returned with their names and unchecked item counts
- AND archived lists are not included

#### Scenario: Archived list is excluded

- GIVEN a list has been archived
- WHEN the household requests the family's active lists
- THEN the archived list does not appear in the results

---

### Requirement: List Detail

A household SHALL be able to retrieve the full content of a list, including all items.

Items are returned in two groups: unchecked items first, then checked items. Within each group, items are ordered by their stable display order. All items are included regardless of checked state.

#### Scenario: Household opens a list

- GIVEN a list exists with a mix of checked and unchecked items
- WHEN the household retrieves the list detail
- THEN all items are returned
- AND unchecked items appear before checked items

---

### Requirement: List Update

A household SHALL be able to update a list's name and optional associations.

Updatable fields: name, Area association, linked Plan, kind. At least one field must be provided. Fields not included in the request are unchanged. Area association and Plan linkage may be cleared explicitly.

#### Scenario: Household renames a list

- GIVEN a list exists
- WHEN the household provides a new name
- THEN the list is updated with the new name
- AND no other fields are affected

#### Scenario: Household removes a plan link

- GIVEN a list is linked to a plan
- WHEN the household updates the list and explicitly clears the plan link
- THEN the list is no longer linked to the plan
- AND the list's items are unaffected

---

### Requirement: List Archive

A household SHALL be able to archive a list that is no longer in active use.

Archiving transitions the list out of the active collection. All data is preserved — items retain their names, quantities, notes, and checked states. No items are removed or modified. The archive operation has no effect on linked Areas, Plans, or any other context.

A list that is already archived cannot be archived again.

#### Scenario: Household archives a list

- GIVEN an active list exists
- WHEN the household archives it
- THEN the list is no longer returned in the default active list query
- AND all items remain intact

#### Scenario: Household attempts to archive an already-archived list

- GIVEN a list is already archived
- WHEN the household attempts to archive it again
- THEN the operation is rejected

---

### Requirement: List Restore

A household SHALL be able to restore an archived list to active use.

Restore is the symmetric inverse of archive. No item data is modified. The restored list behaves identically to any other active list. A list that is not archived cannot be restored.

#### Scenario: Household restores an archived list

- GIVEN an archived list exists
- WHEN the household restores it
- THEN the list appears again in the default active list query
- AND all items are in the same state as when the list was archived

---

### Requirement: Item Addition

A household SHALL be able to add a new item to an active list with only a name.

A new item is always created in an unchecked state. Optional fields at creation: quantity and note. Items are appended in stable order — each new item receives the next position. Importance and temporal fields are not set at item creation; they are applied through dedicated operations after creation.

#### Scenario: Household adds an item to a list

- GIVEN a list exists
- WHEN the household adds an item with a name
- THEN the item is created unchecked at the end of the list
- AND it is immediately available in the list

#### Scenario: Sequential item capture

- GIVEN the household adds multiple items in sequence
- THEN each item is appended in the order it was added
- AND no modal or interruption is required between items

---

### Requirement: Item Update

A household SHALL be able to update the base fields of a list item: name, quantity, and note.

At least one of these fields must be provided. Fields not included are left unchanged. Quantity may be cleared explicitly. This operation does not affect checked state, importance, temporal fields, or item order.

#### Scenario: Household updates an item's name

- GIVEN a list item exists
- WHEN the household provides a new name
- THEN the item name is updated
- AND the item's checked state, importance, and temporal fields are unchanged

#### Scenario: Household clears an item's quantity

- GIVEN a list item has a quantity set
- WHEN the household explicitly clears the quantity
- THEN the item no longer has a quantity
- AND the item name and other fields are unchanged

---

### Requirement: Item Toggle

A household SHALL be able to toggle the checked state of any item in a list.

Toggle is binary: unchecked → checked (item is handled for this use), or checked → unchecked (item is relevant again for the next use). Toggling an item does not remove it from the list, does not affect its importance or temporal fields, and has no effects outside the list.

A checked item with temporal fields continues to project into the Agenda surface in a de-emphasized state. Toggle does not clear temporal fields.

#### Scenario: Household checks an item

- GIVEN an unchecked item exists in a list
- WHEN the household toggles it
- THEN the item becomes checked
- AND it remains in the list

#### Scenario: Household unchecks an item

- GIVEN a checked item exists in a list
- WHEN the household toggles it
- THEN the item becomes unchecked
- AND it is treated as relevant again

#### Scenario: Checked item with a due date remains in Agenda

- GIVEN an item has a due date and is checked
- WHEN the household views the Agenda for that date
- THEN the item still appears in a de-emphasized state
- AND it is not removed from projection until its temporal fields are cleared

---

### Requirement: Item Removal

A household SHALL be able to permanently remove an item from a list.

Removal is immediate and irreversible. The item is deleted from the list's ordered collection. If the removed item had temporal fields, it is removed from Agenda projection. Removal has no effects outside the list.

#### Scenario: Household removes an item

- GIVEN a list item exists
- WHEN the household removes it
- THEN the item is no longer present in the list
- AND the remaining items are unchanged

#### Scenario: Removing an item with temporal fields clears its Agenda projection

- GIVEN a list item with a due date is projecting into Agenda
- WHEN the household removes the item
- THEN the item no longer appears in Agenda

---

### Requirement: Item Reorder

A household SHALL be able to set the display order of items within a list.

The operation takes the complete intended order as a full replacement. The provided list must contain exactly the current set of items — no additions, no omissions. Order carries no semantic meaning (no priority, urgency, or importance). Checked and unchecked items are part of the same ordered sequence; display grouping is a surface concern.

#### Scenario: Household reorders items

- GIVEN a list with several items
- WHEN the household provides a complete new order for those items
- THEN each item is assigned its new position
- AND no item field other than order is modified

#### Scenario: Reorder with mismatched item set is rejected

- GIVEN a list with three items
- WHEN the household provides a reorder containing only two of the three items
- THEN the operation is rejected
- AND the current item order is preserved

---

### Requirement: Item Importance

A household SHALL be able to mark or unmark any list item as important.

Importance is a binary flag (starred / not-starred). It is not a score or ranking. Setting importance to a value that is already set is a no-op. Importance does not affect temporal fields, checked state, or Agenda projection eligibility.

#### Scenario: Household marks an item as important

- GIVEN a list item with no importance set
- WHEN the household sets importance to true
- THEN the item is marked as starred
- AND no other item fields are affected

#### Scenario: Setting importance is idempotent

- GIVEN a list item already marked as important
- WHEN the household sets importance to true again
- THEN the operation succeeds without error
- AND the item state is unchanged

---

### Requirement: Item Temporal Assignment

A household SHALL be able to assign temporal fields to a list item: due date, reminder, and/or repeat rule.

At least one temporal field must be provided per operation. Fields not included in the request are left unchanged. Setting any temporal field makes the item eligible for Agenda projection. Setting temporal fields does not convert the item to a task and does not create a calendar event.

When an item transitions from having no temporal fields to having at least one, this is treated as a distinct scheduling event. Subsequent updates to already-set temporal fields are treated as updates.

#### Scenario: Household sets a due date on an item

- GIVEN a list item with no temporal fields
- WHEN the household sets a due date
- THEN the item becomes eligible for Agenda projection on that date
- AND the item is not converted to a task

#### Scenario: Household sets a reminder without a due date

- GIVEN a list item with no temporal fields
- WHEN the household sets only a reminder (absolute date-time)
- THEN the item becomes eligible for Agenda projection at the reminder time
- AND no due date is required

#### Scenario: Household sets a repeat rule

- GIVEN a list item
- WHEN the household sets a repeat rule
- THEN the item becomes eligible for Agenda projection on each recurrence occurrence
- AND the repeat rule alone is sufficient for projection eligibility

---

### Requirement: Item Temporal Clearing

A household SHALL be able to remove all temporal fields from a list item in a single operation.

All three fields — due date, reminder, and repeat rule — are cleared atomically. After clearing, the item no longer projects into the Agenda surface. The item is not deleted and all other fields are unchanged.

If the item has no temporal fields, the operation is a no-op and succeeds silently.

#### Scenario: Household clears temporal fields from an item

- GIVEN a list item with a due date and a reminder
- WHEN the household clears the item's temporal fields
- THEN the item no longer appears in Agenda
- AND the item name, importance, checked state, and other fields are unchanged

#### Scenario: Clearing temporal fields on an item with none is safe

- GIVEN a list item with no temporal fields
- WHEN the household clears the temporal fields
- THEN the operation succeeds without error
- AND the item is unchanged

---

### Requirement: Agenda Projection

List items with any temporal field SHALL project into the Agenda surface as a distinct entry type.

An item projects when any of the following conditions is satisfied:
- its due date falls within the Agenda's requested date window
- its reminder datetime falls within the Agenda's requested date window
- its repeat rule produces an occurrence within the requested date window

All three conditions are independently sufficient. No temporal field requires another as a prerequisite.

Projected list items are read-only in Agenda. They carry a visible list-origin cue. Editing an item is done through the Lists surface only.

Checked items that meet a projection condition still appear in Agenda, de-emphasized. They are not removed from projection until their temporal fields are cleared.

#### Scenario: Item with due date appears in Agenda on that date

- GIVEN a list item with a due date set to today
- WHEN the Agenda for today is loaded
- THEN the item appears as a projected list item with a list-origin cue
- AND it is visually distinct from tasks and plans

#### Scenario: Checked item continues to project

- GIVEN a list item with a due date that has been checked
- WHEN the Agenda for that date is loaded
- THEN the item still appears in Agenda
- AND it is de-emphasized

#### Scenario: Item is removed from Agenda after temporal fields are cleared

- GIVEN a list item that was projecting into Agenda
- WHEN the household clears all temporal fields
- THEN the item no longer appears in Agenda
- AND the item continues to exist in the list

---

## Notes

### Terminology: "Shared Lists" vs "Lists"

The context document (`docs/04_contexts/shared-lists.md`) and the item model document use "Shared Lists" and "SharedList" throughout. The product surface (`00_product/surfaces/lists.md`) and feature specs use "Lists". This spec uses "Lists" as the product-facing term. The transition is incomplete in the domain docs; "SharedList" remains the aggregate name in the domain model.

### Contradiction: `repeat` independence — Resolved

The `set-item-temporal` feature spec previously stated that `repeat` requires a `dueDate` to be present, contradicting the item model document which states that `repeat` is independently sufficient for Agenda projection. This contradiction is resolved by `docs/01_system/system-spec.md`, which explicitly states: "Repeat on a list item may be set independently of due date. Repeat is itself a temporal anchor sufficient for Agenda projection." The item model canonical position is confirmed. The `set-item-temporal` feature spec constraint was stale.

### `get-list-detail` item fields

The `get-list-detail` feature spec defines the per-item result shape as: name, quantity, note, checked, order — and explicitly excludes temporal and importance fields. This contradicts the item capability model, which makes these fields part of the item's full state. The item model document is marked as locking canonical state before implementation. The detail spec appears to predate it and should be updated to include importance and temporal fields in the item result.

### Partial temporal field clearing

`ClearSharedListItemTemporal` clears all three fields atomically. To clear only specific temporal fields individually, the `SetSharedListItemTemporal` operation is used by setting specific fields to null. This asymmetry is intentional per the source.

### Shopping list creation from Meal Planning

The context document notes that the Lists context receives shopping lists generated from Meal Planning as a `List` of kind `shopping`. The reception side behavior (how a shopping list lands in Lists) is owned by the Meal Planning spec, not this spec. The Lists context treats a shopping list as a regular list once created.

---

## Source References

- `docs/04_contexts/shared-lists.md`
- `docs/04_contexts/shared-lists-item-model.md`
- `00_product/surfaces/lists.md`
- `specs/features/lists/create-list.md`
- `specs/features/lists/update-list.md`
- `specs/features/lists/archive-list.md`
- `specs/features/lists/restore-list.md`
- `specs/features/lists/get-family-lists.md`
- `specs/features/lists/get-list-detail.md`
- `specs/features/lists/add-item-to-list.md`
- `specs/features/lists/update-list-item.md`
- `specs/features/lists/reorder-list-items.md`
- `specs/features/lists/toggle-list-item.md`
- `specs/features/lists/set-item-importance.md`
- `specs/features/lists/set-item-temporal.md`
- `specs/features/lists/clear-item-temporal.md`
