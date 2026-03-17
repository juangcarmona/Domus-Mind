# Calendar Coordination

Families do not manage calendars.

Families coordinate **people and time**.

DomusMind merges household plans into a single shared timeline so the family can easily understand what is happening and who needs to be involved.

The system exists to answer one question:

> Who needs to be where, and when? 

---

# Concept

Traditional calendars organize events.

DomusMind organizes **household coordination**.

Instead of each person maintaining separate calendars, DomusMind maintains a **shared household timeline** where plans involving the family become visible to everyone.

The goal is clarity, not event management.

---

# Sources of Plans

Household plans may originate from different places.

Examples include:

* personal calendars
* school schedules
* manually added household plans
* shared activities

DomusMind unifies these into a single household view.

Users do not need to think about where an event came from.

They only see what affects the household.

---

# Creating a Plan

Creating a plan should be extremely simple.

Minimal information is required.

Example:

```
Mateo football practice
Friday 18:00
Participants: Mateo, Juan
```

Participants are **first-class information**.

The core coordination question of the product is:

> Who needs to be where, and when?

Because of this, participant visibility is essential. The system must clearly show:

* who is involved
* who must be present
* who may be affected

This allows the system to reason about availability and detect coordination conflicts.

---

# Household Perspective

Most calendar systems are **personal**.

DomusMind is **household-first**.

The primary view shows plans affecting the household, not an individual's private schedule.

Example:

```
Today

Lucía dentist — 09:00
Mateo football — 18:00
```

The system answers:

* what the household is doing
* who is involved
* when attention is required

---

# Calendar vs Operational Work

DomusMind distinguishes between **plans in time** and **operational work**.

Recurring **fixed-time activities** belong to the **Calendar context**, not Tasks.

Examples:

```
football practice every Tuesday
piano class every Thursday
school every weekday
```

These remain **calendar plans** because they represent scheduled presence in time.

Operational work belongs to the **Tasks context**.

Examples:

```
take out trash
clean kitchen
grocery shopping
```

These represent actions that must be completed, not time-bound presence.

---

# Conflict Awareness

DomusMind identifies situations where responsibilities and plans overlap.

Example:

```
Juan assigned to Trash
Juan has a meeting at the same time
```

Instead of forcing users to manually resolve conflicts, the system surfaces the issue.

Possible suggestion:

```
Move trash to Saturday?
```

The goal is **quick household decisions**, not manual schedule management.

---

# Weekly Awareness

Families need to understand busy days.

DomusMind highlights overloaded periods.

Example:

```
Wednesday

Juan business trip
Mateo exam
Trash duty
```

The system may suggest adjustments or redistributions when the schedule becomes unrealistic.

The **week view** may also display lightweight coordination cues derived from:

* plans
* routines
* tasks

Examples:

```
trash day
busy evening
school morning
```

These cues are **derived read-model information**, not new domain entities.

They exist only to improve coordination visibility.

They do **not represent aggregates or persisted domain concepts**.

---

# Relationship to the Household Timeline

Calendar coordination is not a separate interface.

Plans simply appear in the **Household Timeline** alongside:

* chores
* tasks
* reminders

This ensures the household sees everything in one place.

---

# Design Goal

The calendar should disappear as a tool.

Instead of managing events, families should experience **clear coordination of time**.

Users should not think:

> I need to check the calendar.

They should feel:

> I know what is happening today.

---

# Core Question

Calendar coordination ultimately answers:

```
Who needs to be where, and when?
```

Not:

```
What events exist?
```
