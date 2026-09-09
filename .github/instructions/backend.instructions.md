---
applyTo: "src/backend/**/*.cs"
---

# Backend Instructions

## Architecture

Follow the existing layered structure:
- Domain: business rules only
- Application: commands, queries, handlers
- Contracts: request/response DTOs
- Infrastructure: persistence, auth, integrations
- API: controllers

Do not move logic across layers.

## Rules

- controllers are thin (no business logic)
- one handler = one responsibility
- do not introduce generic repositories
- do not introduce broad service/helper classes
- do not expose domain entities through API contracts

## Size and structure

- file target: <300 lines
- class target: focused, single purpose
- method target: small and readable

Split code when it mixes responsibilities.

## Changes

- prefer modifying existing slices over creating new ones
- follow existing naming and folder conventions
- add or update tests when behavior changes

## Testing

- use xUnit for tests
- use NSubstitute for mocking
- mock only external dependencies (repositories, services, infrastructure)
- do not mock domain logic or value objects
- prefer testing real handlers and domain behavior over over-mocking
- keep tests small, focused, and readable
- name tests by behavior, not implementation, e.g. `Handle_WhenCondition_ShouldResult`
- add or update tests when behavior changes
- use AAA (Arrange-Act-Assert) structure in tests

## Completion gate

Never report a backend task as done without:
1. Running `dotnet build` and confirming it succeeds (0 errors)
2. Running `dotnet test` and confirming all tests pass
3. If existing tests fail due to the change, fix them before proceeding
4. If new behavior was added, add a test for it

## Vertical Slice Architecture

Our backend follows a strict vertical slice pattern where each feature is self-contained in its own folder structure:

### Folder Structure
```
src/backend/DomusMind.Application/Features/<FeatureName>/
├── <FeatureName>Command.cs
├── <FeatureName>CommandHandler.cs
└── <FeatureName>Validator.cs (optional)
```

### Key Principles
1. **Commands in Application Layer** - Commands reference DTOs from Contracts project
2. **Handlers in Application Layer** - Business logic implemented here, no repositories
3. **No Repositories** - Direct persistence via Infrastructure layer
4. **Contracts Project** - Contains all request/response DTOs
5. **Infrastructure Layer** - Contains EF Core configurations and persistence logic

### Example Structure
```
src/backend/DomusMind.Application/Features/MealPlanning/
├── CreateMealPlan/
│   ├── CreateMealPlanCommand.cs
│   ├── CreateMealPlanCommandHandler.cs
│   └── CreateMealPlanValidator.cs
├── GetMealPlans/
│   ├── GetMealPlansQuery.cs
│   └── GetMealPlansQueryHandler.cs
└── ...
```

### Layer Responsibilities
- **Domain Layer**: Business rules only, no persistence
- **Application Layer**: Commands, queries, handlers, validators
- **Contracts Layer**: Request/response DTOs
- **Infrastructure Layer**: EF Core, persistence, repositories (if needed)
- **API Layer**: Controllers, HTTP mapping, authentication

## Migration Generation

When implementing new features with database changes, generate migrations using:

```bash
dotnet ef migrations add <MigrationName> --project src\backend\DomusMind.Infrastructure --startup-project src\backend\DomusMind.Api --output-dir Persistence\Migrations
```

Example for meal planning:
```bash
dotnet ef migrations add AddMealPlanningTables --project src\backend\DomusMind.Infrastructure --startup-project src\backend\DomusMind.Api --output-dir Persistence\Migrations
```

This ensures proper connection string resolution and migration placement in the correct project structure.
