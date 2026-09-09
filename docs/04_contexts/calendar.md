# DomusMind - Calendar Context

## Purpose

The Calendar context defines the **temporal structure of family life**.

It is responsible for representing:

* events
* external calendar connections
* external calendar feeds
* external calendar entries used for read-only projection
* schedules
* participants
* reminders

This context answers:

* what is happening
* when it happens
* who is involved

It does not own identity or responsibility structures.
It does not own the Agenda read model.
It does not own the family timeline — the timeline is a cross-context read model that draws from Calendar, Tasks, and Shared Lists.
Calendar is one source for those projections, not the owner.

---

# Responsibilities

The Calendar context is responsible for:

* scheduling events
* updating event schedules
* managing event participation
* generating reminders
* ingesting selected external calendar data for member-scoped projections
* providing calendar events and external calendar entries as sources for the Agenda and family timeline read models

Calendar is the source of truth for **time-based planning** in the household.
Calendar owns the Event aggregate. The Agenda and family timeline read models are not owned by Calendar — they are assembled from multiple contexts.

User interfaces may present events as **Plans**, but the domain model remains centered on the **Event aggregate**.

Imported external calendar data must remain **read-only integration state**.
It must not be treated as native household planning state.

---

# Aggregate Roots

## Event

The `Event` aggregate is the primary aggregate root owned by the Calendar context.

It owns:

* event identity
* event schedule
* event participants
* reminder definitions
* event status

UI surfaces may present an Event as a **Plan**, but this is a **presentation concern only**.

The domain language of the Calendar context remains **Event**.

Examples of events:

* school activity
* medical appointment
* sports training
* trip
* maintenance appointment
* family gathering

## ExternalCalendarConnection

The `ExternalCalendarConnection` aggregate is the member-scoped aggregate root for third-party calendar ingestion.

It owns:

* connection identity
* member association
* provider identity
* delegated auth metadata
* selected feed configuration
* sync settings
* sync status

The aggregate does not own native household scheduling semantics.

It exists to manage read-only ingestion from external providers while preserving the boundary between imported entries and native `Event` aggregates.

Phase 1 provider support:

* Microsoft Outlook via Microsoft Graph delegated access

Future providers may be added later without changing the native `Event` model.

---

# Schedule Semantics

The event schedule defines the **temporal structure** of an event.

It includes:

* start time
* optional end time
* optional recurrence rule

Recurrence rules allow events to represent **recurring time-bound commitments**.

Examples:

```
football practice every Tuesday
school every weekday
weekly language class
```

Recurrence describes **time commitments**, not operational work.

Operational work belongs to the **Tasks context** when it is explicitly created.

---

# Internal Entities

## Participant

Represents an entity participating in an event.

Participants may be:

* members
* dependents
* pets

Participants are referenced by identity from the Family context.

Participant visibility is essential because the coordination question is:

> Who needs to be where, and when?

---

## Reminder

Represents a scheduled notification associated with an event.

Examples:

* 24 hours before
* 2 hours before
* 30 minutes before

Reminder scheduling belongs to the Event aggregate.

Notification delivery belongs to infrastructure.

## ExternalCalendarFeed

Represents one selected provider calendar under an `ExternalCalendarConnection`.

An external calendar feed tracks:

* provider calendar identity
* calendar display name
* whether the calendar is selected
* sync horizon configuration
* last successful sync time
* delta cursor for the active view

Delta cursor validity is tied to the active sync horizon.
If the horizon changes, the stored cursor must be discarded and the feed must perform a fresh bounded sync.

## ExternalCalendarEntry

Represents a read-only imported occurrence stored for projection into Agenda read models.

An external calendar entry may include:

* provider event identity
* iCal UID
* series master reference
* title
* start and end time
* all-day flag
* original timezone
* location
* participant summary
* provider status
* provider payload version or hash
* provider last-modified timestamp
* deletion tombstone state

An external calendar entry is not an `Event` aggregate and must not emit native Calendar domain events such as `EventScheduled`.

---

# Value Objects

Suggested value objects:

