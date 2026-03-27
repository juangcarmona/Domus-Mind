# Design Spec - Member Management

> **Status**: Phase 1 - Implemented  
> **Context**: Family  
> **Governs**: Member classification, lifecycle, roles, identity enrichment, and read models

---

## Phase 1 - Scope and status

Phase 1 establishes the minimal foundation for household member management:

- stable member directory with correct ordering and server-computed UI flags
- 5-state account access model with first-login distinguishment
- admin access actions (provision, disable, enable, regenerate password)
- server-computed authorization projection so the client never re-derives permission rules

What Phase 1 explicitly defers (documented in §8 for future expansion):

- contact methods, addresses, emergency info
- documents metadata (passports, insurance, medical)
- full member profile page
- MemberKind/ResidencyType redesign (Guest, ExternalCollaborator, etc.)
- membership lifecycle states (Planned → Active → Inactive → Archived)
- automated expiry
- extended relationship types

---

## Baseline Audit - What Exists Today

Before introducing anything new, this is the exact current state.

### Family Aggregate - Current Entities (Phase 1 baseline)

`FamilyMember` is the single member entity. `Role` is `MemberRole` value object with values `Adult`, `Child`, `Caregiver`, `Pet`.

Fields: `MemberId`, `FamilyId`, `Name`, `Role`, `IsManager`, `BirthDate?`, `JoinedAtUtc`, `AuthUserId?`

---

## Phase 1 - Implementation

### Slice set (Phase 1)

| Slice | Direction | Handler |
|-------|-----------|---------|
| `view-member-directory` | Query | `GetFamilyMembersQuery` → `IReadOnlyCollection<MemberDirectoryItemResponse>` |
| `view-member-details` | Query | `GetMemberDetailsQuery` → `MemberDetailResponse` |
| `update-member-core-details` | Command | `UpdateMemberCommand` (existing) |
| `grant-member-access` | Command | `ProvisionMemberAccessCommand` (existing) |
| `disable-member-access` | Command | `DisableMemberAccessCommand` (existing) |
| `enable-member-access` | Command | `EnableMemberAccessCommand` (new in Phase 1) |
| `regenerate-member-password` | Command | `RegenerateTemporaryPasswordCommand` (existing) |

### API endpoints

```
GET  /api/families/{familyId}/members               → MemberDirectoryItemResponse[]
GET  /api/families/{familyId}/members/{memberId}    → MemberDetailResponse
POST /api/families/{familyId}/members/{memberId}/enable-access  → EnableMemberAccessResponse
```

### MemberAccessStatus (5-state model)

Computed by the backend on each request. The disambiguation between `InvitedOrProvisioned` and `PasswordResetRequired` uses `AuthUser.LastLoginAtUtc`:

| Status | Condition |
|--------|-----------|
| `NoAccess` | No `AuthUserId` linked |
| `InvitedOrProvisioned` | Has account, `MustChangePassword = true`, `LastLoginAtUtc = null` |
| `PasswordResetRequired` | Has account, `MustChangePassword = true`, `LastLoginAtUtc` is set |
| `Active` | Has account, not disabled, `MustChangePassword = false` |
| `Disabled` | Has account, `IsDisabled = true` |

`LastLoginAtUtc` is set by `LoginCommandHandler` on each successful login.

### MemberDirectoryItemResponse - server-computed projection

```
MemberId, FamilyId, Name, Role, IsManager, BirthDate?, JoinedAtUtc,
AuthUserId?, AccessStatus, LinkedEmail?,
IsCurrentUser   ← true when AuthUserId == requestedByUserId
HasAccount      ← AuthUserId != null
CanGrantAccess  ← isRequestingManager && !hasAccount && role != Pet
CanEdit         ← isRequestingManager || isCurrentUser
AvatarInitial   ← Name[0].ToUpperInvariant()
```

The client must not re-derive these values from raw data. They are authoritative.

### Sort order (directory)

Adults and Caregivers first (group 0) → Children (group 1) → Pets (group 2).
Within each group: managers first, then alphabetical by name.

### Permission rules

| Action | Required |
|--------|----------|
| View directory | Family access |
| Edit own profile | self |
| Edit any member | manager |
| Provision access | manager, role ≠ Pet |
| Disable access | manager, not self |
| Enable access | manager |
| Regenerate password | manager, not self |

