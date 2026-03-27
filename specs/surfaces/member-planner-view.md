## Feature Spec 2 - Member Planner View (Personal Temporal Surface)

### Purpose

Provide a **time-based planning and inspection view for a single member**, supporting daily execution and short-term planning.

---

### Entry Points

* tap member card (deep interaction)
* dedicated navigation (future)

---

### Surface

Full-screen page focused on one member.

Header:

* member identity
* date navigation
* view switch: Day / Week / Month

---

### Core Modes

#### 1. Day View (default)

Hourly vertical timeline.

Displays:

* events (time-bound, fixed position)
* tasks (placed at due time or top section if no time)
* routines (projected at default or assigned time)

---

### Day View Structure

```
00:00
01:00
...
08:00   ● Event
        □ Task
...
19:30   ● Event
```

Top section (before timeline):

* overdue tasks
* tasks without time

---

### Interaction

* tap slot → create item at time
* tap item → open detail
* long press (future) → reschedule

---

### Conflict Visibility

Detect and display:

* overlapping events
* task assigned during event
* task assigned during unavailability

Visual:

* stacked items
* subtle warning indicator

---

### 2. Week View

Grid:

* columns → days
* rows → time or compact rows

Displays:

* events
* routines
* tasks (lightweight markers or blocks)

Purpose:

* load visualization
* planning awareness

No editing complexity required in V1.

---

### 3. Month View

Calendar grid.

Displays:

* density indicators per day
* selected day → updates Day view

---

### Unavailability

Represented as:

* full-width time block (Day view)
* background overlay (Week view)

Modeled as event type.

---

### Task Behavior

* tasks may have:

  * due date only → appears in top section
  * due time → placed in timeline

Completed tasks:

* remain visible in Day view (collapsed or faded)

---

### Navigation

* swipe or arrows → change day
* switching views preserves selected date

---

### Non-Goals

* no multi-member editing
* no complex drag & drop (initially)
* no AI suggestions

---

### Relationship with Today Panel

* Today panel = household snapshot
* Member planner = individual deep view

Both share:

* same underlying data
* same ordering logic

---

### Success Criteria

* user can plan a day in < 10 seconds
* conflicts are visible without effort
* switching between day/week/month is fluid
* no cognitive translation required between views