* `EventId`
* `ExternalCalendarConnectionId`
* `FamilyId`
* `EventTitle`
* `EventDescription`
* `EventType`
* `EventSchedule`
* `ExternalCalendarProvider`
* `SyncHorizon`
* `ExternalCalendarCursor`
* `ReminderOffset`
* `ParticipantId`
* `ParticipantType`
* `EventStatus`

Optional future value objects:

* `Location`
* `TravelBuffer`
* `EventVisibility`
* `PreparationNotes`
* `OccurrenceNotes`

Preparation or occurrence notes may support metadata such as:

```
bring school documents
prepare equipment
bring vaccination card
```

These are optional metadata attached to events.

---

# Invariants

The Event aggregate must enforce the following invariants.

## Identity

* every event must have a stable `EventId`
* every event must belong to exactly one family

## Schedule

* every event must define a valid schedule
* start time must be before end time when both exist
* recurring events must define a recurrence rule

## Participation

* participants must be unique within an event
* participants must reference valid family entities
* unknown participants are not allowed

## Reminder Integrity

* reminder offsets must be unique per event
* reminders must reference the event schedule

## Lifecycle

* cancelled events cannot be modified except for archival
* completed events cannot change schedule

## Ownership Boundary

* only the Calendar context may change event schedules
* participant identity must be validated against Family
* responsibility routing must remain external to the calendar

## External Calendar Connection Integrity

* one member may own zero to many external calendar connections
* each connection points to exactly one provider account
* duplicate active connections for the same member and provider account are not allowed
* selected feed identities must be unique within a connection
* sync horizon options are bounded to supported values
* changing the sync horizon invalidates existing delta state for affected feeds
* only selected feeds may ingest external entries

## External Calendar Entry Boundary

* imported external entries are read-only inside DomusMind
* imported external entries must not be converted automatically into native `Event` aggregates
* imported external entries may be projected into Agenda member views when relevant
* imported external entries do not appear in household-native event write flows
* deletion from the provider must be reflected through tombstoning or removal in local integration storage

---

# Commands

Core commands owned by this context:

* `ScheduleEvent`
* `RescheduleEvent`
* `CancelEvent`
* `AddEventParticipant`
* `RemoveEventParticipant`
* `AddReminder`
* `RemoveReminder`
* `RenameEvent`
* `ConnectOutlookAccount`
* `ConfigureExternalCalendarConnection`
* `SyncExternalCalendarConnection`
* `DisconnectExternalCalendarConnection`

Suggested future commands:

* `CompleteEvent`
* `CreateRecurringEvent`
* `SkipOccurrence`
* `MoveOccurrence`

---

# Queries

Core queries supported by this context:

* `GetEvent`
* `GetEventsByFamily`
* `GetEventsByParticipant`
* `GetEventsInTimeRange`
* `GetUpcomingEvents`
* `GetMemberExternalCalendarConnections`
* `GetMemberAgenda`

Suggested future queries:

* `GetDailyAgenda`
* `GetWeeklyAgenda`
* `GetFamilyTimeline`

---

# Domain Events Emitted

The Calendar context emits:

* `EventScheduled`

External calendar ingestion may emit integration-specific events for internal orchestration, such as connection configured or connection synchronized, but these must remain distinct from native household event lifecycle events.

---

# External Calendar Integration Boundary

Phase 1 external calendar ingestion follows these rules.

## Provider and Access Model

Phase 1 supports:

* Microsoft Outlook only
* Microsoft Graph delegated access
* scopes: `Calendars.Read` and `offline_access`

The delegated account is the provider account being read.

## Projection Rule

Imported external calendar entries:

* remain stored as external integration records
* are projected into Agenda member scope only in phase 1
* do not become native household Plans
* are read-only in DomusMind

## Horizon Rule

The recommended default sync horizon is:

* from now - 1 day
* to now + 90 days

Supported phase 1 forward horizon options:

* 30 days
* 90 days
* 180 days
* 365 days

The active sync horizon is part of sync identity for delta tracking.

## Sync Rule

Phase 1 supports both:

* manual sync on one connection
* scheduled refresh of stale connections

Scheduled refresh runs hourly by default, with support for:

* configurable interval in user settings
* login catch-up when stale
* Agenda-open catch-up when stale

The periodic refresher must run in a dedicated background worker.

