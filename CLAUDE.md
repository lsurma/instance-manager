# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

InstanceManager is a .NET 9.0 application for managing project instances with hierarchical relationships. It uses a **CQRS-like architecture** with MediatR, Azure Functions for the API backend, and Blazor WebAssembly for the frontend.

## Architecture

### Project Structure

The solution consists of 4 projects:

- **InstanceManager.Application.Contracts**: MediatR commands, queries, and DTOs shared between all layers
- **InstanceManager.Application.Core**: Business logic, EF Core entities, request handlers, database context, and migrations
- **InstanceManager.Host.AzFuncAPI**: Azure Functions backend API (port 7233)
- **InstanceManager.Host.WA**: Blazor WebAssembly frontend (port 5070)

### Request/Response Flow (CQRS Pattern)

1. Frontend creates a Command/Query (implements `IRequest<TResponse>`) from `.Application.Contracts`
2. `HttpRequestSender` serializes the request and sends GET to `/api/query/{RequestName}?body={urlEncodedJson}`
3. Azure Functions `QueryController` receives the request
4. `RequestRegistry` uses reflection to resolve request type by name (scans all `IRequest<>` types in Contracts assembly)
5. MediatR dispatches to appropriate handler in `.Application.Core`
6. Handler executes business logic using `InstanceManagerDbContext` (EF Core)
7. Response returns as DTOs

**Key Insight:** No explicit endpoint registration is needed. The `RequestRegistry` auto-discovers all request types, enabling dynamic routing. To add a new endpoint, just create a request/query in Contracts and its handler in Core.

### Database

- **SQLite** database at `db/instanceManager.db`
- Connection string: `Data Source=db/instanceManager.db`
- EF Core migrations in `InstanceManager.Application.Core/Data/Migrations`
- Database is **auto-migrated and seeded** on API startup via `InitializeDatabaseAsync()` in `ServiceCollectionExtensions`

### Pagination and Querying System

The application has a sophisticated pagination/filtering system built around these key components:

- **`PaginatedQuery<TResponse>`**: Base class for queries that return paginated results
  - Contains `Pagination`, `Ordering`, and `Filtering` parameters
  - Static `AllItems()` helper sets PageSize to int.MaxValue for non-paginated results

- **`IQueryService`**: Core service for query building
  - `PrepareQuery()`: Applies filters, search, includes, and ordering
  - `ExecutePaginatedQueryAsync()`: Executes query and returns `PaginatedList<TDto>`
  - `ApplyOrdering()`, `ApplyPagination()`, `ApplyTextSearch()`, `ApplySpecification()`

- **`IFilterHandlerRegistry`**: Dynamic filter system
  - Auto-registers all `IFilterHandler<TEntity, TFilter>` implementations via reflection
  - Allows custom filters per entity (e.g., `TranslationFilterApplicator` for culture/dataset filtering)
  - Filters are applied via `FilteringParameters.QueryFilters` using expressions

- **`QueryOptions<TEntity>`**: Configuration for query preparation
  - `SearchPredicate`: Func for full-text search across properties
  - `SearchSpecificationFactory`: Alternative using specification pattern
  - `IncludeFunc`: EF Core Include() chains for eager loading

**Usage Pattern:**
```csharp
// In handler
var query = _context.Translations.AsQueryable();
query = _queryService.PrepareQuery(query, request.Filtering, request.Ordering, options);
return await _queryService.ExecutePaginatedQueryAsync(query, request.Pagination, mapper);
```

See `PAGINATION_IMPLEMENTATION.md` and `PAGINATION_QUICKSTART.md` for detailed examples.

## Common Commands

### Build
```bash
dotnet build InstanceManager.sln
```

### Run Backend (Azure Functions API)
```bash
dotnet run --project InstanceManager.Host.AzFuncAPI/InstanceManager.Host.AzFuncAPI.csproj
```
API runs on `http://localhost:7233`

### Run Frontend (Blazor WebAssembly)
```bash
dotnet run --project InstanceManager.Host.WA/InstanceManager.Host.WA.csproj
```
Frontend runs on `http://localhost:5070` (http) or `https://localhost:7023` (https)

### Database Migrations

**Create new migration:**
```bash
dotnet ef migrations add <MigrationName> --project InstanceManager.Application.Core/InstanceManager.Application.Core.csproj --startup-project InstanceManager.Host.AzFuncAPI/InstanceManager.Host.AzFuncAPI.csproj
```

**Apply migrations manually:**
```bash
dotnet ef database update --project InstanceManager.Application.Core/InstanceManager.Application.Core.csproj --startup-project InstanceManager.Host.AzFuncAPI/InstanceManager.Host.AzFuncAPI.csproj
```

Note: Migrations are automatically applied on API startup via `InitializeDatabaseAsync()`.

