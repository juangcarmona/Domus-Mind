# Calendar Specification

## Purpose

Calendar is the temporal structure of household life. It owns the **Event** aggregate: time-bound commitments that affect one or more household participants.

Calendar answers:
- what is happening in this household
- when it happens
- who is involved

Calendar is the source of truth for time-based planning. It owns event schedules, participants, and reminder definitions. Notification delivery belongs to infrastructure, not to Calendar.

Calendar also manages **external calendar connections** — member-scoped delegated integrations that import read-only entries from third-party providers (Phase 1: Microsoft Outlook) into the household's temporal read surfaces.

Calendar does not own the Agenda or family timeline read models. It is one source for those projections. The Agenda and timeline are assembled from Calendar, Tasks, and Shared Lists.

User interfaces may present Events as **Plans**, but the domain model remains centered on the **Event** aggregate.

Imported external calendar data is read-only integration state. It must never be treated as native household planning state.

---

## Requirements

### Requirement: Event Scheduling

A household SHALL be able to schedule a time-bound event with a title and start time as the minimum required inputs.

Optional inputs: end time (must be after start time if provided), participants (family members, dependents, or pets), and a responsibility domain reference for contextual grouping. A newly created event starts in **scheduled** status and belongs to exactly one family.

#### Scenario: Household creates an event with minimum inputs

- GIVEN a family exists
- WHEN a member schedules an event with a title and start time
- THEN an event is created in scheduled status
- AND the event belongs to that family

#### Scenario: Event with end time before start time is rejected

- GIVEN a member provides a start time and an end time
- WHEN the end time is earlier than the start time
- THEN the event is not created
- AND a validation error is returned

#### Scenario: Event with invalid participant reference is rejected

- GIVEN a participant identifier does not belong to the family
- WHEN a member attempts to schedule an event including that participant
- THEN the event is not created
- AND a validation error is returned

---

### Requirement: Event Rescheduling

A household SHALL be able to update the schedule of a scheduled event.

Rescheduling updates the start time and optionally the end time. The event's identity, participants, and reminders remain unchanged. Cancelled events cannot be rescheduled.

#### Scenario: Event is rescheduled

- GIVEN an event exists in scheduled status
- WHEN the household provides a new start time
- THEN the event schedule is updated
- AND participants and other event properties remain unchanged

#### Scenario: Cancelled event cannot be rescheduled

- GIVEN an event in cancelled status
- WHEN the household attempts to reschedule it
- THEN the operation is rejected

#### Scenario: Rescheduling with invalid times is rejected

- GIVEN a new end time that is earlier than the new start time
- WHEN the household attempts to reschedule the event
- THEN the operation is rejected

---

### Requirement: Event Cancellation

A household SHALL be able to cancel a scheduled event.

Cancellation preserves the event in history but removes it from active planning. Cancelled events cannot be rescheduled or receive new participants.

#### Scenario: Event is cancelled

- GIVEN an event exists in scheduled status
- WHEN the household cancels the event
- THEN the event status becomes cancelled
- AND it is no longer considered active

#### Scenario: Already cancelled event cannot be cancelled again

- GIVEN an event in cancelled status
- WHEN the household attempts to cancel it again
- THEN the operation is rejected

---

### Requirement: Event Participant Management

A household SHALL be able to add and remove participants from an event.

Participants may be family members, dependents, or pets. Each participant must be unique within the event. Participants cannot be added to a cancelled event.

#### Scenario: Participant is added to an event

- GIVEN an event exists in scheduled status
- AND a family entity (member, dependent, or pet) is not already a participant
- WHEN the household adds that entity as a participant
- THEN the entity is added to the event's participant set

#### Scenario: Duplicate participant is rejected

- GIVEN an entity is already a participant in an event
- WHEN the household attempts to add the same entity again
- THEN the operation is rejected

#### Scenario: Participant cannot be added to a cancelled event

- GIVEN an event in cancelled status
- WHEN the household attempts to add a participant
- THEN the operation is rejected

#### Scenario: Participant is removed from an event

- GIVEN an entity is a participant in a scheduled event
- WHEN the household removes that participant
- THEN the entity is no longer in the event's participant set
- AND the event and other participants remain unchanged

