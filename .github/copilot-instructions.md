# DomusMind - Copilot Instructions

This repository is documentation-driven.

Before making non-trivial changes:
- read the relevant files in `docs/` and `specs/`
- identify the owning area (backend, web app, public site)
- find an existing pattern and follow it

## Working rules

- prefer extending existing code over introducing new abstractions
- keep changes small and localized
- keep files focused and small (target <300 lines)
- keep methods/components small and single-purpose
- avoid multi-responsibility classes or components
- avoid generic “service”, “manager”, “helper” patterns unless already established
- avoid introducing new architectural layers

## Structure awareness

Respect current structure and conventions:
- backend is layered (Domain / Application / Contracts / Infrastructure / API)
- web app uses feature-based structure and shared state
- docs and specs are the source of truth for behavior and terminology

Do not reorganize structure unless explicitly required.

## Backend constraints

- controllers must remain thin
- business logic belongs in domain/application, not controllers
- do not expose domain entities through API contracts
- do not introduce generic repositories or mapping frameworks unless already used

## Web app constraints

- avoid large components (split when mixing data, state, and rendering)
- reuse existing API/state patterns
- avoid duplicating forms or logic across features

## Docs and specs

- do not invent new terminology if existing terms exist in docs/specs
- if behavior changes, update the corresponding docs/specs
- do not create parallel or conflicting explanations

## Validation

Before reporting any backend task as done:
- build the affected project(s) - `dotnet build` must succeed
- run `dotnet test` and confirm all tests pass
- fix any failing tests before declaring completion
- do NOT say a task is finished if tests have not been run and confirmed passing

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

For Aspire environments, ensure the database connection is available before running the command.

For frontend tasks:
- `npm run build` must succeed before calling the task done
- run relevant tests if they exist