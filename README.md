# Todo App

Created by Michael John Rieser

A personal Todo Web API built with **.NET 10**, following **Clean Architecture** and **Domain-Driven Design (DDD)** principles.
The project is intentionally simple, but serves as a clean reference implementation for architecture, error handling,
and separation of concerns.

---

## Architecture Overview

The project follows **Clean Architecture** principles:

- Dependencies always point **inwards**
- The domain is independent of frameworks and infrastructure
- Technical details (Web, Database, EF Core) are replaceable

### Project Structure

```
src/
 ├─ TodoApp.Api             // HTTP API (ASP.NET Core, Swagger)
 ├─ TodoApp.Application     // Use cases, CQRS, ErrorOr, ports
 ├─ TodoApp.Domain          // Domain model (entities, rules)
 └─ TodoApp.Infrastructure  // EF Core, repository implementations
```

### Layer Responsibilities

#### TodoApp.Domain
- Contains the **domain model**
- Entities, invariants, and business rules
- No dependencies on ASP.NET, EF Core, or other frameworks

Examples:
- `TodoItem`
- `DomainException`

---

#### TodoApp.Application
- Implements **use cases** (CQRS with MediatR)
- Orchestrates domain logic
- Defines **ports** (e.g. `ITodoRepository`)
- Uses **ErrorOr** to model business errors

Examples:
- `CreateTodoCommand`
- `GetTodoByIdQuery`
- `TodoDto`
- `TodoErrors`

---

#### TodoApp.Infrastructure
- Technical implementations
- EF Core `DbContext`
- Repository implementations
- No business logic

Examples:
- `TodoDbContext`
- `EfTodoRepository`

---

#### TodoApp.Api
- HTTP API (controllers, routing, Swagger)
- Translates application results into HTTP responses
- Contains **no business logic**

---

## Entity Framework Core Configuration

The Infrastructure layer contains explicit EF Core configurations using
`IEntityTypeConfiguration<T>` to keep persistence concerns out of the
domain model.

### TodoItemConfiguration

The `TodoItem` aggregate is configured via:

```
src/TodoApp.Infrastructure/Persistence/Configurations/TodoItemConfiguration.cs
```

Key aspects:

- Mapping of the `TodoItem` aggregate root
- Mapping of the `TodoTitle` value object using a **ValueConverter**
- Domain validation remains fully inside the domain model
- The database stores `TodoTitle` as a simple string column

This approach ensures:

- No EF Core attributes in the domain layer
- Full encapsulation of domain invariants
- Clean separation between domain and persistence concerns

All configurations are registered automatically in the `DbContext`
using:

``` csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodoDbContext).Assembly);
```

---

## Error Handling

The project uses **ErrorOr** for business errors and **exceptions only for unexpected or technical failures**.

### Core Principles

- **Business errors** → `ErrorOr`
- **Unexpected errors** → global exception handling (HTTP 500)

### ErrorOr in the Application Layer

Use cases return:

```csharp
ErrorOr<TodoDto>
```

instead of throwing exceptions for expected cases.

Examples:
- Todo not found → `Error.NotFound`
- Business validation error → `Error.Validation`

Central error definitions:

```csharp
TodoErrors.NotFound(id)
TodoErrors.Domain(message)
```

---

### ErrorOr Usage Rules

The following rules define when **ErrorOr** is used within the application.

#### General Rule

> **Use `ErrorOr` when a use case can fail for business reasons.**  
> **Do not use `ErrorOr` for simple list or read-only queries where an empty result is valid.**

#### Practical Guidelines

| Use Case Type       | Example                    | Return Type        |
|--------------------|----------------------------|--------------------|
| Command (write)     | Create / Update / Delete   | `ErrorOr<T>` |
| Targeted query      | Get by Id                  | `ErrorOr<T>` |
| List / Search       | ListTodos                  | `List<T>` |
| Technical failure   | DB down, bug               | Exception (HTTP 500) |

#### Rationale

- An **empty list is valid data**, not an error.
- Business decisions such as *Not Found*, *Validation*, or *Conflict* are explicitly modeled using `ErrorOr`.
- Exceptions are reserved for **unexpected or technical failures**.

---

## MediatR Pipeline Behaviors

The application uses **MediatR Pipeline Behaviors** to implement cross-cutting concerns
that apply to all use cases.

Pipeline behaviors act like **middleware for commands and queries** and are executed
*before and/or after* the actual request handler.

### Validation Behavior

- FluentValidation is integrated via a custom `ValidationBehavior`
- Validation runs **before** the handler is executed
- Validation failures are converted into `ErrorOr.Validation` results
- The handler is **not executed** when validation fails

This ensures:
- No validation logic in controllers
- No duplicated validation code in handlers
- Consistent error responses across all use cases

---

## Object Mapping

The project uses **Mapster** to map domain entities to DTOs.

### Rationale

- Avoids repetitive manual mapping code in handlers
- Keeps handlers focused on orchestration and business flow
- Centralizes mapping configuration in the Application layer
- Supports efficient query projections for list endpoints