#### Scenario: Removing a non-participant is rejected

- GIVEN an entity is not a participant in an event
- WHEN the household attempts to remove that entity
- THEN the operation is rejected

---

### Requirement: Event Reminders

A household SHALL be able to add and remove time-based reminders on an event.

Reminders are defined as offsets relative to the event's start time (e.g. 30 minutes before, 24 hours before). Each offset must be unique per event. Calendar defines reminder schedules; notification delivery belongs to infrastructure.

#### Scenario: Reminder is added to an event

- GIVEN an event exists
- AND no reminder with the same offset already exists on that event
- WHEN the household adds a reminder with an offset
- THEN the reminder is added to the event's reminder set

#### Scenario: Duplicate reminder offset is rejected

- GIVEN a reminder with a specific offset already exists on an event
- WHEN the household adds a reminder with the same offset
- THEN the operation is rejected

#### Scenario: Reminder is removed from an event

- GIVEN a reminder with a specific offset exists on an event
- WHEN the household removes that reminder
- THEN the reminder is no longer part of the event's reminder set
- AND the event and other reminders remain unchanged

#### Scenario: Removing a non-existent reminder is rejected

- GIVEN no reminder with a specific offset exists on an event
- WHEN the household attempts to remove a reminder with that offset
- THEN the operation is rejected

---

### Requirement: Outlook Account Connection

A member SHALL be able to connect a Microsoft Outlook account to their DomusMind identity for read-only calendar ingestion.

Connecting establishes a delegated external calendar connection. Connection requires a successful delegated authorization including `Calendars.Read` and `offline_access` scopes. A member may not have two active connections to the same Outlook account. Connecting does not create native Event aggregates. The initial data import occurs through a subsequent sync operation. On connection creation, default sync settings are applied: a sync horizon of now − 1 day to now + 90 days forward, scheduled refresh enabled with a default interval of 60 minutes.

#### Scenario: Member connects an Outlook account

- GIVEN a family member exists
- AND a successful delegated authorization for a Microsoft account is available
- WHEN the member connects the Outlook account
- THEN an external calendar connection is created associated with that member
- AND the connection status is pending initial sync
- AND no native Event aggregates are created

#### Scenario: Duplicate connection for the same account is rejected

- GIVEN a member already has an active connection to a specific Outlook account
- WHEN the member attempts to connect the same account again
- THEN the operation is rejected

#### Scenario: Connection with missing required scopes is rejected

- GIVEN a delegated authorization does not include the required scopes
- WHEN a member attempts to connect the account
- THEN the connection is rejected

---

### Requirement: External Calendar Configuration

A member SHALL be able to configure which provider calendars are selected for ingestion and which sync horizon applies to a connection.

Supported sync horizons: 30, 90, 180, or 365 days forward. Changing the selected calendars or the sync horizon triggers rehydration of affected feeds. Removed feeds stop projecting entries and their sync state is cleared. A configuration change must not create native Event aggregates.

#### Scenario: Member selects calendars and sets a sync horizon

- GIVEN an active external calendar connection exists
- WHEN the member selects one or more provider calendars and sets a supported horizon value
- THEN the connection updates its selected feed set and horizon configuration

#### Scenario: Deselected feed stops projecting

- GIVEN a feed is currently selected and projecting entries
- WHEN the member deselects that feed
- THEN stored entries from that feed stop appearing in projections
- AND that feed's delta state is cleared

#### Scenario: Horizon change triggers rehydration

- GIVEN an active connection with a stored sync state
- WHEN the member changes the sync horizon to a different supported value
- THEN existing delta cursors for affected feeds are discarded
- AND the next sync performs a fresh bounded load for the new window

#### Scenario: Unsupported horizon value is rejected

- GIVEN a sync horizon value is not one of the supported options (30, 90, 180, 365 days)
- WHEN the member attempts to save the configuration
- THEN the operation is rejected

---

### Requirement: External Calendar Disconnection

A member SHALL be able to disconnect an external calendar connection.

Disconnection removes the connection and all its imported read-only entries from DomusMind. Disconnection does not modify any native Event aggregates. Imported entries owned by the connection stop appearing in Agenda projections immediately after disconnection. Disconnection removes only DomusMind's local integration state and locally held delegated access material — no operations are performed on the provider account or provider-side calendars.

