# Spec - View Family Timeline

## Purpose

Provide a unified chronological view of household activity.

The timeline aggregates **events** and **tasks** for a family. 

## Context

* Module: Calendar (read model)
* Read Model: `FamilyTimeline`
* Slice: `view-family-timeline`
* Query: `GetFamilyTimeline`

## Inputs

Required:

* `familyId`

Optional:

* `startTime`
* `endTime`
* `participantId`

## Preconditions

* target family must exist
* time range must be valid if provided

## Data Sources

The timeline read model may include:

* scheduled events
* upcoming tasks
* recently completed tasks
* routine-generated tasks

## Query Behavior

The system retrieves timeline items ordered by time.

Items are merged from multiple contexts into a single chronological view.

Participant filtering may match any participant associated with an event or task.

## Result Structure

Each timeline item includes:

* `type` (`event | task`)
* `title`
* `time`
* `participants`
* `status`

Participant display data should be richer than raw IDs.

A participant projection may include:

* `participantId`
* `displayName`
* `kind` (`member | dependent | pet`)
* optional display metadata needed by the UI

## Success Result

Return:

* ordered list of timeline items
* filtered by family and optional time range

## Failure Cases

* family not found
* invalid time range

## Notes

This query represents the primary operational overview of household activity in DomusMind.

Coordination **cues** are reserved for read models such as the weekly grid.

They are projection-only artifacts and are **not part of the timeline contract yet**.