---

## Phase 1 infrastructure changes

- `AuthUser.LastLoginAtUtc` (`DateTime?`) - set on every successful login
- Migration: `AddLastLoginAtUtcToAuthUser`
- `IAuthUserRepository` extended: `EnableUserAsync`, `UpdateLastLoginAtAsync`
- `AuthUserStatusProjection` extended: `DateTime? LastLoginAtUtc`

---

## §8 - Extension seam for future rich member data

Future phases (V2+) should add the following through a dedicated member-profile slice, NOT by extending `MemberDirectoryItemResponse`:

- **Contact methods**: email addresses, phone numbers, messaging handles
- **Addresses**: home, work, other
- **Emergency information**: emergency contact name/phone, relationship
- **Notes**: free-text household notes (non-sensitive)
- **Documents metadata**: document type, issuing authority, expiry date - no file content

This seam lives in a future `MemberProfileResponse` returned by a `view-member-profile` query. The directory and detail responses (`MemberDirectoryItemResponse`, `MemberDetailResponse`) deliberately do not grow with these fields.

---

> The sections below are roadmap proposals for V1.1 and later. They have not yet been implemented.
> Phase 1 implementation is fully described above.

---

## Future roadmap - Domain Model Proposals (V1.1+)

### Decision: MemberKind extended classification (V1.1)

Phase 1 keeps `MemberRole` with values `Adult`, `Child`, `Caregiver`, `Pet`. Future phases will introduce:

- `Guest` - temporary presence with defined period
- `ExternalCollaborator` - non-resident helper (nanny, babysitter, au pair)
- `ServiceProvider` - external service (cleaning, maintenance)
- `ExtendedFamily` - non-resident family-related person

Migration of existing values: `Adult → Adult`, `Child → Child`, `Caregiver → Caregiver` (no breaking change).

### 2. Domain Model Proposal

### Decision: Retire `Dependent` as a separate entity

**Current state**: `Member` and `Dependent` are separate entities with separate IDs.

**Decision**: The `Dependent` entity is retired. Children, elderly dependents, and other care recipients are represented as `Member` entities with `MemberKind = Child` or `MemberKind = Caregiver`. The dependency relationship between members (parent→child, caregiver→elderly) is already modeled through the `Relationship` entity and that remains unchanged.

**Justification**: The only behavioral difference between `Member` and `Dependent` in the current model is the note that "dependents do not receive task assignments." This constraint belongs in capability rules (enforced by `MemberKind`) - not in separate entity identity. Maintaining two entity types creates double-representation, complicates queries, and prevents uniform treatment. No `add-dependent` slice was shipped, so there is no migration surface to break.

**Retired events**: `DependentAdded` and `DependentRemoved` are removed from the domain. `MemberAdded` with `kind = Child` replaces them.

### Decision: Keep `Pet` as a separate entity

Pets are not people. They cannot hold accounts, cannot be assigned tasks, and cannot participate in coordination surfaces in the operational sense. Keeping `Pet` separate preserves this distinction without cluttering `Member` with non-human concerns. This decision is unchanged from the current model.

---

### Updated Value Objects

#### MemberKind

Replaces the current `MemberRole`. Absorbs the former `Dependent` cases.

| Kind | Meaning | Default Residency |
|------|---------|-------------------|
| `Adult` | Resident adult | Resident |
| `Child` | Resident child (replaces Dependent) | Resident |
| `Caregiver` | Live-in caregiver | Resident |
| `Guest` | Temporary presence with defined period | Temporary |
| `ExternalCollaborator` | Non-resident helper (nanny, babysitter, au pair) | NonResident |
| `ServiceProvider` | External service (cleaning, maintenance) | NonResident |
| `ExtendedFamily` | Non-resident family-related person | NonResident |

Migration path for existing `MemberRole` values:

- `Adult` → `MemberKind.Adult`
- `Child` → `MemberKind.Child`
- `Caregiver` → `MemberKind.Caregiver`

No breaking change to existing data.

---

#### ResidencyType

A separate dimension from MemberKind. Captures where the person lives relative to the household.

| Value | Meaning |
|-------|---------|
| `Resident` | Lives in the household |
| `Temporary` | Staying for a defined, bounded period |
| `NonResident` | External to the household but linked |

