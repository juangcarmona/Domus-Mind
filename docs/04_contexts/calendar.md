# DomusMind - Calendar Context

## Purpose

The Calendar context defines the **temporal structure of family life**.

It is responsible for representing:

* events
* schedules
* participants
* reminders
* the family timeline

This context answers:

* what is happening
* when it happens
* who is involved

It does not own identity or responsibility structures. 

---

# Responsibilities

The Calendar context is responsible for:

* scheduling events
* updating event schedules
* managing event participation
* generating reminders
* maintaining the unified family timeline

It is the source of truth for **time-based planning** in the household.

User interfaces may present events as **Plans**, but the domain model remains centered on the **Event aggregate**.

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

Generated operational work belongs to the **Tasks context**.

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

---

# Value Objects

Suggested value objects:

* `EventId`
* `FamilyId`
* `EventTitle`
* `EventDescription`
* `EventType`
* `EventSchedule`
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

Suggested future queries:

* `GetDailyAgenda`
* `GetWeeklyAgenda`
* `GetFamilyTimeline`

---

# Domain Events Emitted

The Calendar context emits:

* `EventScheduled`
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

Tasks may be generated from events.

Examples:

```
prepare school bag
buy ingredients
bring documents
```

Tasks represent **operational work**.

Key boundary rule:

* **Calendar owns time-bound commitments**
* **Tasks owns operational work**

Recurring fixed-time activities remain in **Calendar**.

Generated work belongs to **Tasks**.

Example:

```
Event: football practice every Tuesday
Task: pack sports bag
```

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
* routine-generated work (Tasks)

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
