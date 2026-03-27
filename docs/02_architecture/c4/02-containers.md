# DomusMind - C4 Container Diagram

## Purpose

This document defines the main runtime containers of DomusMind.

DomusMind is built as a domain-centric, API-first modular monolith with vertical slices, command/query dispatching, domain events, and an append-only event log.

Authentication is implemented **internally** as part of the DomusMind backend (ADR-002).

---

## Containers

### "Mobile App"

Primary user interface for household members.  
Uses the HTTP API.

### "Web App"

Administrative and desktop-oriented interface.  
Uses the HTTP API.

### "Messaging Adapter"

Optional interface for capture and notifications.  
Bridges platforms such as Telegram to the API.

### "DomusMind API"

Single system boundary for external clients.

Responsibilities:

- HTTP API surface
- authentication
- authorization
- command/query dispatching

### "Application"

Executes commands and queries through vertical slices and internal dispatchers.

### "Domain"

Contains aggregates, entities, value objects, and domain events.

Core V1 modules:

- "Family"
- "Responsibilities"
- "Calendar"
- "Tasks"

### "Infrastructure"

Provides persistence, event storage, integrations, and technical services.

### "Primary Database"

Stores operational state for aggregates and read models.

### "Event Log"

Append-only store for committed domain events, used for auditability, projections, and retries.

---

## Diagram

```mermaid
C4Container
title "DomusMind" - Container Diagram

Person(member, "Family Member", "Uses DomusMind")
Person(admin, "Household Administrator", "Configures and manages the household")

System_Ext(msg, "Messaging Platform", "Telegram or similar messaging platform")

System_Boundary(domusmind, "DomusMind") {

    Container(mobile, "Mobile App", "Client App", "Mobile interface")
    Container(web, "Web App", "Client App", "Web interface")
    Container(adapter, "Messaging Adapter", ".NET Adapter", "Messaging bridge")

    Container(api, "DomusMind API", "ASP.NET Core", "HTTP API and authentication boundary")
    Container(app, "Application", ".NET", "Vertical slices, command/query dispatchers, handlers")
    Container(domain, "Domain", ".NET", "Aggregates, entities, value objects, domain events")
    Container(infra, "Infrastructure", ".NET", "Persistence, event log, integrations")

    ContainerDb(db, "Primary Database", "PostgreSQL", "Operational state and read models")
    ContainerDb(eventlog, "Event Log", "PostgreSQL table or separate store", "Append-only domain event log")
}

Rel(member, mobile, "Uses")
Rel(admin, web, "Uses")
Rel(member, web, "Uses")

Rel(member, adapter, "Uses")
Rel(adapter, msg, "Bridges messages")

Rel(mobile, api, "Calls", "HTTPS/JSON")
Rel(web, api, "Calls", "HTTPS/JSON")
Rel(adapter, api, "Calls", "HTTPS/JSON")

Rel(api, app, "Dispatches commands and queries")
Rel(app, domain, "Executes domain behavior")
Rel(app, infra, "Uses persistence and technical services")

Rel(infra, db, "Reads/Writes")
Rel(infra, eventlog, "Appends committed events")
Rel(domain, eventlog, "Produces domain events", "via application/infrastructure")
```

---

## Notes

Keep the container view small.

DomusMind V1 is deployed as a **modular monolith**, not a distributed system.

Authentication is implemented **inside the main backend**, not via an external identity provider.


