# Surface Spec - Settings

## Purpose

Provide the household's low-frequency configuration surface.

Settings answers:

- how people and household preferences are configured
- how personal connection-based integrations are managed
- where status and maintenance actions live when they do not belong in operational surfaces

Settings is where phase 1 Outlook calendar connections are managed.

---

## Depends On

- `docs/00_product/experience.md`
- `docs/00_product/surface-system.md`
- `docs/04_contexts/family.md`
- `docs/04_contexts/calendar.md`
- `docs/06_interfaces/external-calendar-api.md`

---

## Entry Points

- main navigation → `Settings`
- profile or account menu → `Settings`
- contextual link from Agenda when calendar connection setup is missing
- post-connect or sync-failure recovery link

---

## Role

Settings is the secondary configuration surface.

It is:

- low-frequency
- explicit
- maintenance-oriented
- compact

It is not:

- a dashboard
- the main household coordination surface
- a generic admin back office
- a place to edit temporal data that belongs in Agenda

---

## Shell

Settings uses the standard product shell.

Desktop:

- left navigation rail
- compact page header
- settings section navigation in the page body
- main detail panel
- optional right inspector only for secondary detail

Mobile:

- same product logic
- compact header
- stacked section navigation
- pushed detail sections or bottom sheet for short edits

---

## Information Architecture

Phase 1 Settings should stay narrow.

Primary sections:

- `Profile`
- `Household`
- `Preferences`

Phase 1 Outlook connection management lives in `Profile`.

The structure is:

- Profile
  - identity summary
  - account details
  - calendar connections
- Household
  - household-level preferences when available
- Preferences
  - personal defaults and future settings seams

Settings must not become a dumping ground for unrelated product state.

---

## Default View

Default Settings entry should open `Profile`.

The profile view should show:

- member identity summary
- account identity summary
- calendar connections section

The connection section must remain scannable without opening deep drill-down first.

---

## Calendar Connections Section

### Purpose

The calendar connections section manages member-scoped external calendar connections.

It answers:

- which Outlook accounts are connected for this person
- which provider calendars are included
- whether sync is healthy
- when sync last succeeded
- what corrective action is available

### Header actions

The section header contains:

- title: `Calendar connections`
- primary action: `Connect Outlook`
- secondary action: `Sync calendars` when more than one connection exists

### Empty state

When no connection exists, show:

- concise explanation that imported Outlook items appear read-only in Agenda
- `Connect Outlook` action

Do not over-explain sync mechanics in the empty state.

### Connection rows

Each connection row should show:

- provider cue: `Outlook`
- account label or email
- selected calendar count or calendar names when compact enough
- horizon summary such as `Next 90 days`
- last sync time
- current status
- actions: `Sync now`, `Edit`, `Disconnect`

Status examples:

- Connected
- Syncing
- Needs attention
- Auth expired
- Rehydrating

The row should stay dense and readable.
It is not a large settings card.

### Detail / edit state

Selecting `Edit` opens inline detail on desktop or a pushed detail section on mobile.

The edit state contains:

- account label
- provider email
- calendar checklist
- horizon selector: 30 / 90 / 180 / 365 days
- scheduled refresh toggle
- scheduled refresh interval
- last sync summary
- last error summary when present

`Disconnect` is visible but secondary.

---

## Interaction Rules

### Connect Outlook

`Connect Outlook` launches a focused provider-connect flow.

Expected flow:

1. user starts connect from Settings/Profile
2. provider auth flow completes
3. user returns to Settings
4. new connection appears with default calendar selected or awaiting configuration
5. user may edit included calendars and horizon immediately

### Sync now

`Sync now` is per connection.

Behavior:

- starts manual sync for that connection
- shows in-progress state inline
- updates last sync and status on completion
- surfaces failure inline without leaving Settings

### Sync calendars

`Sync calendars` is the section-level bulk action.

Behavior:

- available when several connections exist
- dispatches one sync per eligible connection
- shows aggregate progress summary at section level
- does not hide per-connection failure state

### Disconnect

Disconnect uses a confirm step.

Confirmation copy must make the outcome explicit:

- imported Outlook items will disappear from Agenda
- native household plans are unaffected

---

## Read Model Expectations

Settings/Profile needs a connection summary model that includes:

- connection identity
- provider label
- account label and email
- selected feed count
- horizon
- refresh interval
- sync status
- last successful sync
- last error summary

Edit state additionally needs:

- available provider calendars
- selected provider calendars
- current window configuration

---

## Cross-Surface Relationship

Settings owns connection setup and maintenance.

Agenda consumes the result.

Rules:

- Agenda may link to Settings when connection setup is missing or auth is stale
- Settings does not become another agenda-like temporal surface
- imported entries are inspected in Agenda, not in Settings

---

## Permissions and Ownership

Phase 1 should keep this simple.

Default rule:

- a member manages their own Outlook connections

If a later household-admin override is introduced, it must be explicit and auditable.
Phase 1 should not normalize one person editing another person's provider credentials.

---

## Mobile Behavior

Mobile Settings keeps the same content in a collapsed form.

Expected behavior:

- section navigation as stacked list or segmented switch
- connection rows remain compact
- edit opens a pushed detail section
- connect flow may hand off to provider auth and return to the same section
- sync feedback appears inline, not as a blocking full-screen flow

---

## Non-Goals

Settings does not handle:

- editing native plans
- browsing imported entries as a calendar surface
- provider write-back
- household-wide admin over all provider credentials in phase 1

---

## Success Criteria

Settings succeeds when:

- a person can connect Outlook without confusion
- connected accounts are understandable at a glance
- sync health and corrective actions are visible
- the section stays dense and low-ceremony
- the user understands that imported Outlook items are read-only in Agenda