## Recovery Rule

If delta state becomes invalid, corrupted, or misaligned with the current horizon:

* discard the feed cursor
* clear local entries for the affected feed and window
* run a fresh bounded load
* `EventRescheduled`
* `EventCancelled`
* `EventCompleted`
* `EventParticipantAdded`
* `EventParticipantRemoved`
* `ReminderAdded`
* `ReminderRemoved`

These events must be emitted only after successful state change.

---

# Domain Events Consumed

The Calendar context depends on Family identity.

It may consume:

* `FamilyCreated`
* `MemberAdded`
* `MemberRemoved`
* `DependentAdded`
* `PetAdded`

Possible uses:

* validating participant references
* maintaining read models
* cleaning up invalid participant references

Optional integrations:

* `PrimaryOwnerAssigned` (from Responsibility)

Default rule:

**Calendar consumes identity signals but does not modify identity structures.**

---

# Boundaries With Other Contexts

## Family Context

Family owns identity.

Calendar references:

* `MemberId`
* `DependentId`
* `PetId`

Calendar must not modify family structure.

---

## Responsibility Context

Responsibility domains may optionally categorize events.

Example:

```
school
pets
food
```

Calendar may reference `ResponsibilityDomainId`.

Responsibility owns ownership semantics.

---

## Tasks Context

Tasks owns **operational work associated with those commitments when that work is explicitly created**.

Examples:

```
Event: School trip
    Related tasks:
        prepare backpack
        sign permission form
```

Tasks represent **operational work**.

Key boundary rule:

* **Calendar owns time-bound commitments**
* **Tasks owns operational work explicitly created by the household**

Recurring fixed-time activities remain in **Calendar**.

---

## Reminder / Notification Systems

Reminder execution belongs to infrastructure.

Calendar defines reminder schedules but does not deliver notifications.

---

# Read Models

Useful read models for this context.

## Family Timeline

Contains:

* all events belonging to a family
* ordered by time

Fields:

* event ID
* title
* start time
* end time
* participants
* status

This read model is the **core chronological view of the household**.

---

## Daily Agenda

Contains:

* events occurring today
* grouped by participant

Useful for daily operational views.

---

## Weekly Agenda / Coordination Grid

A weekly planning view may combine information from multiple contexts.

It may display:

* events (Calendar)
* chores or tasks (Tasks)
* routines (projected)

The grid may include **lightweight coordination cues** derived from:

* plans
* routines
* tasks

These cues are **read-model artifacts only**.

They do not represent new aggregates or domain entities.

---

# Ubiquitous Language Notes

Within this context:

* `Event` means a time-bound occurrence
* `Schedule` means the temporal definition of an event
* `Participant` means an entity attending or affected by an event
* `Reminder` means a scheduled notification offset

UI surfaces may refer to events as **Plans**, but the domain language remains **Event**.

Avoid ambiguous synonyms such as:

* appointment
* booking
* calendar item
* entry

unless defined as event subtypes.

---

# Slice Mapping

Initial slices mapped to this context:

* `schedule-event`
* `reschedule-event`
* `cancel-event`
* `add-event-participant`
* `remove-event-participant`
* `add-reminder`

These slices operate only on the `Event` aggregate.

---

# Transaction Rules

Rules:

* one command modifies one `Event` aggregate
* all schedule and participant changes occur inside the `Event` transaction boundary
* cross-context reactions occur through domain events

Example:

```
ScheduleEvent
→ updates Event
→ emits EventScheduled
```

Other modules may react after commit.

---

# Design Notes

The Calendar context models **time and coordination**, not execution.

It must not absorb logic that belongs to:

* task completion
* responsibility ownership
* household inventory
* meal planning
* administration

Calendar answers:

* what is happening
* when it happens
* who participates

Execution belongs elsewhere.

---

# Summary

The Calendar context defines the **temporal coordination layer of DomusMind**.

It owns:

* the `Event` aggregate
* schedules and recurrence semantics
* participants
* reminders
* the family timeline

User interfaces may present events as **Plans**, but the domain model remains centered on **Events**.

Calendar owns **time-bound commitments**, while **Tasks owns operational work**.
