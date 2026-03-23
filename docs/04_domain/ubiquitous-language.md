# DomusMind — Ubiquitous Language

## Purpose

This document defines the **shared vocabulary of the DomusMind domain**.

All code, documentation, APIs, and discussions about the system must use these terms consistently.

---

# Core Concepts

## Family

A **Family** is the primary organizational unit of the system.

A family represents a group of people managing a shared household and its associated plans, tasks, routines, and household activity.

---

## Member

A **Member** is a person belonging to a family.

Members participate in plans, tasks, and routines.

---

## Area

An **Area** is a lightweight label used to classify household activity.

Examples:

```
house
pets
family
leisure
admin
```

Areas:

* are optional
* are flat
* help recognition and filtering
* do not enforce business rules

Areas are a read-model and UI-facing concept, not a bounded context or aggregate.

---

## Plan (Household Term)

A **Plan** is a household-facing concept representing something scheduled in time that affects the family.

Plans appear in the **Household Timeline** and communicate what the household is doing.

---

## Event (Internal Model)

An **Event** is the internal calendar-domain model representing a scheduled activity at a specific point in time.

Events are **not a user-facing concept**.
In the household experience, users interact with **Plans**, not Events.

---

## Timeline

The **Household Timeline** represents the chronological sequence of things affecting the household.

The timeline aggregates entries originating from multiple contexts, including:

* plans
* tasks
* routines
* reminders

Areas may be attached to timeline entries to improve scanning and filtering.

---

## Task

A **Task** is the household concept representing a concrete action that must be completed.

Tasks appear in the **Household Timeline** and in coordination views (Today board, Week grid).

---

## Routine

A **Routine** represents **recurring operational behavior in the household**.

Routines define **how operational work repeats over time**.

---

## Marker / Weekly Cue

A **Marker** (or **Weekly Cue**) is a **read-model-only concept** used by coordination views.

Markers are **not domain entities** and do not exist as aggregates.
