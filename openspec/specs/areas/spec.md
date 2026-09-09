# Areas Specification

## Purpose

Areas represent household accountability structure. Each Area is a named domain of household management — such as school, food, finances, or maintenance — with explicit ownership assigned to family members.

The Areas surface answers: which household areas exist, who owns each one, who supports each one, and where ownership is missing.

Areas model accountability, not execution. Owning an Area means being accountable for that domain of household life; it does not imply direct execution of every related task, plan, or routine.

Areas are scoped to the household (family). Other capabilities — tasks, plans, routines, calendar events — may reference an Area as organizational context, but only the Responsibilities context may change ownership assignments.

In the domain model, an Area is a **Responsibility Domain**. In the product surface and household-facing language, it is an **Area**. The product roles **Owner** and **Support** correspond to the domain roles **Primary Owner** and **Secondary Owner**.

---

## Requirements

### Requirement: Area Creation

A household SHALL be able to create a named Area scoped to the family.

An Area name is required. Ownership is optional at creation — an Area may exist without an assigned owner.

Other capabilities (tasks, plans, meal plans) may optionally reference an Area for categorization and accountability context. Sparse usage is acceptable; the household functions without needing every Area to be fully configured.

Default Areas may be bootstrapped for a newly created household to reduce setup friction.

#### Scenario: Household creates an Area

- GIVEN a family exists
- WHEN the household creates an Area with a valid name
- THEN the Area is created scoped to that family
- AND it has no owner assigned by default

---

### Requirement: Primary Owner Assignment

An Area SHALL have at most one primary owner at any time.

The primary owner is the member accountable for that Area. The primary owner must belong to the same family as the Area.

Assigning a primary owner when one is already set replaces the previous owner. Transfer of primary ownership must always result in exactly one active primary owner.

#### Scenario: Household assigns a primary owner to an Area

- GIVEN an Area exists with no primary owner
- AND a family member exists
- WHEN the household assigns that member as the primary owner
- THEN the member becomes the primary owner of the Area
- AND the Area moves from an unowned to an owned state

#### Scenario: Primary owner is replaced

- GIVEN an Area already has a primary owner
- WHEN the household assigns a different member as the primary owner
- THEN the new member becomes the primary owner
- AND the previous primary owner no longer holds that role

#### Scenario: Non-family member cannot be assigned as owner

- GIVEN a member that does not belong to the family
- WHEN the household attempts to assign that member as primary owner of a family Area
- THEN the assignment is rejected

---

### Requirement: Secondary Owner Assignment

An Area MAY have one or more secondary owners.

Secondary owners provide backup or shared accountability coverage for the Area. Each secondary owner must belong to the same family as the Area. Secondary owners must be unique within the Area — the same member cannot be assigned as secondary owner more than once.

A secondary owner does not replace the primary owner.

#### Scenario: Household adds a secondary owner to an Area

- GIVEN an Area exists
- AND a family member is not already a secondary owner of that Area
- WHEN the household assigns the member as a secondary owner
- THEN the member is added to the secondary owner set
- AND the primary owner is unchanged

#### Scenario: Duplicate secondary owner is rejected

- GIVEN a member is already a secondary owner of an Area
- WHEN the household attempts to assign the same member as a secondary owner again
- THEN the assignment is rejected

---

### Requirement: Secondary Owner Removal

A household SHALL be able to remove a secondary owner from an Area.

Removing a secondary owner does not affect the primary owner or other secondary owners.

#### Scenario: Household removes a secondary owner

- GIVEN an Area has at least one secondary owner
- WHEN the household removes one of the secondary owners
- THEN that member is no longer a secondary owner of the Area
- AND all other secondary owners remain unchanged
- AND the primary owner is unchanged

---

### Requirement: Responsibility Transfer

A household SHALL be able to explicitly transfer primary ownership of an Area to another member.

Transfer is an explicit, auditable operation — not an implicit overwrite. The Area remains active and owned throughout the transfer. After transfer, exactly one active primary owner exists.

The new primary owner must belong to the same family as the Area.

#### Scenario: Household transfers Area ownership

- GIVEN an Area has a primary owner
- AND another family member exists
- WHEN the household transfers primary ownership to the new member
- THEN the new member becomes the primary owner
- AND the previous primary owner no longer holds the primary owner role
- AND the Area remains active

#### Scenario: Transfer to a non-family member is rejected