Default mapping from MemberKind (can be overridden):

| MemberKind | Default ResidencyType |
|---|---|
| Adult | Resident |
| Child | Resident |
| Caregiver | Resident |
| Guest | Temporary |
| ExternalCollaborator | NonResident |
| ServiceProvider | NonResident |
| ExtendedFamily | NonResident |

An au pair with a MembershipPeriod and ResidencyType=Temporary is a valid combination of `ExternalCollaborator` + `Temporary`.

---

#### MemberStatus

Represents the lifecycle state of a member.

| Status | Meaning |
|--------|---------|
| `Planned` | Member is expected to arrive; not yet active |
| `Active` | Currently active in the household |
| `Inactive` | No longer active (left, role ended, period expired) |
| `Archived` | Historical record only; excluded from all operational surfaces |

**Allowed transitions**:

```
Planned  → Active     (member activates / arrives)
Planned  → Archived   (cancelled before arrival)
Active   → Inactive   (member leaves)
Active   → Archived   (immediate archive for data cleanup)
Inactive → Archived   (permanent closure of record)
```

No reactivation from `Inactive` in V1.1. If a person returns, a new member record is created and linked via relationship to the historical record if needed. This is the simplest consistent model.

---

#### HouseholdRole

Authorization capacity within the household. This is the operational permission level, separate from MemberKind.

| Role | Meaning |
|------|---------|
| `Manager` | Full household management: all members, all contexts |
| `Participant` | Full coordination participation: tasks, calendar, responsibilities |
| `Observer` | Read-only access to household surfaces |
| `Collaborator` | Scoped participation: limited to assigned task/area scope |
| `Service` | Narrowest scope: specific task categories as defined by manager |

**Constraints on HouseholdRole by MemberKind**:

| MemberKind | Allowed HouseholdRoles | Default |
|---|---|---|
| Adult | Manager, Participant, Observer | Participant |
| Child | Participant, Observer | Observer |
| Caregiver | Participant, Observer | Participant |
| Guest | Observer | Observer |
| ExternalCollaborator | Collaborator, Observer | Collaborator |
| ServiceProvider | Service | Service |
| ExtendedFamily | Observer, Collaborator | Observer |

These are domain invariants enforced by the Family aggregate.

---

#### MembershipPeriod (new value object)

Temporal bounds for a member's active presence.

```
StartDate : DateOnly (nullable)
EndDate   : DateOnly (nullable)
```

Invariants:

- If both are set: `EndDate > StartDate`
- Guest members **must** have a non-null `EndDate`
- ServiceProvider members with a recurring contract may set `EndDate` to contract end
- Resident Adult and Child members have null MembershipPeriod (unbounded)

This value object does not trigger automatic status changes in V1.1. Expiry enforcement is manual (operator/manager deactivates the member). Automated enforcement is V2.

---

#### DisplayIdentity (new value object)

Replaces the current flat `name` string.

```
LegalName     : string  (optional - required only if member has a linked account)
PreferredName : string  (required - used in all UI surfaces)
Nickname      : string  (optional)
AvatarRef     : string  (optional - URI or storage reference)
```

Invariant: `PreferredName` must always be non-empty.

---

### Updated Member Entity

```
Member {
    MemberId           : MemberId          (stable, unique within family)
    FamilyId           : FamilyId          (parent reference)
    Kind               : MemberKind        (classification)
    ResidencyType      : ResidencyType     (where they live relative to household)
    Status             : MemberStatus      (lifecycle state, default: Active)
    HouseholdRole      : HouseholdRole     (authorization capacity)
    Identity           : DisplayIdentity   (how this person is identified)
    MembershipPeriod   : MembershipPeriod? (null = unbounded)
    BirthDate          : DateOnly?
    LinkedUserId       : UserId?           (link to auth identity; null = no account)
    Notes              : string?
}
```

---

### Extended Relationship Types

Current: `Parent`, `Child`, `Spouse`, `Partner`, `Sibling`, `Caregiver`, `Dependent`

Extended (add the following):

| Type | Direction | Notes |
|------|-----------|-------|
| `Grandparent` | Grandparent → Grandchild | Bidirectional by convention |
| `Grandchild` | (inverse of Grandparent) | |
| `ExtendedFamily` | Generic for uncle, aunt, cousin | Non-directional |
| `ServiceRelation` | ServiceProvider ↔ Household member | Weak link, informational |

