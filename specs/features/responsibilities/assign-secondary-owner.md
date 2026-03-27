# Spec - Assign Secondary Owner

## Purpose

Assign a secondary owner to a responsibility domain.

A secondary owner provides backup or shared accountability for the domain.

In product surfaces, this role may be labeled Support for the corresponding Area.

## Context

- Module: Responsibilities
- Aggregate: `ResponsibilityDomain`
- Slice: `assign-secondary-owner`
- Command: `AssignSecondaryOwner`

## Inputs

Required:

- `responsibilityDomainId`
- `memberId`

Optional:

- `effectiveDate`

## Preconditions

- responsibility domain must exist
- member must exist in the same family
- member must not already be a secondary owner
- command modifies only the `ResponsibilityDomain` aggregate

## State Changes

On success, the member is added to the secondary owner set.

The primary owner remains unchanged.

This assignment extends accountability coverage. It does not imply execution responsibility for every related plan or task.

## Invariants

- secondary owners must be unique within the domain
- assigned members must belong to the same family
- ownership structure must remain valid

## Events

Emit:

- `SecondaryOwnerAssigned`

## Success Result

Return:

- `responsibilityDomainId`
- `memberId`
- `status = assigned`

## Failure Cases

- responsibility domain not found
- member not found
- duplicate secondary owner

## Notes

Secondary owners support redundancy and reduce concentration of mental load.

UI wording should preserve the domain distinction: Support is a product label for `SecondaryOwner`.