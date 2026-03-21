# Create Routine

Context: Tasks  
Aggregate: Routine  
Capability: CreateRoutine

## Command

CreateRoutine

Fields:

- RoutineId
- FamilyId
- Name
- Scope (Household | Members)
- Kind (Scheduled | Cue)
- Color (hex)
- Frequency (Daily | Weekly | Monthly | Yearly)
- DaysOfWeek (for Weekly)
- DaysOfMonth (for Monthly, Yearly)
- MonthOfYear (for Yearly)
- Time (optional)
- TargetMemberIds (for Members scope)

Example

{
  "routineId": "ULID",
  "familyId": "ULID",
  "name": "Weekly grocery shopping",
  "scope": "Household",
  "kind": "Scheduled",
  "color": "#3B82F6",
  "frequency": "Weekly",
  "daysOfWeek": [1],
  "time": "10:00"
}

## Rules

- RoutineId must be unique
- Family must exist
- Name must not be empty
- Frequency must be Daily, Weekly, Monthly, or Yearly
- Weekly requires at least one DaysOfWeek value
- Monthly requires at least one DaysOfMonth value
- Yearly requires MonthOfYear and at least one DaysOfMonth value
- Daily requires no day selectors
- TargetMemberIds required when Scope is Members

## State Change

Creates a new Routine aggregate in Active status.

The routine defines a recurrence pattern. It does not generate Task aggregates.
The routine is projected on-the-fly when building timeline and weekly grid read models.

## Events

RoutineCreated

## Success

Routine exists and will appear in timeline projections on matching dates.

## Failure

ValidationError  
FamilyNotFound  
MemberNotFound  
RoutineAlreadyExists
