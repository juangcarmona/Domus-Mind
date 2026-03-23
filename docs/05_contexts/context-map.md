# DomusMind — Context Map

This document describes how bounded contexts collaborate inside DomusMind.

Core contexts in V1:

* Family
* Calendar
* Tasks

Areas are a lightweight supporting concept used by read models. They are not a bounded context.

---

# Context Relationships

Family is the upstream identity provider.

Calendar defines time structure using Family participants.
Tasks defines operational work referencing members and events.
Areas improve recognition and filtering in timeline-oriented views.

---

# Dependency Graph

The dependency structure is **not a linear chain**.

Calendar and Tasks both depend on Family.
Areas are applied inside read models and do not introduce new cross-context dependencies.

```
        Family
        /   \
       ↓     ↓
   Calendar  Tasks
```

Dependency interpretation:

* **Family** provides identity and relationship structure
* **Calendar** depends on Family for participant identity
* **Tasks** depends on Family for assignees and may react to Calendar events
* **Areas** classify entries without owning workflow or domain rules

---

## Collaboration Model

Contexts collaborate using **domain events**.

No context may directly modify another context's aggregates.

Communication rules:

* identity flows from Family
* time flows from Calendar
* execution happens in Tasks
* areas remain lightweight labels in projections and UI-facing views

Contexts react to events rather than forming direct structural dependencies.

---

# Summary

The DomusMind core model is built around three cooperating contexts:

* **Family** → identity
* **Calendar** → time
* **Tasks** → execution

Areas support readability across household views without becoming their own module.