- GIVEN a member that does not belong to the family
- WHEN the household attempts to transfer primary ownership to that member
- THEN the transfer is rejected
- AND the existing primary owner is unchanged

---

### Requirement: Area Renaming

A household SHALL be able to rename an Area.

#### Scenario: Household renames an Area

- GIVEN an Area exists
- WHEN the household provides a new name for the Area
- THEN the Area is updated with the new name

---

### Requirement: Area Archiving

A household SHALL be able to archive an Area.

Archived Areas are retained and visible when filtered for, but are distinguished from active Areas.

#### Scenario: Household archives an Area

- GIVEN an Area exists
- WHEN the household archives it
- THEN the Area is marked as archived
- AND it is excluded from the default active view
- AND it remains visible when the archived filter is applied

---

### Requirement: Ownership Visibility

The Areas surface SHALL surface ownership gaps visibly.

The default ordered view prioritizes unowned Areas first, then partially assigned Areas, then fully assigned Areas, then archived Areas. This ordering keeps accountability gaps in view without requiring the household to search for them.

An Area is considered unowned when no primary owner is assigned. An Area is considered partially assigned when a primary owner is assigned but no secondary owner exists (this distinction is informational, not a hard system state).

Each Area row shows the Area name, primary owner (or a gap indicator if unowned), and support members if present.

#### Scenario: Unowned Area appears at the top of the list

- GIVEN a household has a mix of owned and unowned Areas
- WHEN the household views the Areas surface
- THEN unowned Areas appear before owned Areas in the default view

#### Scenario: Ownership gap is indicated on the Area row

- GIVEN an Area with no primary owner
- WHEN the household views the Areas list
- THEN a visible gap indicator is shown in place of the owner

---

### Requirement: Cross-Context Referencing

Other household capabilities — including tasks, plans, routines, and calendar events — MAY reference an Area for organizational context.

Referencing an Area does not change ownership assignments. Ownership belongs exclusively to the Responsibilities context. Other contexts use Area identity as a categorization anchor only.

#### Scenario: A task references an Area

- GIVEN a task is created with a reference to an Area
- THEN the Area context is recorded on the task as a categorization reference
- AND the Area's ownership assignments are not affected

---

## Notes

### Terminology convergence

"Area" (product surface) and "Responsibility Domain" (domain model) refer to the same concept. All repository sources are consistent on this mapping. "Owner" maps to Primary Owner; "Support" maps to Secondary Owner. This spec uses Area, Owner, and Support when describing product-facing behavior, and notes the domain terms where relevant.

`docs/04_contexts/areas.md` does not exist in this repository. The context document for this domain lives at `docs/04_contexts/responsibilities.md`. The Areas label is used by the product surface (`00_product/surfaces/areas.md`) and the navigation entry point.

### AssignPrimaryOwner vs. TransferResponsibility

Both operations result in a new primary owner. The distinction documented in the source files is: direct assignment (when setting an owner for the first time or implicitly replacing one) versus explicit transfer (an intentional, auditable change of accountability between two named members). The behavioral difference in terms of system outcome is not fully specified. This should be clarified before implementing both commands.

### Participant role

The domain model defines a Participant role (a member involved in an Area but not primarily accountable). Commands for adding and removing participants exist in the domain. However, the surface spec does not expose participants as a first-class product element — participant detail is described as "secondary" and shown only in the inspector or secondary metadata. No feature spec exists for participant management. This spec does not include a Participant requirement; it should be added when a feature spec is created.

### Area color

The surface spec describes an Area color cue (colored dot, changeable inline via the inspector). This is referenced in the Areas surface spec but is absent from the domain model and all feature specs. It is not included as a behavioral requirement here. It should be specified as a feature before implementation.

### Archive: no removal behavior specified

The repository specifies archiving but does not document reactivation or permanent deletion of Areas. `ReactivateResponsibilityDomain` is listed as a future command in the context document. This spec covers archiving only.

---

## Source References

- `docs/04_contexts/responsibilities.md`
- `00_product/surfaces/areas.md`
- `specs/features/responsibilities/create-responsibility-domain.md`
- `specs/features/responsibilities/assign-primary-owner.md`
- `specs/features/responsibilities/assign-secondary-owner.md`
- `specs/features/responsibilities/transfer-responsibility.md`
- `docs/03_domain/ubiquitous-language.md`
