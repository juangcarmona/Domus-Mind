# DomusMind — Slice Catalog (Updated)

## Purpose

This document lists the **V1 slices of DomusMind**.

Detailed behavior is defined in `specs/features/...`.

Slices represent **user-visible system capabilities** implemented as vertical slices.

---

# Family

Household identity and membership.

* create-family
* identify-self
* add-member
* add-initial-members
* assign-relationship
* view-family
* view-family-members
* update-household-settings

---

# Responsibilities

Household ownership structure.

* create-responsibility-domain
* assign-primary-owner
* assign-secondary-owner
* transfer-responsibility
* suggest-owner
* rebalance-responsibilities
* detect-overload
* reassign-responsibility
* view-responsibility-visibility

---

# Calendar

Planning and coordination.

* schedule-event
* reschedule-event
* cancel-event
* add-event-participant
* remove-event-participant
* add-reminder
* remove-reminder
* detect-calendar-conflict
* suggest-participant
* propose-new-time
* view-family-timeline
* view-family-plans

---

# Tasks

Execution and routines.

* create-task
* assign-task
* complete-task
* cancel-task
* reschedule-task
* reassign-task
* create-routine
* update-routine
* pause-routine
* resume-routine

---

# Coordination Views (Read Models)

Household coordination surfaces.

* view-family-timeline-enriched
* view-weekly-household-grid
* view-member-activity
* view-responsibility-balance
* view-responsibility-overload

---

# Account

User self-management.

* view-account
* update-display-name
* change-password
* logout-session

---

# System

Operational and metadata capabilities.

* view-supported-languages
* system-health-check

