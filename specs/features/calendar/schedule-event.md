# Spec - Schedule Event

## Purpose

Create a time-bound event in the family calendar.

Events represent scheduled activities that affect one or more family participants. :contentReference[oaicite:0]{index=0}

## Context

- Module: Calendar
- Aggregate: `Event`
- Slice: `schedule-event`
- Command: `ScheduleEvent`

## Inputs

Required:

- `eventId`
- `familyId`
- `title`
- `startTime`

Optional:

- `endTime`
- `participants`
- `description`
- `responsibilityDomainId`

## Preconditions

- target family must exist
- `eventId` must be unique
- `startTime` must be valid
- if provided, `endTime` must be after `startTime`
- command must modify a single aggregate boundary :contentReference[oaicite:1]{index=1}

## State Changes

On success, the system creates a new `Event` aggregate with:

- stable identifier
- family association
- event schedule
- optional participant set :contentReference[oaicite:2]{index=2}

## Invariants

- every event belongs to one family
- event schedule must be valid
- participant identifiers must reference family entities :contentReference[oaicite:3]{index=3}

## Events

Emit:

- `EventScheduled` :contentReference[oaicite:4]{index=4}

## Success Result

Return:

- `eventId`
- `familyId`
- `title`
- `startTime`
- `status = scheduled`

## Failure Cases

- family not found
- duplicate `eventId`
- invalid schedule
- invalid participant references

## Notes

Other contexts may react to this event to generate operational tasks.