# DomusMind Household Experience

This document defines the intended experience of using DomusMind inside a household.

It describes **how the product should feel to use**, not how it is implemented.

DomusMind is not "family management software".

DomusMind is **household coordination infrastructure**.

The system exists to eliminate the invisible coordination work families perform every day. 

---

# Core UX Principles

DomusMind follows four fundamental principles.

---

## 1 — Reduce Cognitive Load

Household coordination generates constant mental overhead:

* remembering events
* reminding others
* negotiating chores
* tracking responsibilities
* anticipating obligations

DomusMind removes this burden.

The system should quietly maintain the operational state of the household.

---

## 2 — Action First

Users should always see **what matters today**.

Not dashboards.
Not configuration screens.

The system should immediately answer:

> What does this household need today?

This is why the **Household Timeline** is the primary interface.

---

## 3 — No Configuration Barrier

A family should go from installation to useful operation in **under three minutes**.

Early interaction must avoid:

* configuration flows
* system terminology
* complex setup

The system should become helpful immediately.

---

## 4 — Household Language

Users interact with the system using natural household concepts.

Internal domain terminology must remain invisible.

| Internal Model | Household Language |
| -------------- | ------------------ |
| Family         | Household          |
| Member         | Person             |
| Task           | Chore              |
| Event          | Plan               |
| Responsibility | Area               |

The system must always speak **household language**.

---

# The Home Screen

DomusMind is not a dashboard.

The home screen represents the **current state of the household**.

Example:

```
Today

Trash → Juan
Laundry → Gema
Buy milk

Mateo football practice — 18:00
```

This screen answers one question:

> What matters today for this household?

---

# Weekly Coordination

The **Week view is a primary operational surface**, not a secondary calendar page.

Families coordinate life at the **weekly scale**: school schedules, sports practices, recurring chores, and busy days become visible only when viewed together.

The week view provides a **coordination grid** that helps the household understand:

* where people need to be
* when commitments occur
* what operational work exists around those commitments
* which days are overloaded

This view may combine information from multiple contexts:

* plans (calendar events)
* chores (tasks)
* routine-generated work

The goal is rapid situational awareness of the household’s upcoming week.

---

# Smart Household Whiteboard

In weekly coordination mode, DomusMind should feel like a **smart household whiteboard**.

Not a calendar application.

Not a productivity tool.

A shared board where the household can instantly see:

* the week’s plans
* who is responsible for what
* which days are busy
* what needs preparation

The experience should resemble a **living coordination board for the household**.

---

# Long-Term UX Direction

DomusMind should gradually evolve toward **household autopilot**.

Examples:

```
Trash was missed yesterday.
Remind Juan today?
```

```
You usually buy milk every five days.
Add it to groceries?
```

```
Mateo has exams next week.
Reduce chores temporarily?
```

Over time the system should anticipate household needs and reduce coordination work automatically.

---

# Product Philosophy

DomusMind should feel like a **smart household whiteboard**, not a management system.

The system should be:

* quiet
* helpful
* predictable
* lightweight

Users should think:

> "The house runs smoother."

Not:

> "We need to maintain DomusMind."
