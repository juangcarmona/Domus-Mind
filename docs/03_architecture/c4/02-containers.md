# C4 — Containers

DomusMind uses a modular monolith with:

- API container
- application/domain code
- infrastructure/persistence
- supporting web clients

Family, Calendar, and Tasks remain the core bounded contexts.
Areas stay inside read models and supporting UI flows.
