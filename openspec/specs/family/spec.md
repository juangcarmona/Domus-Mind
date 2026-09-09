# Family Specification

## Purpose

The Family context defines the household unit and its internal human structure.

A **Family** is the primary identity boundary of DomusMind. It is the root organizational unit. All other contexts depend on Family for participant identity but may not modify its structure.

A **Member** is a person belonging to a household. Members are independent participants who may be assigned tasks, hold responsibilities, and participate in plans. A member has a `MemberRole`: `Adult`, `Child`, `Caregiver`, or `Pet`. A member may additionally be designated as a **manager** — a boolean flag that grants authority to perform administrative actions (editing other members, provisioning access, etc.). How a member is first designated as manager is not specified in available source documents (see NOTE N4).

A **Pet** is a non-human entity belonging to the household. In Phase 1, pets are represented as household members with `MemberRole = Pet`. The domain design describes Pet as a distinct entity (with its own identity separate from Member); Phase 1 uses `FamilyMember` with `MemberRole = Pet` instead. The `Pet` role carries specific operational constraints: pets cannot be provisioned with system access, cannot receive task assignments, cannot hold responsibility ownership, and are excluded from household coordination views (Agenda rows, member timelines) in V1. Pets may be added as event participants (e.g. a vet appointment).

A **Relationship** is a structural link between two family members. Relationships express household dependency and care structures (parent→child, spouse↔spouse, sibling↔sibling, caregiver→dependent). Relationship management is a V1.1 capability (see NOTE N5).

The Family context is the upstream identity provider. It emits more than it consumes. No downstream context may create, modify, or remove family structure.

---

## Requirements

### Requirement: Household Creation

A household SHALL be created with a name as the only required input.

On creation, the household starts with empty member, pet, and relationship sets. The household receives a stable, unique identifier that never changes.

#### Scenario: Household is created

- GIVEN a valid name is provided
- WHEN a household is created
- THEN a Family is established with a stable FamilyId
- AND the household has an empty member set, pet set, and relationship set
- AND a `FamilyCreated` event is emitted

#### Scenario: Household creation fails with an empty name

- WHEN a household creation is attempted with an empty or missing name
- THEN the household is not created
- AND a validation error is returned

---

### Requirement: Member Addition

A household SHALL be able to add a new member with a name and a role.

Valid roles are: `Adult`, `Child`, `Caregiver`, `Pet`. A member receives a unique identifier within the household. Once added, the member becomes part of the household roster and may be referenced by other contexts through their MemberId.

Optional inputs: birth date, notes.

#### Scenario: A member is added to the household

- GIVEN a household exists
- WHEN a member is added with a valid name and role
- THEN the member is added to the household with a unique MemberId
- AND a `MemberAdded` event is emitted

#### Scenario: Member addition fails with a duplicate MemberId

- GIVEN a household exists with an existing member
- WHEN a new member is submitted with the same MemberId
- THEN the member is not added
- AND a validation error is returned

#### Scenario: Member addition fails with an invalid role

- WHEN a member is submitted with a role not in the allowed set
- THEN the member is not added
- AND a validation error is returned

#### Scenario: Member addition fails if the household does not exist

- WHEN a member is submitted for a non-existent FamilyId
- THEN the operation is rejected

---

### Requirement: Member Removal _(V1.1)_

> **Scope note:** This capability is explicitly deferred to V1.1 in `specs/system/system-spec.md`. Member removal requires validating open task assignments and participant references. It is modeled in the domain but not exposed via API in V1.

A household SHALL be able to remove an existing member.

A removed member cannot participate in new relationships. Removing a member does not cascade into other contexts — those contexts reference by ID and must handle identity resolution independently.

#### Scenario: A member is removed from the household

- GIVEN a household exists with at least one member
- WHEN that member is removed
- THEN the member is no longer part of the household roster
- AND a `MemberRemoved` event is emitted

---

### Requirement: Member Core Details Update

A household manager SHALL be able to update a member's full name, role, and optional birth date.

Birth date, when provided, must be in the past. Only managers may perform this action. A member may edit their own profile details through a separate profile update path.

#### Scenario: Manager updates a member's name and role

- GIVEN a household exists with a member
- AND the requesting user is a manager
- WHEN the member's name, role, or birth date is updated with valid values
- THEN the member record reflects the new values

#### Scenario: Non-manager cannot update another member's core details

- GIVEN a household exists with a member
- AND the requesting user is not a manager and is not the target member
- WHEN a core detail update is attempted
- THEN the update is rejected

#### Scenario: Birth date in the future is rejected

- WHEN a member update is submitted with a future birth date
- THEN the update is rejected
- AND a validation error is returned

