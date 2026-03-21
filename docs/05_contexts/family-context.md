# DomusMind — Family Context

## Purpose

The Family context defines the **household unit** and its internal human structure.

It is responsible for representing:

* the family itself
* its members
* dependents
* pets
* relationships between members

This context establishes the **core identity boundary** of DomusMind.

Most other contexts reference Family entities, but do not own or modify their structure. 

---

# Responsibilities

The Family context is responsible for:

* creating a family
* adding and removing members
* registering dependents
* registering pets
* defining relationships between members
* maintaining the household identity boundary

It is the source of truth for **who belongs to a family and how they relate to one another**.

---

# Aggregate Roots

## Family

The `Family` aggregate is the primary aggregate root of this context.

It owns:

* members
* dependents
* pets
* relationships

All modifications to family structure must occur through `Family`.

Other contexts may reference family entities but cannot change their structure.

---

# Internal Entities

## Member

Represents a person belonging to the household.

Members are typically **independent participants** in household coordination.

Examples:

* adult
* teenager
* caregiver

Members may:

* participate in plans (Calendar)
* receive task assignments (Tasks)
* hold responsibility ownership (Responsibilities)

---

## Dependent

Represents a household entity **requiring care or supervision** from members.

Dependents may include:

* children
* elderly relatives
* other care recipients

Dependents may appear as **participants in plans** but typically do not receive task assignments.

Example:

```
Child participates in school event
Parent is responsible for preparation tasks
```

Dependents may have relationships linking them to responsible members.

Example:

```
Child depends on Parent
```

These dependency relationships are defined **inside the Family context**.

---

## Pet

Represents an animal belonging to the household.

Pets are considered a special form of dependent.

Examples:

* dog
* cat
* rabbit

Pet identity is owned by the Family context. Pets can be registered as family members using the `Pet` role.

**V1 operational scope for pets:**

- Pets may be added to the family for identity and reference purposes
- Pets do **not** appear in coordination views (Today board, Week grid, Timeline) in V1
- Pets **cannot** receive task assignments in V1
- Pets **cannot** hold responsibility ownership in V1
- Pets **can** be added as event participants (e.g. a vet appointment)

The coordination surface filters members by operational roles (`Adult`, `Child`, `Caregiver`). The `Pet` role is intentionally excluded from this filter in V1.

Pet-specific operational capabilities (feeding schedules, vet visit tracking, pet care responsibilities) are deferred to V2 or later.

---

## Relationship

Represents a **structural relationship between two family entities**.

Examples:

* parent → child
* spouse ↔ spouse
* sibling ↔ sibling
* caregiver → dependent

Relationships express **dependency and responsibility structures inside the household**.

Example:

```
Child depends on Parent
```

These semantics are important for later operational rules, such as:

* determining responsible adults
* validating task assignment
* interpreting participant responsibility

---

# Value Objects

Suggested value objects:

* `FamilyId`
* `MemberId`
* `DependentId`
* `PetId`
* `RelationshipId`
* `MemberName`
* `PetName`
* `RelationshipType`
* `MemberRole`

Identifiers must remain **strongly typed**.

---

# Invariants

The Family aggregate must enforce the following invariants.

## Identity and Membership

* a family must have a stable `FamilyId`
* a member belongs to exactly one family
* member IDs must be unique within the family
* dependent IDs must be unique within the family
* pet IDs must be unique within the family

## Structural Integrity

* relationships must reference existing members of the same family
* a relationship cannot reference unknown entities
* a pet must belong to exactly one family
* a dependent must belong to exactly one family

## Dependency Semantics

The Family context defines **dependency relationships** between entities.

Typical patterns:

```
Parent → Child
Caregiver → Dependent
Owner → Pet
```

These relationships determine **who is responsible for dependents**.

Only the Family context defines these semantics.

Other contexts must not infer or modify dependency structures.

## Valid State Rules

* a removed member cannot participate in new relationships
* duplicate active members are not allowed
* duplicate active pets are not allowed
* duplicate relationships of the same type between the same entities should not exist unless explicitly modeled

## Ownership Boundary

* only the Family context may change membership or relationships
* other contexts must reference family entities **by ID only**

---

# Commands

Core commands owned by this context:

