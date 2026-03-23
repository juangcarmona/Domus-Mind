# DomusMind — Slice Conventions

Slices are vertical implementations of user-visible capabilities.

A slice should contain only what it needs:

- request/command or query
- validation
- handler
- explicit mapping

Examples:

- Create Family
- Schedule Event
- Create Task
- Get Household Areas

Read-model slices may compose data from multiple modules when they do not introduce new aggregate boundaries.