---

### Requirement: Member Profile Update

A member SHALL be able to update their own optional contact details: preferred name, phone, email, and household note.

Managers may also update any member's profile details.

#### Scenario: Member updates their own profile

- GIVEN a household member is authenticated
- WHEN that member updates their own preferred name, phone, email, or household note
- THEN the profile is updated

---

### Requirement: Member Access Provisioning

A household manager SHALL be able to provision, disable, enable, and reset system access for members.

Access provisioning links a member's household identity to an authentication account. Pets cannot be provisioned with access.

The access state of a member is one of: `NoAccess`, `InvitedOrProvisioned`, `PasswordResetRequired`, `Active`, `Disabled`.

| State | Meaning |
|---|---|
| `NoAccess` | No authentication account linked |
| `InvitedOrProvisioned` | Account exists, password change required, never logged in |
| `PasswordResetRequired` | Account exists, password change required, has logged in before |
| `Active` | Account exists, not disabled, no forced password change |
| `Disabled` | Account exists but is disabled |

Rules:
- Only a manager may provision, disable, enable, or regenerate access for other members
- A manager may not disable their own access
- Pets cannot be provisioned with access

#### Scenario: Manager provisions access for a member

- GIVEN a member exists with `NoAccess` state
- AND the requesting user is a manager
- AND the member role is not Pet
- WHEN access is provisioned
- THEN the member is linked to an authentication account
- AND the member state becomes `InvitedOrProvisioned`

#### Scenario: Manager disables a member's access

- GIVEN a member exists with `Active` state
- AND the requesting user is a manager
- AND the target is not the requesting manager themselves
- WHEN access is disabled
- THEN the member state becomes `Disabled`

#### Scenario: Manager enables a member's access

- GIVEN a member exists with `Disabled` state
- AND the requesting user is a manager
- WHEN access is enabled
- THEN the member state becomes `Active`

#### Scenario: Provisioning a Pet is rejected

- GIVEN a member with role `Pet`
- WHEN access provisioning is attempted
- THEN the operation is rejected

---

### Requirement: Pet Registration

A household SHALL be able to register a pet by adding a member with `MemberRole = Pet`.

In Phase 1, pets are stored as `FamilyMember` entries with the `Pet` role — not as a distinct Pet entity. The domain design describes Pet as a separate entity; Phase 1 collapses this into the member model (see NOTE N6).

Pets in V1:
- cannot be provisioned with system access
- cannot receive task assignments
- cannot hold responsibility ownership
- are excluded from Agenda household rows and member coordination timelines
- may be added as event participants (e.g. vet appointments)
- appear in the member directory as a distinct group (after Adults, Caregivers, and Children)

#### Scenario: A pet is added to the household

- GIVEN a household exists
- WHEN a member is added with a valid name and `MemberRole = Pet`
- THEN the pet is part of the household with a unique MemberId
- AND a `MemberAdded` event is emitted

#### Scenario: A pet is removed from the household

- GIVEN a household exists with a registered pet
- WHEN the pet member is removed
- THEN the pet is no longer part of the household
- AND a `MemberRemoved` event is emitted

---

### Requirement: Relationship Assignment _(V1.1)_

> **Scope note:** This capability is explicitly deferred to V1.1 in `specs/system/system-spec.md`. Relationship semantics are modeled in the domain but not exposed via API or UI in V1.

A household SHALL be able to define a structural relationship between two of its members.

Both parties must be existing members of the same household. Duplicate relationships of the same type between the same entities are not allowed. Valid relationship types include: parent→child, spouse↔spouse, sibling↔sibling, caregiver→dependent.

#### Scenario: A relationship is assigned between two members

- GIVEN a household exists with two distinct members
- WHEN a relationship of a valid type is assigned between them
- THEN the relationship is recorded in the household structure
- AND a `RelationshipAssigned` event is emitted

#### Scenario: Relationship assignment fails if either member does not exist

- WHEN a relationship references a MemberId not in the household
- THEN the assignment is rejected

#### Scenario: Duplicate relationship is rejected

- GIVEN a relationship of a given type already exists between two members
- WHEN the same relationship type is submitted for the same pair
- THEN the assignment is rejected

#### Scenario: Relationship assignment fails if both parties are the same member

- WHEN a relationship is submitted with the same member ID for both parties
- THEN the assignment is rejected

#### Scenario: A relationship is removed

- GIVEN a relationship exists between two members
- WHEN the relationship is removed
- THEN it is no longer part of the household structure
- AND a `RelationshipRemoved` event is emitted

---

### Requirement: Household Member Directory

A household member SHALL be able to view the member directory of their household.

