# DomusMind — Domain Events

## Purpose

Domain events represent **facts that have occurred in the household system**.

They enable:

- decoupled bounded contexts
- extensible behavior
- automation
- integrations
- auditability
- AI interpretation

Events are part of the **core domain language**.

---

# Event Principles

## Events Represent Facts

Events describe something that **has already happened**.

Examples:

```

FamilyCreated
MemberAdded
EventScheduled
PetAdded
PropertyRegistered

```

Events are **immutable**.

---

## Past Tense Naming

Events must use **past tense**.

Correct:

```

EventScheduled
TaskCompleted
MealPlanGenerated

```

Incorrect:

```

ScheduleEvent
CreateTask
GenerateMealPlan

```

Commands trigger behavior.  
Events describe outcomes.

---

## Events Belong to the Domain

Event names must reflect the **ubiquitous language**.

They must be understandable without technical knowledge.

---

## Events Are Emitted After State Change

An event is published **only after the domain state has successfully changed**.

Events must never be speculative.

---

# Event Ownership

Each domain event has a **single owning bounded context**.

Rules:

- Only the owning context may emit the event.
- Other contexts may subscribe.
- Other contexts must never publish the same event.

Example:

```

FamilyCreated

```

Owned by:

```

Family Context

```

---

# Event Structure

Each event contains:

```

EventId
EventType
OccurredAt
AggregateId
Payload

```

Example:

```json
{
  "eventId": "evt_98fa3b",
  "eventType": "MemberAdded",
  "occurredAt": "2026-01-01T10:00:00Z",
  "aggregateId": "family_123",
  "payload": {
    "memberId": "member_456",
    "name": "Lucas",
    "role": "Child"
  }
}
```

---

# Event Categories

## Family Events

Household structure changes.

```
FamilyCreated
MemberAdded
MemberRemoved
PetAdded
PetRemoved
RelationshipAssigned
```

---

## Responsibility Events

Ownership of household domains.

```
ResponsibilityDomainCreated
ResponsibilityAssigned
ResponsibilityTransferred
SecondaryOwnerAssigned
```

---

## Calendar Events

Timeline operations.

```
EventScheduled
EventRescheduled
EventCancelled
ReminderCreated
ReminderTriggered
```

---

## Task Events

Operational actions.

```
TaskCreated
TaskAssigned
TaskReassigned
TaskCompleted
TaskCancelled
TaskRescheduled
RoutineCreated
RoutineUpdated
RoutinePaused
RoutineResumed
```

---

## Property Events

Property lifecycle changes.

```
PropertyRegistered
PropertyExpenseRecorded
PropertyIncomeRecorded
MaintenanceScheduled
MaintenanceCompleted
```

---

## Administration Events

Documents and contracts.

```
DocumentStored
DocumentExpirationApproaching
ContractRegistered
ContractRenewalApproaching
InsurancePolicyAdded
```

---

## Inventory Events

Household resource state.

```
InventoryItemAdded
InventoryItemUpdated
InventoryItemDepleted
ShoppingListGenerated
```

---

## Food Events

Food planning operations.

```
RecipeAdded
MealPlanned
MealPlanCreated
ShoppingListGeneratedFromMealPlan
```

---

## Pet Events

Pet care activities.

```
PetAdded
VetAppointmentScheduled
VaccinationDue
MedicationReminderCreated
```

---

## Finance Events

Financial operations.

```
ExpenseRecorded
IncomeRecorded
RecurringExpenseCreated
SubscriptionRenewalApproaching
```

---

# Event Consumption

Events may be consumed by:

* other bounded contexts
* automation services
* notification systems
* integration adapters
* AI processing pipelines

Example:

```
EventScheduled
```

Possible reactions:

* reminder generation
* preparation tasks
* responsibility notifications

---

# Event Storage

Events may optionally be stored for:

* audit trails
* historical analysis
* automation
* AI learning

Possible implementations:

* event logging
* event streaming
* event sourcing (future option)

Full event sourcing is **not required initially**.

---

# Event Versioning

Events must support evolution.

Possible strategies:

* version field
* backward compatible payloads
* transformation layers

Example:

```
EventScheduled v1
EventScheduled v2
```

Consumers must tolerate older versions.

---

# Event Processing Model

Typical flow:

```
Command
   ↓
Domain Logic
   ↓
State Change
   ↓
Domain Event Emitted
   ↓
Event Bus
   ↓
Subscribers
```

Asynchronous processing is preferred when possible.

---

# Event Stability

Domain events form part of the **long-term system contract**.

Avoid changing event semantics.

Instead:

* introduce new events
* version existing events

---

# Example Event Flow

Scheduling a school excursion:

```
EventScheduled
TaskGeneratedFromEvent
ReminderCreated
```

Subscribers may:

* notify responsible members
* generate preparation tasks
* suggest packing lists

```