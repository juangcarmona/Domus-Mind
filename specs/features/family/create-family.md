# Spec - Create Family

## Purpose

Create the root household unit in DomusMind.

This capability establishes the initial identity boundary for all downstream contexts. A family is the primary operational unit of the system. 

## Context

- Module: Family
- Aggregate: `Family`
- Slice: `create-family`
- Command: `CreateFamily`

## Inputs

Required:

- `familyId`
- `name`

Optional:

- `createdByUserId`

## Preconditions

- `familyId` must be unique
- `name` must be non-empty
- command must target exactly one aggregate boundary 

## State Changes

On success, the system creates a new `Family` aggregate with:

- stable `FamilyId`
- household name
- empty member set
- empty dependent set
- empty pet set
- empty relationship set 

## Invariants

- a family must have a stable identifier
- family structure is owned only by the Family context
- no other context may create household identity 

## Events

Emit:

- `FamilyCreated` 

## Success Result

Return:

- `familyId`
- `name`
- `status = created`

## Failure Cases

- duplicate `familyId`
- invalid or empty `name`
- malformed command payload

## Notes

This is the first capability in V1 and the root dependency for:

- members
- responsibilities
- events
- tasks