The `Relationship` entity already references `MemberId` on both ends, so these new types extend the `RelationshipType` value object with no structural change.

---

### Invariants (complete set for updated model)

**Membership**:
- A member belongs to exactly one family
- MemberIds are unique within a family
- A member in `Archived` status may not be modified
- A member in `Inactive` status may not receive new task assignments or be added as a calendar participant

**MemberKind + HouseholdRole**:
- ServiceProvider may only hold HouseholdRole = Service
- Guest may only hold HouseholdRole = Observer
- ExternalCollaborator may hold HouseholdRole = Collaborator or Observer
- Adult and Caregiver are the only kinds eligible for HouseholdRole = Manager
- Child may not hold HouseholdRole = Manager or Collaborator

**MembershipPeriod**:
- Guest members must have a non-null EndDate
- EndDate must be after StartDate when both are set

**Identity**:
- PreferredName must be non-empty for all members
- LegalName is required if LinkedUserId is set

**Account linking**:
- A Child member may not have a LinkedUserId in V1.1
- ServiceProvider and Guest members may have a LinkedUserId
- Only one member within the same family may link to the same UserId
- Only a Manager-role member may invoke link-member-to-user

---

### New Domain Events

| Event | Trigger | Payload |
|-------|---------|---------|
| `MemberKindChanged` | Kind reclassification | memberId, previousKind, newKind |
| `MemberStatusChanged` | Lifecycle transition | memberId, previousStatus, newStatus |
| `MembershipPeriodSet` | Temporal bounds set | memberId, startDate, endDate |
| `HouseholdRoleAssigned` | Role changed | memberId, previousRole, newRole |
| `MemberLinkedToUser` | Auth identity linked | memberId, userId |
| `MemberUnlinkedFromUser` | Auth identity removed | memberId, userId |
| `MemberProfileUpdated` | Identity fields changed | memberId, changed fields |

**Retired events**:
- `DependentAdded` → replaced by `MemberAdded` with kind = Child
- `DependentRemoved` → replaced by `MemberRemoved`

---

## 3. Slice Design

Slices are grouped by capability. Each slice maps to one command or query and touches one aggregate boundary.

### Group A - Member Registration (extends V1)

| Slice | Type | Phase | Description |
|-------|------|-------|-------------|
| `add-member` | Command | V1 → extend | Extend to accept `kind`, `residencyType`, `householdRole`, `membershipPeriod?`. Backward-compatible extension. |
| `register-temporary-member` | Command | V1.1 | Adds a member with explicit `MembershipPeriod`. Validates Guest/ServiceProvider rules. Emits `MemberAdded` + `MembershipPeriodSet`. |
| `add-initial-members` | Command | V1 → extend | Extend to accept `kind` and `householdRole` per member. |

### Group B - Member Lifecycle

| Slice | Type | Phase | Description |
|-------|------|-------|-------------|
| `activate-member` | Command | V1.1 | Transitions `Planned → Active`. Emits `MemberStatusChanged`. |
| `deactivate-member` | Command | V1.1 | Transitions `Active → Inactive`. Emits `MemberStatusChanged`. Final lifecycle marker. |
| `archive-member` | Command | V1.1 | Transitions `Inactive → Archived`. Hard close. Emits `MemberStatusChanged`. |
| `remove-member` | Command | V1.1 | Already planned; transitions Active → Inactive then Archived in a single command. |
| `set-membership-period` | Command | V1.1 | Sets or updates `MembershipPeriod` on existing member. Emits `MembershipPeriodSet`. |

### Group C - Member Identity and Profile

| Slice | Type | Phase | Description |
|-------|------|-------|-------------|
| `update-member-profile` | Command | V1.1 | Updates `DisplayIdentity` fields (PreferredName, LegalName, Nickname, AvatarRef), BirthDate, Notes. Emits `MemberProfileUpdated`. |

### Group D - Household Role Management

| Slice | Type | Phase | Description |
|-------|------|-------|-------------|
| `assign-household-role` | Command | V1.1 | Changes `HouseholdRole` on a member. Enforces kind/role compatibility invariants. Emits `HouseholdRoleAssigned`. Requires calling member to hold Manager role. |
| `link-member-to-user` | Command | V1.1 | Associates a `MemberId` with a `UserId`. Enforces: no duplicate links, no Child links, Manager-only operation. Emits `MemberLinkedToUser`. |
| `unlink-member-from-user` | Command | V1.1 | Removes the `UserId` association. Emits `MemberUnlinkedFromUser`. |

