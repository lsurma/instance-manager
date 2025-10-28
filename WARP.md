# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

InstanceManager is a .NET 9.0 solution for managing project instances with hierarchical relationships. It uses a **CQRS-like architecture** with MediatR, Azure Functions for the API backend, and Blazor WebAssembly for the frontend.

## Architecture

### Project Structure

- **InstanceManager.Application.Contracts**: Defines MediatR commands, queries, and DTOs shared between all layers
- **InstanceManager.Application.Core**: Business logic, EF Core entities, handlers, database context, and migrations
- **InstanceManager.Host.AzFuncAPI**: Azure Functions backend API (runs on port 7233)
- **InstanceManager.Host.WA**: Blazor WebAssembly frontend (runs on port 5070)

### Key Architectural Patterns

**Request/Response Flow:**
1. Frontend creates a Command/Query from `.Application.Contracts`
2. `HttpRequestSender` serializes and sends to Azure Functions endpoint `/api/query/{RequestName}`
3. `QueryController` uses `RequestRegistry` to dynamically resolve request type by name
4. MediatR dispatches to appropriate handler in `.Application.Core`
5. Handler executes business logic using `InstanceManagerDbContext`
6. Response returns through the stack as DTOs

**Request Registry Pattern:**
The API uses reflection to automatically discover all `IRequest<>` types from the Contracts assembly, allowing dynamic request routing without explicit endpoint registration per request type.

### Database

- SQLite database located at `db/instanceManager.db`
- Connection string: `Data Source=db/instanceManager.db`
- Entity Framework Core with migrations in `InstanceManager.Application.Core/Data/Migrations`
- Database is auto-migrated and seeded on application startup via `InitializeDatabaseAsync()`

## Common Commands

### Build
```powershell
dotnet build InstanceManager.sln
```

### Run Azure Functions API (Backend)
```powershell
dotnet run --project InstanceManager.Host.AzFuncAPI\InstanceManager.Host.AzFuncAPI.csproj
```
API runs on `http://localhost:7233`

### Run Blazor WebAssembly (Frontend)
```powershell
dotnet run --project InstanceManager.Host.WA\InstanceManager.Host.WA.csproj
```
Frontend runs on `http://localhost:5070` (http) or `https://localhost:7023` (https)

### Run Tests
No test projects currently exist in the solution.

### Database Migrations

**Create new migration:**
```powershell
dotnet ef migrations add <MigrationName> --project InstanceManager.Application.Core\InstanceManager.Application.Core.csproj --startup-project InstanceManager.Host.AzFuncAPI\InstanceManager.Host.AzFuncAPI.csproj
```

**Apply migrations manually:**
```powershell
dotnet ef database update --project InstanceManager.Application.Core\InstanceManager.Application.Core.csproj --startup-project InstanceManager.Host.AzFuncAPI\InstanceManager.Host.AzFuncAPI.csproj
```

**Note:** Migrations are automatically applied on API startup via `InitializeDatabaseAsync()` in `ServiceCollectionExtensions`.

## Development Guidelines

### Adding New Features (CQRS Pattern)

1. **Define Contract** in `InstanceManager.Application.Contracts`:
   - Create Command/Query class implementing `IRequest<TResponse>`
   - Create DTO if needed

2. **Implement Handler** in `InstanceManager.Application.Core`:
   - Create handler implementing `IRequestHandler<TRequest, TResponse>`
   - Place in appropriate feature folder (e.g., `ProjectInstance/Handlers/`)

3. **Frontend Usage**:
   - Inject `IRequestSender` in Blazor component
   - Call `SendAsync<TResponse>(request)` with your command/query

The request will be automatically discoverable via `RequestRegistry` without modifying the API controller.

### Entity Changes

When modifying entities in `InstanceManager.Application.Core`:
1. Update entity class
2. Update corresponding configuration in `Data/Configurations/` if needed
3. Create and apply EF Core migration (see Database Migrations section)

### Frontend Development

- Uses Microsoft Fluent UI and Radzen.Blazor components
- `LocalStorageService` available for browser storage
- API base URL configured in `Program.cs`: `http://localhost:7233/api/`
- **Dialogs/Panels**: 
  - Use `IDialogService` (injected) to display dialogs and panels
  - Panel components should implement `IDialogContentComponent<TParameters>` interface
  - Use `ShowPanelAsync<T>()` for right-side panels, `ShowDialogAsync<T>()` for centered dialogs
  - Set `Modal = false` in `DialogParameters` to allow interaction with page content behind the panel
  - Track dialog references with `IDialogReference` to close previous dialogs before opening new ones
  - `FluentDialogProvider` must be added to the main layout for dialog service to work

### DTOs and Data Transfer

- **Prefer records over classes for DTOs** when copying is needed (enables `with` expressions for non-destructive copying)
- Use `with { }` syntax to create shallow copies of record DTOs when editing to avoid modifying original objects
- Initialize non-nullable string properties with default values (`= string.Empty`) in DTOs
