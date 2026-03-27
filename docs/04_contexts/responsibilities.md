# DomusMind - Responsibility Context

## Purpose

The Responsibility context defines how household responsibilities are explicitly distributed across family members.

In product surfaces, responsibility domains may be presented as Areas. This is a UI and product-language mapping only; the domain model remains centered on `ResponsibilityDomain`, `PrimaryOwner`, `SecondaryOwner`, and `TransferResponsibility`.

It is responsible for representing:

- responsibility domains
- ownership assignments
- responsibility hierarchy
- transfer of ownership
- participation in a domain

This context makes cognitive ownership visible, structured, and traceable.

It models accountability, not execution. The context determines who owns a household area of management; it does not execute plans or tasks on behalf of that member.

---

# Responsibilities

The Responsibility context is responsible for:

- creating responsibility domains
- assigning a primary owner
- assigning secondary owners
- assigning participants or collaborators
- transferring ownership
- keeping responsibility assignments consistent with the family structure

It is the source of truth for **who is accountable for what** inside the household.

---

# Aggregate Roots

## ResponsibilityDomain

The `ResponsibilityDomain` aggregate is the primary aggregate root of this context.

It owns:

- responsibility identity
- ownership assignments
- participant assignments
- transfer history semantics, if modeled inside the aggregate

A responsibility domain represents a bounded area of household management.

In the product experience, this is the concept exposed to households as an Area.

Examples:

- school
- food
- finances
- maintenance
- administration
- pets
- travel
- logistics

---

# Internal Entities

## ResponsibilityAssignment

Represents the association between a responsibility domain and a family member.

Possible assignment roles:

- primary owner
- secondary owner
- participant

Product surfaces may label these roles as Owner and Support, while preserving the domain semantics of `PrimaryOwner` and `SecondaryOwner`.

---

# Value Objects

Suggested value objects:

- `ResponsibilityDomainId`
- `FamilyId`
- `MemberId`
- `ResponsibilityDomainName`
- `ResponsibilityRole`
- `ResponsibilityStatus`

Optional future value objects:

- `ResponsibilityDescription`
- `AssignmentEffectivePeriod`

Identifiers must remain strongly typed.

---

# Invariants

The ResponsibilityDomain aggregate must enforce the following invariants:

## Identity and Ownership

- a responsibility domain must belong to exactly one family
- a responsibility domain must have a stable `ResponsibilityDomainId`
- a responsibility domain may have at most one active primary owner
- secondary owners must be unique within the domain
- participants must be unique within the domain

## Family Consistency

- every assigned member must belong to the same family as the responsibility domain
- assignments must reference valid existing family members
- removed or inactive family members cannot receive new assignments

## Role Consistency

- the same member cannot appear twice with the same active role
- a primary owner cannot also be assigned as a secondary owner for the same active domain unless explicitly allowed by model evolution
- transfer of primary ownership must result in exactly one active primary owner

## Lifecycle Integrity

- ownership changes must be explicit
- responsibility domains should not exist in an invalid unowned state if the domain requires a primary owner
- if ownership is optional during creation, the aggregate must support an explicit transition to assigned state

## Ownership Boundary

- only the Responsibility context may change ownership assignments
- other contexts may reference ownership information but must not mutate it

---

# Commands

Core commands owned by this context:

- `CreateResponsibilityDomain`
- `AssignPrimaryOwner`
- `AssignSecondaryOwner`
- `RemoveSecondaryOwner`
- `AddParticipantToResponsibilityDomain`
- `RemoveParticipantFromResponsibilityDomain`
- `TransferResponsibility`
- `RenameResponsibilityDomain`
- `ArchiveResponsibilityDomain`

Suggested future commands:

- `ReactivateResponsibilityDomain`
- `ClearPrimaryOwner`
- `SetResponsibilityDescription`

---

# Queries

Core queries supported by this context:

- `GetResponsibilityDomain`
- `GetResponsibilityDomainsByFamily`
- `GetResponsibilityAssignments`
- `GetResponsibilitiesByMember`
- `GetPrimaryResponsibilitiesByMember`
- `GetSecondaryResponsibilitiesByMember`

Suggested future queries:

- `GetResponsibilityCoverage`
- `GetUnassignedResponsibilityDomains`
- `GetResponsibilityLoadOverview`

---

# Domain Events Emitted

The Responsibility context emits:

- `ResponsibilityDomainCreated`
- `ResponsibilityDomainRenamed`
- `PrimaryOwnerAssigned`
- `SecondaryOwnerAssigned`
- `SecondaryOwnerRemoved`
- `ResponsibilityParticipantAdded`
- `ResponsibilityParticipantRemoved`
- `ResponsibilityTransferred`
- `ResponsibilityDomainArchived`