### Group E - Relationship Management (existing slice extended)

| Slice | Type | Phase | Description |
|-------|------|-------|-------------|
| `assign-relationship` | Command | V1.1 | Already planned. Extend `RelationshipType` enum to include Grandparent, ExtendedFamily, ServiceRelation. |
| `remove-relationship` | Command | V1.1 | Already planned. No change needed. |

### Group F - Contact Information (V2)

| Slice | Type | Phase | Description |
|-------|------|-------|-------------|
| `add-member-contact` | Command | V2 | Adds a `ContactInfo` entry (email, phone, messaging handle) to a member. |
| `remove-member-contact` | Command | V2 | Removes a specific contact entry. |
| `set-primary-contact` | Command | V2 | Marks a contact entry as primary for its type. |

### Group G - Emergency Contacts (V2)

| Slice | Type | Phase | Description |
|-------|------|-------|-------------|
| `add-emergency-contact` | Command | V2 | Registers an emergency contact for a member. |
| `remove-emergency-contact` | Command | V2 | Removes an emergency contact. |

### Group H - Queries and Read Models

| Slice | Type | Phase | Description |
|-------|------|-------|-------------|
| `view-member-directory` | Query | V1.1 | Returns all members grouped by kind. Filtered by status (excludes Archived by default). |
| `view-member-profile` | Query | V1.1 | Returns full member detail including relationships and MembershipPeriod. |
| `view-active-members` | Query | V1.1 | Returns only Active members. Used by Calendar and Tasks contexts. |
| `view-assignable-members` | Query | V1.1 | Returns members eligible for task assignment. Respects kind and status constraints. |

---

## 4. Read Models

### MemberDirectoryView

Purpose: household-facing member directory, grouped for display.

```
MemberDirectoryView {
    familyId
    groups: [
        {
            kind: MemberKind
            members: [MemberSummary]
        }
    ]
    pets: [PetSummary]
    generatedAt: timestamp
}

MemberSummary {
    memberId
    preferredName
    nickname?
    kind: MemberKind
    residencyType: ResidencyType
    status: MemberStatus
    householdRole: HouseholdRole
    avatarRef?
    hasAccount: bool         -- derived from LinkedUserId != null
    membershipPeriod?        -- shown for Guest and ServiceProvider
}
```

Groups are ordered: Resident Adults → Resident Children → Caregivers → Guests → External Collaborators → Service Providers → Extended Family.

Archived members are excluded.

---

### MemberProfileView

Purpose: detailed view of a single member.

```
MemberProfileView {
    memberId
    familyId
    kind: MemberKind
    residencyType: ResidencyType
    status: MemberStatus
    householdRole: HouseholdRole
    identity: {
        preferredName
        legalName?         -- visible only to Manager-role callers
        nickname?
        avatarRef?
    }
    birthDate?
    membershipPeriod?
    relationships: [
        {
            relatedMemberId
            relatedMemberPreferredName
            relationshipType
        }
    ]
    hasAccount: bool
    notes?                 -- visible only to Manager-role callers
}
```

`legalName` and `notes` are sensitive. The query handler must inspect the caller's HouseholdRole before including them. This is enforced at the application layer in the query handler, not in the domain.

---

### AssignableMembersView

Purpose: compact list consumed by Tasks context when presenting assignment options.

```
AssignableMembersView {
    familyId
    members: [
        {
            memberId
            preferredName
            kind: MemberKind
            householdRole: HouseholdRole
        }
    ]
}
```

Inclusion rules:
- Status must be `Active`
- Kind must not prevent assignment: `Participant`, `Collaborator`, or `Service` household role
- Caller context (Tasks) must respect ServiceProvider-only-gets-service-tasks constraint independently

---

## 5. Lifecycle Rules

### Member Status State Machine

```
                 ┌─────────────────────────────────┐
                 │                                 │
             Planned ──────────────────────────► Archived
                 │
                 ▼
             Active ──────────────────────────► Archived
                 │
                 ▼
             Inactive ────────────────────────► Archived
```

