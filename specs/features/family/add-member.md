# Spec - Add Member

## Purpose

Add a new person to an existing family.

This capability extends the household identity structure and enables later assignment in responsibilities, calendar participation, and tasks. 

## Context

- Module: Family
- Aggregate: `Family`
- Slice: `add-member`
- Command: `AddMember`

## Inputs

Required:

- `familyId`
- `memberId`
- `name`
- `role`

Optional:

- `birthDate`
- `notes`

## Preconditions

- target family must exist
- `memberId` must be unique within the family
- role must be valid in Family language
- command modifies only the `Family` aggregate 

## State Changes

On success, the `Family` aggregate adds a new active member.

The member becomes part of the family roster and may later be referenced by other contexts through `MemberId` only. 

## Invariants

- a member belongs to exactly one family
- member identifiers must be unique within the family
- relationships cannot reference unknown members 

## Events

Emit:

- `MemberAdded` 

## Success Result

Return:

- `familyId`
- `memberId`
- `name`
- `role`
- `status = added`

## Failure Cases

- family not found
- duplicate `memberId`
- invalid role
- invalid or empty `name`

## Notes

Other contexts may consume `MemberAdded`, but only Family owns membership lifecycle. 