* `CreateFamily`
* `AddMember`
* `RemoveMember`
* `AddDependent`
* `RemoveDependent`
* `AddPet`
* `RemovePet`
* `AssignRelationship`
* `RemoveRelationship`

Suggested future commands:

* `RenameFamily`
* `UpdateMemberProfile`
* `ArchiveFamily`

---

# Queries

Core queries supported by this context:

* `GetFamily`
* `GetFamilyMembers`
* `GetFamilyDependents`
* `GetFamilyPets`
* `GetFamilyRelationships`
* `GetMemberById`

Suggested future queries:

* `GetFamilyStructure`
* `GetHouseholdRoster`

---

# Domain Events Emitted

The Family context emits:

* `FamilyCreated`
* `MemberAdded`
* `MemberRemoved`
* `DependentAdded`
* `DependentRemoved`
* `PetAdded`
* `PetRemoved`
* `RelationshipAssigned`
* `RelationshipRemoved`

These events are facts and must be emitted only after successful state change.

---

# Domain Events Consumed

The Family context should consume very few events.

In principle, it should not depend on downstream operational contexts such as:

* Tasks
* Calendar
* Responsibilities
* Food
* Inventory

Possible future consumed events:

* identity import events from external integrations
* household bootstrap events during migration/import flows

Default rule:

**Family is upstream. It emits more than it consumes.**

---

# Boundaries With Other Contexts

## Responsibilities Context

Responsibilities may be assigned to members defined in Family.

Responsibilities cannot create, modify, or remove members.

Integration rule:

* Responsibilities references `MemberId`
* Family owns membership validity

---

## Calendar Context

Events may include participants that are:

* members
* dependents
* pets

Calendar does not own these entities.

Integration rule:

* Calendar references participant IDs
* Family owns participant identity and relationships

---

## Tasks Context

Tasks may be assigned to members defined in Family.

Tasks cannot change member structure.

Example:

```
Parent assigned task: prepare school bag
Child participates in school event
```

Assignment and participation rules depend on relationships defined by Family.

---

## Identity Ownership Rule

Other contexts may reference Family entities as:

* **participants** (Calendar)
* **assignees** (Tasks)
* **responsibility owners** (Responsibilities)

However:

**Family alone owns membership and relationship semantics.**

This ensures consistent rules for cases such as:

```
Child participates in event
Parent receives preparation task
```

Dependency structures remain authoritative in the Family context.

---

# Read Models

Useful read models for this context.

## Family Summary

Contains:

* family ID
* family name
* total members
* total dependents
* total pets

---

## Family Roster

Contains:

* all active members
* all dependents
* all pets
* roles
* statuses

---

## Family Relationship Graph

Contains:

* members
* relationship edges
* relationship types

This read model is useful for UI and potential AI-assisted reasoning.

---

# Ubiquitous Language Notes

Within this context:

* `Family` means the household unit
* `Member` means an independent household person
* `Dependent` means an entity requiring care from members
* `Pet` means an animal dependent belonging to the household
* `Relationship` means a structural dependency or kinship link

Avoid ambiguous synonyms such as:

* user
* account member
* profile
* contact
* participant

unless they belong to another explicit context.

---

# Slice Mapping

Initial slices mapped to this context:

* `create-family`
* `add-member`
* `remove-member`
* `add-dependent`
* `add-pet`
* `assign-relationship`

These slices operate only on the `Family` aggregate.

---

# Transaction Rules

Rules:

* one command modifies one `Family` aggregate
* all structural changes occur inside the `Family` transaction boundary
* downstream reactions occur through domain events
* no cross-aggregate write inside the same command

Example:

```
AddMember
→ updates Family
→ emits MemberAdded
```

Other modules may react after commit.

---

# Design Notes

The Family context is intentionally small and strict.

It models **identity and household structure**, not operational behavior.

It must not absorb logic that belongs to:

* scheduling
* task execution
* responsibility assignment
* reminders
* food planning
* administration

If the question is **“who belongs to this household?”**, the answer belongs here.

If the question is **“what must happen for this person?”**, the answer belongs elsewhere.

---

# Summary

The Family context defines the **identity backbone of DomusMind**.

It owns:

* household identity
* members
* dependents
* pets
* relationships

Other contexts may reference these entities as participants or assignees, but **only Family defines and maintains their relationships and dependency semantics**.