**activate-member**: `Planned → Active`
- Precondition: member exists, status is `Planned`
- Effect: sets status to `Active`, stamps `ActivatedAtUtc`
- Event: `MemberStatusChanged`

**deactivate-member**: `Active → Inactive`
- Precondition: member exists, status is `Active`
- Effect: sets status to `Inactive`, stamps `DeactivatedAtUtc`
- Event: `MemberStatusChanged`
- Side effect: downstream contexts receive `MemberStatusChanged` and may remove member from pending assignments; they do not act synchronously

**archive-member**: `Inactive → Archived` or `Active → Archived`
- Precondition: member exists, status is `Inactive` or `Active`
- Effect: sets status to `Archived`
- Event: `MemberStatusChanged`
- Hard constraint: once `Archived`, no further modifications are permitted

**remove-member** (existing V1.1 slice):
- This slice performs `Active → Inactive → Archived` in a single idempotent command
- It is the standard path for removing a current member
- It does NOT delete the record

---

### Temporal Membership Lifecycle

The `MembershipPeriod` is set when registering a temporary member or via `set-membership-period`. It does not automatically change status in V1.1.

In V1.1, a manager reviews members with past `EndDate` and manually invokes `deactivate-member`.

A future system query (V2) `view-expired-memberships` will surface members whose `EndDate` has passed and status remains `Active`, allowing batch deactivation.

---

### Member Registration Defaults by Kind

| MemberKind | Default Status | Default HouseholdRole | MembershipPeriod required? |
|---|---|---|---|
| Adult | Active | Participant | No (unbounded) |
| Child | Active | Observer | No (unbounded) |
| Caregiver | Active | Participant | No (may be set) |
| Guest | Active | Observer | Yes (EndDate required) |
| ExternalCollaborator | Active | Collaborator | Optional |
| ServiceProvider | Active | Service | Optional (contract end) |
| ExtendedFamily | Active | Observer | Optional |

A member can be created with `Planned` status by explicitly passing `status = Planned` and a `MembershipPeriod.StartDate` in the future.

---

## 6. Permission Model

### Design decision: HouseholdRole is the authorization axis

The authorization model has two distinct parts:

1. **HouseholdRole on Member** - domain concept; defines what this member is allowed to do operationally
2. **Enforcement at application layer** - handlers check the calling user's linked member's `HouseholdRole`

These are separate concerns. The domain does not know about HTTP requests or JWT tokens. It owns the `HouseholdRole` value. The application layer enforces it.

---

### Household Role Permissions Matrix

| Capability | Manager | Participant | Observer | Collaborator | Service |
|---|---|---|---|---|---|
| View member directory | ✓ | ✓ | ✓ | ✗ | ✗ |
| View member profiles (public fields) | ✓ | ✓ | ✓ | ✗ | ✗ |
| View member profiles (sensitive fields) | ✓ | ✗ | ✗ | ✗ | ✗ |
| Add/remove members | ✓ | ✗ | ✗ | ✗ | ✗ |
| Assign household roles | ✓ | ✗ | ✗ | ✗ | ✗ |
| Link/unlink user accounts | ✓ | ✗ | ✗ | ✗ | ✗ |
| Create/assign tasks | ✓ | ✓ | ✗ | ✗ | ✗ |
| Complete tasks (self) | ✓ | ✓ | ✗ | ✓ | ✓ |
| Create calendar events | ✓ | ✓ | ✗ | ✗ | ✗ |
| View calendar | ✓ | ✓ | ✓ | ✗ | ✗ |
| Manage responsibility domains | ✓ | ✓ | ✗ | ✗ | ✗ |

Service role members see only the tasks assigned to them. They do not see other household surfaces.

Collaborator role members see only the task/area scope assigned to them by a Manager. The specific scope definition is V2/V3.

---

### Sensitive Fields Visibility

Sensitive fields in `MemberProfileView`:
- `legalName` - visible to Manager only
- `notes` - visible to Manager only

In V2, when medical/emergency data is introduced, it will follow the same pattern: a `VisibilityLevel` value on each sensitive record determines the minimum HouseholdRole needed to read it.

---

### Account Linking Rules

