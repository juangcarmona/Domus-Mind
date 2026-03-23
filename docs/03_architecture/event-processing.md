# DomusMind — Event Processing

DomusMind uses domain events to propagate meaningful changes across modules.

Examples:

- FamilyCreated
- MemberAdded
- EventScheduled
- TaskCompleted
- RoutinePaused

Areas are not event-driven aggregates.
They are lightweight labels used by read models and UI-facing projections.
