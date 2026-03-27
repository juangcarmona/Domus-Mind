## Feature Spec 1 - Today Panel (Household Operational View)

### Purpose

Provide a **dense, truthful, zero-navigation view of the current day** for the entire household.

---

### Surface

Main Home screen → Today panel (top section)

---

### Core Principles

* no hidden work
* no duplication
* no sections, only ordering
* collapsed by default, expandable per member
* timeline truth (nothing disappears)

---

### Data Scope

For each member:

* overdue tasks
* tasks due today
* events today (time-bound)
* routines occurring today
* tasks completed today

Global:

* shared / unassigned items (Household row)
* unscheduled tasks (separate entry point, not mixed)

---

### Visual Grammar

* `!` overdue task
* `□` task
* `● HH:mm` event
* `⟳` routine
* `✓` completed

No labels. No legends.

---

### Collapsed State (default)

Each member card shows:

* max 2 items
* ordered by priority
* remaining items summarized as `+N`

#### Ordering (strict)

1. overdue
2. tasks today
3. events
4. routines

Completed tasks are excluded from collapsed unless they are the only items.

#### Examples (structure, not content)

```
MemberName
! □ Task A · ● 19:30 Event B · +2
```

```
MemberName
⟳ Routine A
```

```
MemberName
Nothing today
```

---

### Expanded State (on tap)

Card grows vertically in place.

Shows full ordered list:

1. overdue
2. tasks today
3. events
4. routines
5. completed (crossed, low emphasis)

#### Rules

* only one card expanded at a time
* expansion does not navigate away
* collapse restores uniform height

---

### Household Row

Separate row below members.

Contains:

* shared tasks
* unassigned tasks
* shared events

Must not include personal items.

---

### Unscheduled Tasks

Not shown in main list.

Single compact entry at bottom:

```
No date (N)
```

Tap opens separate view.

---

### Empty State

Per member:

* `Nothing today` → no items for today
* must still reflect:

  * overdue count (if exists)
  * unscheduled count (optional, compact)

---

### Interaction Model

* tap member card → expand/collapse
* tap item → open detail modal/page
* tap `+N` → expand
* tap “No date” → navigate to unscheduled list

---

### Non-Goals

* no hourly layout
* no drag & drop
* no multi-selection
* no inline editing

---

### Success Criteria

* user understands the day in < 3 seconds
* no item is hidden or lost
* no navigation required to act or inspect