- Only Manager-role members can invoke `link-member-to-user`
- A `UserId` can be linked to at most one member per family
- `Child` members cannot be linked to a `UserId` in V1.1
- `ServiceProvider` and `Guest` members may be linked - this is the mechanism for temporary system access
- A linked account inherits the `HouseholdRole` of the member it is linked to
- When a member is deactivated or archived, the linked account loses access (downstream auth enforcement, not domain logic)

---

## 7. Incremental Delivery Plan

### Phase 1 - Minimal Viable (V1.1)

**Goal**: Households can model non-resident and temporary members, manage member lifecycle, and see a properly grouped directory.

| Item | Action |
|------|--------|
| Retire `Dependent` entity | Remove from domain model; children registered as `Member` with kind=Child |
| Add `MemberKind` value object | Adult, Child, Caregiver, Guest, ExternalCollaborator, ServiceProvider, ExtendedFamily |
| Add `ResidencyType` value object | Resident, Temporary, NonResident |
| Add `MemberStatus` value object | Planned, Active, Inactive, Archived |
| Add `HouseholdRole` value object | Manager, Participant, Observer, Collaborator, Service |
| Add `DisplayIdentity` value object | PreferredName (required), LegalName, Nickname, AvatarRef |
| Add `MembershipPeriod` value object | StartDate, EndDate, nullable |
| Update `Member` entity | New fields, enforce new invariants |
| Extend `add-member` slice | Accept kind, residencyType, householdRole, membershipPeriod |
| Add `register-temporary-member` slice | Guest/ServiceProvider with required EndDate |
| Add `activate-member` slice | Planned → Active |
| Add `deactivate-member` slice | Active → Inactive |
| Add `archive-member` slice | Inactive → Archived |
| Add `set-membership-period` slice | Set/update temporal bounds |
| Add `update-member-profile` slice | Update DisplayIdentity, notes, birthDate |
| Add `assign-household-role` slice | Change HouseholdRole with kind/role validation |
| Add `link-member-to-user` slice | Member ↔ User association |
| Add `unlink-member-from-user` slice | Break association |
| Extend `assign-relationship` slice | Add Grandparent, ExtendedFamily, ServiceRelation types |
| Add `view-member-directory` query | Grouped by kind, excludes Archived |
| Add `view-member-profile` query | Full detail with sensitive field filtering |
| Add `view-active-members` query | For cross-context consumption |
| Add `view-assignable-members` query | Task assignment helper |
| EF Core schema migration | New columns on Members table; retire Dependents table |
| Update `MemberAdded` event payload | Include kind, residencyType, householdRole |
| Retire `DependentAdded` / `DependentRemoved` events | Remove from emitters; update any existing handlers |

---

### Phase 2 - Enrichment (V2)

**Goal**: Members have real contact information, emergency contacts, and the system can surface expired memberships automatically.

| Item | Action |
|------|--------|
| `ContactInfo` entity (under Member) | Email, Phone, WhatsApp, Telegram with IsPrimary |
| `add-member-contact` slice | |
| `remove-member-contact` slice | |
| `set-primary-contact` slice | |
| `EmergencyContact` entity (under Member) | Name, contact info, relationship, priority |
| `add-emergency-contact` slice | |
| `remove-emergency-contact` slice | |
| `view-expired-memberships` query | Members with past EndDate and Active status |
| Children account linking | `LinkedUserId` on Child member with parental consent flag |
| Skill tags | `SkillTag` collection on Member; `add-skill-tag` / `remove-skill-tag` slices |
| `MemberContactAdded` / `MemberContactRemoved` events | |
| Presence schedule definition | `ExpectedPresence` value object (days of week, time windows) |

---

### Phase 3 - Advanced (V3)

**Goal**: Full privacy model, contextual permissions, and distributed family support.

| Item | Action |
|------|--------|
| Medical/sensitive data | `MedicalProfile` entity with `VisibilityLevel` |
| Document references | Metadata-only; no file storage in domain |
| Communication preferences | Language, timezone, quiet hours per member |
| Contextual permissions | Per-area scope for Collaborator role |
| Financial participation model | Expense weighting per member kind |
| Multi-family identity | Single User linked to members in multiple families |
| Identity merging | Same subject appearing in multiple family contexts |

---

## 8. Risks and Conflicts

### Risk 1 - Dependent entity retirement breaks emitted events