#### Scenario: Member disconnects a connection

- GIVEN an active external calendar connection exists
- WHEN the member disconnects it
- THEN the connection is removed
- AND all imported entries from that connection are cleared or tombstoned
- AND those entries no longer appear in Agenda projections
- AND no native Event aggregates are modified

#### Scenario: Disconnecting a non-existent connection is rejected

- GIVEN a connection ID that does not exist
- WHEN a disconnection is attempted
- THEN the operation is rejected

---

### Requirement: External Calendar Synchronization

A member SHALL be able to manually trigger synchronization of an external calendar connection.

Synchronization imports new, updated, and deleted occurrences from each selected feed within the active sync horizon. The sync operates incrementally using a stored delta cursor when one is available; otherwise it performs a fresh bounded load. Two synchronizations of the same connection must not run concurrently. Imported entries remain read-only.

#### Scenario: Manual sync imports entries from selected feeds

- GIVEN an active connection with at least one selected feed
- WHEN the member triggers a sync
- THEN entries from all selected feeds within the horizon are imported or updated
- AND the sync cursor for each feed is updated

#### Scenario: Sync with invalid delta cursor falls back to fresh load

- GIVEN a stored delta cursor is invalid or stale
- WHEN a sync is triggered
- THEN local entries for the affected feed are cleared
- AND a fresh bounded load is performed
- AND a new cursor is stored

#### Scenario: Concurrent sync for the same connection is rejected

- GIVEN a sync is already running for a connection
- WHEN another sync for the same connection is triggered
- THEN the second request is rejected or deferred

#### Scenario: Sync with no selected feeds is a no-op

- GIVEN a connection has no selected feeds
- WHEN a sync is triggered
- THEN no entries are imported and the sync completes without error

---

### Requirement: Background Feed Refresh

The system SHALL automatically refresh stale external calendar connections in the background so that the Agenda stays current without requiring manual sync.

The default refresh interval is 60 minutes. Catch-up triggers also fire on user login and when Agenda is opened in Member scope with a stale connection. A connection is never synced concurrently with itself. A batch failure for one connection must not prevent other connections from refreshing. Background refresh must not create native Event aggregates.

#### Scenario: Stale connection is refreshed automatically

- GIVEN a connection's last successful sync is older than the configured threshold
- WHEN the background worker evaluates connections
- THEN that connection is synchronized
- AND the sync timestamp is updated

#### Scenario: Already-fresh connection is skipped

- GIVEN a connection was recently synchronized
- WHEN the background worker evaluates connections
- THEN that connection is skipped

#### Scenario: Catch-up sync fires when Agenda opens with stale state

- GIVEN a member opens Agenda in Member scope
- AND one of their connections has stale state
- WHEN the Agenda loads
- THEN a catch-up sync is triggered for the stale connection

---

### Requirement: Household Timeline Projection

The system SHALL produce a unified household-scope temporal read model for a requested date window.

The projection assembles entries from four sources: Calendar Events (as Plans), Tasks (due within the window), Routines (projected occurrences), and temporal Shared List Items (items with a due date, reminder, or repeat rule producing an occurrence within the window). External calendar entries are excluded from the household timeline — they appear in Member scope only. The projection is read-only; it must not create or modify any aggregate. A temporal list item linked to a plan projects independently of the linked event — they appear as separate entries in the timeline.

Projected entries follow this priority order within a day: overdue → tasks due today → list items with importance → plans (by start time) → routines → list items without importance → completed/checked.

#### Scenario: Household timeline includes events, tasks, routines, and list items

- GIVEN a family has scheduled events, pending tasks, active routines, and list items with due dates
- WHEN the household timeline is requested for a date
- THEN all four entry types appear for that date
- AND external calendar entries do not appear

#### Scenario: Temporal list items project into the household timeline

- GIVEN a list item has a due date within the requested window
- WHEN the household timeline is requested
- THEN the list item appears with a list-item type discriminator
- AND it is distinguishable from tasks and plans

#### Scenario: External entries are excluded from the household timeline

- GIVEN a member has imported external calendar entries
- WHEN the household timeline is requested
- THEN those external entries do not appear

