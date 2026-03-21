# DomusMind — Slice Catalog

## Purpose

This document lists all slices of DomusMind, grouped by context and delivery phase.

Detailed behavior is defined in `specs/features/...`.

Slices represent **user-visible system capabilities** implemented as vertical slices.

Phase legend:
- `V1` — shipped or in active delivery
- `V1.1` — deferred from V1; no new bounded contexts; hardening and completeness
- `V2+` — future expansion; may introduce new contexts

---

# Family

Household identity and membership.

| Slice | Phase |
|-------|-------|
| create-family | V1 |
| identify-self | V1 |
| add-member | V1 |
| add-initial-members | V1 |
| view-family | V1 |
| view-family-members | V1 |
| update-household-settings | V1 |
| assign-relationship | V1.1 |
| remove-member | V1.1 |

---

# Responsibilities

Household ownership structure.

| Slice | Phase |
|-------|-------|
| create-responsibility-domain | V1 |
| assign-primary-owner | V1 |
| assign-secondary-owner | V1 |
| transfer-responsibility | V1 |
| view-responsibility-balance | V1.1 |
| detect-overload | V1.1 |
| view-responsibility-visibility | V1.1 |
| suggest-owner | V1.1 |
| rebalance-responsibilities | V2+ |
| reassign-responsibility | V2+ |

---

# Calendar

Planning and coordination.

| Slice | Phase |
|-------|-------|
| schedule-event | V1 |
| reschedule-event | V1 |
| cancel-event | V1 |
| add-event-participant | V1 |
| remove-event-participant | V1 |
| add-reminder | V1 |
| remove-reminder | V1 |
| view-family-timeline | V1 |
| view-family-plans | V1 |
| detect-calendar-conflict | V1.1 |
| suggest-participant | V1.1 |
| propose-new-time | V1.1 |

---

# Tasks

Execution and routines.

| Slice | Phase |
|-------|-------|
| create-task | V1 |
| assign-task | V1 |
| reassign-task | V1 |
| complete-task | V1 |
| cancel-task | V1 |
| reschedule-task | V1 |
| create-routine | V1 |
| update-routine | V1 |
| pause-routine | V1 |
| resume-routine | V1 |

---

# Coordination Views (Read Models)

Household coordination surfaces.

| Slice | Phase |
|-------|-------|
| view-family-timeline-enriched | V1 |
| view-weekly-household-grid | V1 |
| view-member-activity | V1.1 |
| view-responsibility-balance | V1.1 |
| view-responsibility-overload | V1.1 |

---

# Account

User self-management.

| Slice | Phase |
|-------|-------|
| view-account | V1 |
| update-display-name | V1 |
| change-password | V1 |
| logout-session | V1 |

---

# System

Operational and metadata capabilities.

| Slice | Phase |
|-------|-------|
| view-supported-languages | V1 |
| system-health-check | V1 |