**Severity**: Medium  
**Description**: The context spec and domain events doc reference `DependentAdded` / `DependentRemoved`. If any downstream handlers (Calendar, Tasks, Responsibilities) consume these events, retiring them will break those consumers.  
**Resolution**: Audit all `IDomainEventHandler<DependentAdded>` registrations before removing the events. The expected result is zero handlers exist given there is no `add-dependent` slice. Confirm before deleting the event types.

---

### Risk 2 - Existing `add-member` callers use the `role` field with current role values

**Severity**: Low  
**Description**: Any API caller (frontend, mobile, tests) passing `role = "Adult"` / `"Child"` / `"Caregiver"` to `add-member` will be affected if the field is renamed to `kind`.  
**Resolution**: Keep the field name as `kind` in the new API contract and update all callers at the same time. Do not introduce an alias. The V1.1 release is the migration window.

---

### Risk 3 - `MemberRole` renamed to `MemberKind` touches the domain model used in other contexts

**Severity**: Medium  
**Description**: The `MemberRole` value object is referenced in the Responsibilities and Calendar contexts for filtering participants and assignees. Renaming it requires updates in those contexts' read-side logic.  
**Resolution**: Rename `MemberRole` to `MemberKind` globally as part of the V1.1 migration. All cross-context references use `MemberId` only for write operations; read-side projections that filter by role need to be updated to filter by `kind`.

---

### Risk 4 - ServiceProvider task assignment constraint is cross-context

**Severity**: Medium  
**Description**: The invariant "ServiceProvider members may only be assigned tasks in their designated category" is a business rule that involves both the Member kind (Family context) and task assignment (Tasks context). Neither context alone can enforce it without coupling.  
**Resolution**: This constraint is enforced by the `assign-task` handler in the Tasks context by consulting the `AssignableMembersView` read model (which exposes `kind` and `householdRole`). The domain invariant belongs in the Tasks context, not in the Family context. The Family context's responsibility is only to correctly model and expose `MemberKind`. The specific category scoping for ServiceProvider is deferred to V2 when the `Collaborator` scope model is introduced.

---

### Risk 5 - HouseholdRole enforcement requires the calling user's linked member identity

**Severity**: Low  
**Description**: Role-checking in handlers (e.g., "only Manager can invoke assign-household-role") requires knowing the calling user's Member identity and their `HouseholdRole`. This implies the application layer must resolve `UserId → MemberId → HouseholdRole` on each protected command.  
**Resolution**: This is an application/infrastructure concern. The current `ICurrentUserAccessor` provides `UserId`. An additional `ICurrentMemberAccessor` or equivalent query resolves `MemberId` and `HouseholdRole` for the family in context. This seam already exists in security docs (User → Member separation). The implementation is an infrastructure addition, not a domain change.

---

### Risk 6 - Automated membership expiry requires a background scheduler

**Severity**: Low (V1.1 deferred)  
**Description**: Time-bounded memberships need to transition to `Inactive` when `EndDate` passes. This requires a background process or scheduled job infrastructure.  
**Resolution**: Deferred to V2. In V1.1, expiry is manager-triggered. The `view-expired-memberships` query (V2) surfaces records needing deactivation. No infrastructure debt is introduced in V1.1.

---

### Risk 7 - Member archive prevents history loss without deleting records

**Severity**: Low  
**Description**: Soft deletion (Archive) means historical records accumulate. Read model queries must always explicitly filter Archived members.  
**Resolution**: All queries that produce operational surfaces (directory, assignable members, calendar participants) must include `WHERE status != Archived` explicitly. This is a database query discipline item, not a domain risk. It must be documented in the slice conventions for this context.

---

## Appendix - Terminology Alignment

| New Term | Replaces | Notes |
|----------|----------|-------|
| `MemberKind` | `MemberRole` | Broader classification including external kinds |
| `HouseholdRole` | (new concept) | Authorization capacity; was conflated with MemberRole before |
| `MemberStatus` | (new concept) | Explicit lifecycle state |
| `ResidencyType` | (implicit in MemberRole) | Separated into its own dimension |
| `DisplayIdentity` | flat `name` field | Structured identity with PreferredName required |
| `MembershipPeriod` | (new concept) | Temporal bounds; optional for most, required for Guest |
| `Child` (MemberKind) | `Dependent` entity | Children are members; Dependent entity retired |