## Development Guidelines

### Adding New Features (CQRS Pattern)

**1. Define Contract** in `InstanceManager.Application.Contracts/Modules/{ModuleName}/`:
   - Create Query/Command class inheriting from `PaginatedQuery<TDto>` (for paginated) or implementing `IRequest<TResponse>`
   - Create DTO (prefer `record` over `class` for DTOs to enable `with` expressions)
   - For queries with filtering, define filter classes implementing `IQueryFilter`

**2. Register Filter Handlers** (if using custom filters) in `InstanceManager.Application.Core/Modules/{ModuleName}/Filters/`:
   - Implement `IFilterHandler<TEntity, TFilter>` with `GetFilterExpression()` method
   - Handlers are auto-registered via reflection in `ServiceCollectionExtensions.RegisterFilterHandlers()`

**3. Implement Handler** in `InstanceManager.Application.Core/Modules/{ModuleName}/Handlers/`:
   - Create handler implementing `IRequestHandler<TRequest, TResponse>`
   - Use `IQueryService` for paginated queries (see Pagination section above)
   - Create mapping extension methods in `{Module}MappingExtensions.cs`

**4. Frontend Usage**:
   - Inject `IRequestSender` in Blazor component
   - Call `await _requestSender.SendAsync<TResponse>(request)`

The request will be automatically discoverable via `RequestRegistry` without modifying the API controller.

### Entity Changes

When modifying entities in `InstanceManager.Application.Core/Modules/{ModuleName}/`:
1. Update entity class (inherits from `AuditableEntityBase` or `EntityBase`)
2. Update corresponding configuration in `Data/Configurations/` (fluent API mappings)
3. Create and apply EF Core migration (see Database Migrations section)
4. Update mapping extensions if DTOs changed

### Frontend Development

- Uses **Microsoft Fluent UI** and **Radzen.Blazor** components
- `LocalStorageService` available for browser storage
- API base URL configured in `Program.cs`: `http://localhost:7233/api/`

**Dialogs/Panels Pattern:**
- Use `IDialogService` (injected) to display dialogs and panels
- Panel components implement `IDialogContentComponent<TParameters>` interface
- Use `ShowPanelAsync<T>()` for right-side panels, `ShowDialogAsync<T>()` for centered dialogs
- Set `Modal = false` in `DialogParameters` to allow interaction with page content behind panel
- Track dialog references with `IDialogReference` to close previous dialogs before opening new ones
- `FluentDialogProvider` must be in main layout for dialog service to work

**DataLoader Pattern:**
- Use `<DataLoader>` component for fetching and caching data
- Supports `RefreshToken` parameter to trigger re-fetches
- Supports `CacheKey` parameter for separate cache buckets
- Emits `DataFetchedCallback` with results

### DTOs and Data Transfer

- **Prefer `record` over `class` for DTOs** when copying is needed (enables `with` expressions for non-destructive copying)
- Use `with { }` syntax to create shallow copies when editing to avoid modifying original objects
- Initialize non-nullable string properties with default values (`= string.Empty`) in DTOs

### Module Structure

Each module follows this pattern:
```
InstanceManager.Application.Contracts/Modules/{ModuleName}/
  ├── {Entity}Dto.cs              # Data transfer object (record)
  ├── Get{Entity}ByIdQuery.cs     # Single item retrieval
  ├── Get{Entities}Query.cs       # List/paginated query (inherits PaginatedQuery<TDto>)
  ├── Save{Entity}Command.cs      # Create/update
  ├── Delete{Entity}Command.cs    # Delete
  └── {Entity}Filters.cs          # Custom filter classes (if needed)

InstanceManager.Application.Core/Modules/{ModuleName}/
  ├── {Entity}.cs                 # EF Core entity (inherits AuditableEntityBase)
  ├── {Entity}MappingExtensions.cs # Entity ↔ DTO mappings
  ├── Handlers/
  │   ├── Get{Entity}ByIdQueryHandler.cs
  │   ├── Get{Entities}QueryHandler.cs
  │   ├── Save{Entity}CommandHandler.cs
  │   └── Delete{Entity}CommandHandler.cs
  ├── Specifications/
  │   └── {Entity}SearchSpecification.cs  # Full-text search logic
  └── Filters/
      └── {Entity}FilterApplicator.cs     # Custom filter handlers
```

### Auditable Entities

All entities inherit from `AuditableEntityBase` which provides:
- `Id` (Guid primary key)
- `CreatedAt` (stored as UTC ticks in SQLite as long)
- `UpdatedAt` (stored as UTC ticks in SQLite as long)

Database configuration automatically converts UTC ticks to `DateTimeOffset` via value converters in `AuditableEntityConfiguration`.