---

### Requirement: Member Agenda Projection

The system SHALL produce a member-scoped temporal read model for a requested date window.

The projection assembles entries from five sources: Calendar Events where the member participates or is a household plan, Tasks assigned to the member, Routines for the member or household scope, External Calendar Entries from the member's active connections, and temporal Shared List Items. External entries are scoped to the member whose connection owns them. External entries are read-only and carry a source label (e.g. "Outlook"). An external entry is included only when the connection is active, the feed is selected, the entry falls within the active sync horizon, and the entry has not been tombstoned as deleted. The projection is read-only; it must not create or modify any aggregate.

#### Scenario: Member agenda includes personal events and imported entries

- GIVEN a member participates in an event and has an active Outlook connection with entries
- WHEN the member agenda is requested for that date
- THEN both the native event and the external entries appear
- AND external entries carry a source label and are read-only

#### Scenario: External entries do not appear in household scope

- GIVEN a member has imported external calendar entries
- WHEN the household-scope timeline (not member agenda) is requested
- THEN those external entries are not included

#### Scenario: Member agenda projected list items are not editable in-view

- GIVEN a list item with a due date appears in a member's agenda
- WHEN the member views the agenda
- THEN the list item is distinguishable from tasks and plans
- AND it is not editable from Agenda — editing navigates to the Lists surface

---

## Notes

1. **Recurring events** — `docs/04_contexts/calendar.md` includes `RecurrenceRule` as a value object and mentions recurring events (e.g. "football practice every Tuesday"), but no feature spec covers creating or modifying recurring events beyond the basic schedule invariant (recurring events must define a recurrence rule). This spec captures the invariant. Full recurring event management (skip occurrence, move occurrence) is listed as future scope in the context document.

2. **Event completion** — `CompleteEvent` is listed as a future command in the context document. No feature spec exists. The context document notes "completed events cannot change schedule" as an invariant, which implies the state exists but its behavioral lifecycle is unspecified. This spec does not include a Completion requirement; the invariant is noted here only.

3. **RenameEvent** — `RenameEvent` is listed as an active command in the context document but has no feature spec. Behavior is unspecified beyond identity stability.

4. **Reminder scheduling vs. delivery** — Reminders are defined as time offsets on the event. Delivery (notification infrastructure) is explicitly out of Calendar's scope. Whether reminders auto-remove when an event is cancelled is not specified in any source.

5. **Participant removal from cancelled events** — `cancel-event.md` says cancelled events cannot add participants, but neither it nor `remove-event-participant.md` explicitly states whether removing a participant from a cancelled event is permitted. Not specified.

6. **Phase 1 sync: pull only** — External calendar synchronization is pull-based only. Webhook subscriptions are explicitly out of scope for Phase 1.

7. **`pausedUntil` for connections** — No equivalent time-bounded pause exists in the Calendar external connection model (unlike Routine Pause in Tasks). Connections are either active or disconnected.

8. **Background refresh triggers** — Catch-up triggers fire on login and when Agenda opens in Member scope. The exact staleness threshold (distinct from the 60-minute scheduled interval) is not specified in the source documents.

---

## Source References

- `docs/04_contexts/calendar.md` — primary context document: aggregates, invariants, commands, events, boundary rules, external calendar model
- `specs/features/calendar/schedule-event.md`
- `specs/features/calendar/reschedule-event.md`
- `specs/features/calendar/cancel-event.md`
- `specs/features/calendar/add-event-participant.md`
- `specs/features/calendar/remove-event-participant.md`
- `specs/features/calendar/add-reminder.md`
- `specs/features/calendar/remove-reminder.md`
- `specs/features/calendar/connect-outlook-account.md`
- `specs/features/calendar/configure-external-calendar-connection.md`
- `specs/features/calendar/disconnect-external-calendar-connection.md`
- `specs/features/calendar/sync-external-calendar-connection.md`
- `specs/features/calendar/refresh-external-calendar-feeds.md`
- `specs/features/calendar/view-family-timeline.md`
- `specs/features/calendar/view-member-agenda.md`
- `docs/03_domain/ubiquitous-language.md` — Plan vs Event terminology, Agenda architectural invariant