The directory is ordered: Adults and Caregivers first, then Children, then Pets. Within each group, managers appear before other members; within that ordering, entries are alphabetical by name.

The directory includes server-computed access flags. Clients must not re-derive permission or access state from raw fields.

---

### Requirement: Identity Boundary Enforcement

The Family context SHALL be the sole owner of household structure.

No other context may create, modify, or remove members, pets, or relationships. Other contexts reference family entities by ID only. Cross-context reactions happen through domain events.

---

## Domain Events

| Event | Emitted when |
|---|---|
| `FamilyCreated` | A new household is created |
| `MemberAdded` | A member is added to the household |
| `MemberRemoved` | A member is removed from the household |
| `MemberAdded` (role=Pet) | A pet is registered in Phase 1; the domain design describes a distinct `PetAdded` event — see NOTE N6 |
| `MemberRemoved` (role=Pet) | A pet is removed in Phase 1; domain design describes a distinct `PetRemoved` event — see NOTE N6 |
| `RelationshipAssigned` | A relationship is recorded (V1.1) |
| `RelationshipRemoved` | A relationship is removed (V1.1) |

---

## Invariants

- A Family must have a stable, unique FamilyId
- A member belongs to exactly one family
- Member IDs must be unique within the family
- In Phase 1, pets are members with `MemberRole = Pet`; pet identity uses MemberId and is covered by the member ID uniqueness invariant
- Relationships must reference existing members of the same family
- A relationship cannot reference the same member on both sides
- Duplicate relationships of the same type between the same entities are not allowed
- A removed member cannot participate in new relationships

---

## Notes

**N1 — Dependent entity status**: `docs/04_contexts/family.md` describes `Dependent` as a separate internal entity. However, `specs/system/system-spec.md` lists only `create-family` and `add-member` in the V1 Family feature set — there is no `add-dependent` in V1. `specs/features/family/member-management.md` confirms Phase 1 uses `FamilyMember` as the single member entity with `Child` and `Caregiver` as roles; the `Dependent` entity was designed but never shipped as a V1 capability. The associated commands (`AddDependent`, `RemoveDependent`) and events (`DependentAdded`, `DependentRemoved`) belong to the domain design and a future roadmap phase. This spec reflects the Phase 1 implementation state: children and care recipients are `Member` entities with `MemberRole = Child` or `Caregiver`.

**N2 — `UpdateFamilySettings` command**: Listed in `docs/04_contexts/family.md` but not detailed in any spec or feature document. Behavior is undefined. Excluded from this spec.

**N3 — Manager self-removal and last-manager constraint**: No source document specifies whether a manager can remove themselves, whether a household must always have at least one manager, or what happens when the last manager is removed. Excluded pending clarification.

**N4 — Manager designation**: The `IsManager` boolean flag determines which members may perform administrative actions (edit other members, provision/disable access, etc.). Available source documents do not specify how a member is first designated as manager at household creation, or whether manager status can be granted or revoked through a distinct capability. Excluded pending clarification.

**N5 — Relationship assignment deferral**: `specs/system/system-spec.md` explicitly defers `assign-relationship` and `remove-member` to V1.1, citing cascading complexity. The relationship model (Relationship entity, RelationshipType, RelationshipAssigned/Removed events) is fully modeled in the domain but not exposed via V1 API or UI.

**N6 — Pet entity vs. Pet role**: `docs/04_contexts/family.md` describes Pet as a separate aggregate with its own `PetId`, `PetAdded`, and `PetRemoved` events. `specs/features/family/member-management.md` Phase 1 baseline documents `MemberRole` with values `Adult`, `Child`, `Caregiver`, `Pet` — pets are stored as `FamilyMember` and their identity uses `MemberId`. Phase 1 emits `MemberAdded`/`MemberRemoved` for pet operations. The separate Pet entity model and its associated events (`PetAdded`, `PetRemoved`) represent a domain design goal deferred beyond Phase 1.

**N7 — Auth identity vs. domain identity**: The authentication identity of a user and the household `MemberId` are deliberately separate concerns. Auth identity is linked to a member via `AuthUserId` but the Family context does not own authentication. This spec describes the household identity side only.

**N8 — Onboarding flow**: How a household is created during initial user onboarding (e.g. whether it is triggered automatically or by explicit user action) is not described in the primary source documents. Excluded pending clarification.

---

## Sources

- `docs/04_contexts/family.md`
- `docs/03_domain/ubiquitous-language.md`
- `docs/03_domain/context-map.md`
- `specs/features/family/create-family.md`
- `specs/features/family/add-member.md`
- `specs/features/family/assign-relationship.md`
- `specs/features/family/member-management.md`
- `00_product/surfaces/settings.md`