These events must be emitted only after successful state change.

---

# Domain Events Consumed

The Responsibility context depends on Family for assignment validity.

It may consume:

- `FamilyCreated`
- `MemberAdded`
- `MemberRemoved`

Possible uses:

- bootstrap default responsibility domains for a new family
- validate or reconcile assignments when membership changes
- mark domains needing reassignment after member removal

Bootstrapping default responsibility domains for a new family supports a low-friction product experience where households begin with useful Areas without admin-heavy setup.

Default rule:

**Responsibility depends on Family identity, but owns all responsibility semantics.**

It should not consume downstream operational events from:

- Tasks
- Food
- Inventory
- Administration

unless a future automation layer requires derived behavior.

---

# Read Models

Useful read models for this context:

## Responsibility Domain Summary

Contains:

- domain ID
- family ID
- domain name
- primary owner
- number of secondary owners
- number of participants
- status

This read model may be rendered in product surfaces as an Area summary.

## Family Responsibility Matrix

Contains:

- all domains in a family
- primary owner per domain
- secondary owners
- participants

This read model supports household clarity and mental-load visibility. It is supporting structure for planning and accountability, not a standalone dashboard or reporting module.

## Member Responsibility Overview

Contains:

- member ID
- domains owned as primary
- domains owned as secondary
- domains where member participates

Useful for balancing responsibility across the household.

Plans and tasks may optionally reference a responsibility domain for categorization and accountability context. Sparse usage is acceptable in V1; the household should still function well even when only a few Areas are actively used.

## Responsibility Coverage View

Contains:

- assigned domains
- partially assigned domains
- unassigned domains
- archived domains

Useful for detecting operational risk.

---

# Boundaries With Other Contexts

## Family Context

Family owns household membership.

Responsibility references `FamilyId` and `MemberId`, but does not own members.

Integration rule:

- Family defines who exists
- Responsibility defines who is accountable

## Calendar Context

Calendar events may optionally reference a responsibility domain for categorization or routing.

Calendar does not own responsibility assignments.

Integration rule:

- Calendar may use `ResponsibilityDomainId`
- Responsibility owns ownership semantics

## Tasks Context

Tasks may be generated under a responsibility domain or assigned based on ownership rules.

Tasks does not own responsibility structure.

Integration rule:

- Tasks may reference `ResponsibilityDomainId`
- Responsibility owns accountability relationships

## Administration / Food / Pets / Property Contexts

Operational contexts may map work to a responsibility domain.

They do not define or change ownership.

---

# Ubiquitous Language Notes

Within this context:

- `Responsibility Domain` means an area of household accountability
- `Primary Owner` means the main accountable member
- `Secondary Owner` means backup or shared accountable member
- `Participant` means a member involved but not primarily accountable
- `Transfer Responsibility` means changing the primary owner explicitly

Do not use ambiguous synonyms such as:

- admin
- manager
- assignee
- category
- tag

unless another context explicitly owns those meanings.

---

# Slice Mapping

Initial slices mapped to this context:

- `create-responsibility-domain`
- `assign-primary-owner`
- `assign-secondary-owner`
- `remove-secondary-owner`
- `add-responsibility-participant`
- `transfer-responsibility`

These slices should operate only on the `ResponsibilityDomain` aggregate.

---

# Transaction Rules

Rules:

- one command modifies one `ResponsibilityDomain` aggregate
- all assignment changes occur inside the `ResponsibilityDomain` transaction boundary
- cross-context reactions occur through domain events
- no command may modify Family directly

Example:

`AssignPrimaryOwner`
→ updates `ResponsibilityDomain`
→ emits `PrimaryOwnerAssigned`

Other modules may react after commit.

---

# Design Notes

The Responsibility context models **accountability**, not execution.

It must not absorb logic that belongs to:

- scheduling events
- completing tasks
- generating reminders
- maintaining inventory
- planning meals

This context answers:

- which responsibility domains exist?
- who owns each one?
- who supports each one?
- where are the gaps?

It does not answer:

- what happened today?
- what task is due now?
- what reminder should fire?

Those belong to other contexts.

---

# Summary

The Responsibility context defines the **accountability structure of the household**.

It owns:

- responsibility domains
- primary ownership
- secondary ownership
- participation
- transfer of accountability

It depends on Family for identity and supports the rest of DomusMind by making responsibility explicit, visible, and stable.
