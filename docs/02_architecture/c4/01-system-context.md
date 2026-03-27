# DomusMind - C4 System Context

## Purpose

This document defines the external system context of DomusMind.

DomusMind is the central system that models and operates household life as structured domain state.

---

## Scope

DomusMind is responsible for:

- family structure
- responsibility ownership
- event scheduling
- task and routine execution

These capabilities form the V1 core of the system.

---

## Primary Actors

### "Family Member"

Uses DomusMind to:

- manage household structure
- see responsibilities
- schedule events
- execute tasks

### "Household Administrator"

A family member with elevated operational control.

Uses DomusMind to:

- configure the household
- manage ownership rules
- maintain overall household structure

---

## External Systems

### "Messaging Platform"

Optional interface for quick capture and notifications.

Example:

- Telegram

### "Calendar Providers"

Optional external calendar systems used for import/export or synchronization.

Examples:

- Google Calendar
- Apple Calendar
- Outlook Calendar

### "Notification Services"

Infrastructure used to deliver reminders and notifications.

Examples:

- push notifications
- email
- messaging adapters

---

## System Context Diagram

```mermaid
C4Context
title "DomusMind" - System Context

Person(member, "Family Member", "Uses DomusMind")
Person(admin, "Household Administrator", "Configures and manages the household")

System(domusmind, "DomusMind", "Household operating system")

System_Ext(msg, "Messaging Platform", "Telegram or similar messaging platform")
System_Ext(cal, "Calendar Providers", "External calendar import/export or sync")
System_Ext(notify, "Notification Services", "Push, email, or messaging delivery")

Rel(member, domusmind, "Uses")
Rel(admin, domusmind, "Administers")

Rel(domusmind, msg, "Sends/receives messages")
Rel(domusmind, cal, "Imports/exports calendar data")
Rel(domusmind, notify, "Delivers notifications")
```

---

## Notes

DomusMind is the system of record for household operational state.

Authentication is implemented internally in V1.

External systems may provide transport, synchronization, or delivery capabilities, but they do not own the DomusMind domain model.
