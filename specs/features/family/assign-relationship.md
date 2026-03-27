# Spec - Assign Relationship

## Purpose

Define a structural relationship between two family members.

Relationships model household structure such as parent-child or spouse connections. :contentReference[oaicite:1]{index=1}

## Context

- Module: Family
- Aggregate: `Family`
- Slice: `assign-relationship`
- Command: `AssignRelationship`

## Inputs

Required:

- `familyId`
- `memberAId`
- `memberBId`
- `relationshipType`

Optional:

- `effectiveDate`

## Preconditions

- family must exist
- both members must belong to the same family
- members must be distinct
- command modifies only the `Family` aggregate

## State Changes

On success, a relationship is added to the family relationship set.

The relationship becomes part of the household structure.

## Invariants

- relationships must reference existing members
- duplicate relationships of the same type are not allowed
- members must belong to the same family

## Events

Emit:

- `RelationshipAssigned`

## Success Result

Return:

- `familyId`
- `relationshipType`
- `status = assigned`

## Failure Cases

- family not found
- member not found
- duplicate relationship
- invalid relationship type

## Notes

Relationships help contextualize responsibilities, events, and care roles.