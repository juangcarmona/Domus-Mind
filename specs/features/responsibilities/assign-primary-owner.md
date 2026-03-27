# Spec - Assign Primary Owner

## Purpose

Assign the primary owner of a responsibility domain.

The primary owner is the member accountable for that domain of household management.

In product surfaces, this role may be labeled Owner for the corresponding Area.

## Context

- Module: Responsibilities
- Aggregate: `ResponsibilityDomain`
- Slice: `assign-primary-owner`
- Command: `AssignPrimaryOwner`

## Inputs

Required:

- `responsibilityDomainId`
- `memberId`

Optional:

- `effectiveDate`

## Preconditions

- responsibility domain must exist
- member must exist in the family
- command must modify only the `ResponsibilityDomain` aggregate 

## State Changes

On success, the system assigns the member as the primary owner of the responsibility domain.

If a previous owner exists, ownership is transferred.

This assignment clarifies accountability, not who will execute every related plan or task.

## Invariants

- a domain may have only one primary owner
- the owner must belong to the same family
- ownership must remain consistent with the family structure

## Events

Emit:

- `PrimaryOwnerAssigned`

## Success Result

Return:

- `responsibilityDomainId`
- `memberId`
- `status = assigned`

## Failure Cases

- responsibility domain not found
- member not found
- invalid ownership assignment

## Notes

Other contexts may react to this event to route tasks or categorize events under the domain.

UI wording should not collapse the domain distinction: Owner is a product label for `PrimaryOwner`, not a replacement for the domain concept.