### Mapping Strategy

- **Domain → DTO mapping** is configured centrally using Mapster
- Handlers use `IMapper` to map entities to DTOs
- Manual mapping via `.Select(...)` is avoided

Example:

```csharp
return _mapper.Map<TodoDto>(todo);
```

For list queries:

```csharp
return _mapper.Map<List<TodoDto>>(todos);
```

This approach keeps the Application layer clean, consistent, and easy to maintain.

---

### Mapping in the API

The API translates `ErrorOr` results centrally into HTTP responses:

| ErrorOr Type   | HTTP Status |
|---------------|-------------|
| Validation    | 400         |
| NotFound      | 404         |
| Conflict      | 409         |
| Unauthorized  | 401         |
| Forbidden     | 403         |
| Unexpected    | 500         |

Example in a controller:

```csharp
var result = await _mediator.Send(command);

return result.Match(
    value => Ok(value),
    errors => this.ProblemFromErrors(errors));
```

This ensures:
- No `try/catch` blocks in controllers
- Consistent `ProblemDetails` responses

---

## Global Exception Handling (HTTP 500)

Unexpected errors (e.g. database unavailable, bugs) are handled by a global exception handler,
which always returns a standardized **ProblemDetails** response with status **500**.

Business errors never rely on exceptions.

---

## Database & Migrations

### Prerequisites

Install `dotnet-ef` if not already available:

```bash
dotnet tool install --global dotnet-ef
```

### Create Migrations and Update the Database

Run the following commands from the project root:

```bash
dotnet ef migrations add InitialCreate --project src/TodoApp.Infrastructure --startup-project src/TodoApp.Api

dotnet ef database update --project src/TodoApp.Infrastructure --startup-project src/TodoApp.Api
```

- The `DbContext` is located in **TodoApp.Infrastructure**
- The API is used as the **startup project**
- SQLite is used as the database

---

## API Usage

After starting the API in development mode, open Swagger:

```
https://localhost:{port}/swagger
```

Example endpoints:
- `POST /api/todos`
- `GET /api/todos`
- `GET /api/todos/{id}`

---

## Unit Tests

Automated tests are kept outside the `src/` directory and are structured according to **Clean Architecture** principles.

### Test Structure

```
tests/
 ├─ TodoApp.Domain.Tests        // Unit tests for the domain model
 └─ TodoApp.Application.Tests   // Unit tests for application use cases (commands & queries)
```

---

## Domain Tests

Domain tests verify **pure domain logic only**:

- Value Objects (e.g. `TodoTitle`)
- Entities and invariants
- Domain behavior and state changes
- Domain exceptions

Characteristics:

- ❌ No mocks
- ❌ No dependencies on Application or Infrastructure
- ❌ No ASP.NET Core or EF Core references
- ✅ Only domain objects under test

This keeps domain rules **isolated**, **fast**, and **framework-independent**.

---

## Application Layer Tests

Application tests verify **use case orchestration**, not infrastructure.

They ensure that a command or query:

- Creates or loads domain objects correctly
- Calls the expected repositories
- Persists changes
- Maps domain objects to DTOs
- Returns the correct result or error

### Key Principles

- Dependencies (repositories, mappers, services) are **mocked**
- Domain logic itself is **not mocked**
- Tests focus on **behavior and interactions**
- Each test represents one application use case

### Mocking Strategy

**Moq** is used to mock Application Layer dependencies:

- Repositories (e.g. `ITodoRepository`)
- Mappers (e.g. `IMapper`)
- External services (if any)

Mocks are configured using **`MockBehavior.Strict`**:

- Every interaction must be explicitly defined
- Unexpected calls cause the test to fail
- This protects against accidental changes in orchestration logic

This makes tests more explicit and safer during refactoring.

---

## Test Libraries

### xUnit v3

- Modern .NET test framework
- Successor to xUnit v2 (deprecated)
- Integrated with `dotnet test`

### FluentAssertions

- Improves readability and expressiveness of assertions
- Produces clear, human-readable failure messages
- Well suited for DDD-style tests

### Moq

- Mocking framework for .NET
- Used to isolate the Application Layer
- Enables verification of interactions with dependencies

---

## Design Decisions (Summary)

- **Clean Architecture** for clear separation of concerns
- **CQRS + MediatR** for use case orchestration
- **ErrorOr instead of exceptions** for business errors
- **FluentValidation via MediatR Pipeline Behavior**
- **Mapster for object mapping**
- **EF Core only in Infrastructure**
- **Swagger** for API documentation

---

## Status

🚧 In development

Next steps:
- Check if ownerId validation is correctly implemented
- Change all use cases so ownerId is used
- Create tests for Users and RefreshTokens
- Polish logging and `ProblemDetails` handling, including explicit Swagger response types
- Introduce paging and filtering for list endpoints, including efficient EF Core projections
- Switch to PostgreSQL (local development via Docker Compose)
- Define deployment to Azure, including a clear migrations strategy
- Add health checks and basic monitoring
- Address concurrency concerns and add integration tests using Testcontainers