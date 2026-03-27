# Spec - Create Responsibility Domain

## Purpose

Create a responsibility domain within a family.

A responsibility domain represents an area of household accountability such as food, school, maintenance, or finances.

In product surfaces, this concept may be presented as an Area.

## Context

- Module: Responsibilities
- Aggregate: `ResponsibilityDomain`
- Slice: `create-responsibility-domain`
- Command: `CreateResponsibilityDomain`

## Inputs

Required:

- `responsibilityDomainId`
- `familyId`
- `name`

Optional:

- `description`

## Preconditions

- target family must exist
- `responsibilityDomainId` must be unique
- `name` must be non-empty
- command must modify a single aggregate boundary

## State Changes

On success, the system creates a new `ResponsibilityDomain` aggregate with:

- stable identifier
- associated `FamilyId`
- domain name
- empty ownership assignments

## Invariants

- a responsibility domain belongs to exactly one family
- domains must have unique identifiers
- ownership assignments may be empty initially

## Events

Emit:

- `ResponsibilityDomainCreated`

## Success Result

Return:

- `responsibilityDomainId`
- `familyId`
- `name`
- `status = created`

## Failure Cases

- family not found
- duplicate `responsibilityDomainId`
- invalid or empty name

## Notes

Ownership may be assigned later through `AssignPrimaryOwner`.

Default responsibility domains may be bootstrapped for a new household so the product can expose useful default Areas without forcing setup during first use.

Plans and tasks may optionally reference the created domain as an Area. Sparse Area usage is acceptable in V1.