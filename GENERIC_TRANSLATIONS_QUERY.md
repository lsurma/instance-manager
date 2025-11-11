# Generic Translations Query

This document explains how to use the generic translations query pattern to retrieve translations with custom projections.

## Overview

The generic query pattern allows you to retrieve translations with any custom projection type, reducing data transfer and improving performance by selecting only the fields you need.

## Architecture

The generic query system consists of:

1. **`ITranslationDto`** - Marker interface in `Application.Contracts` that all translation projection DTOs must implement
2. **`GetTranslationsQuery<TProjection> where TProjection : ITranslationDto`** - Generic query in `Application.Contracts`
3. **`ITranslationProjectionMapper<TProjection> where TProjection : ITranslationDto`** - Mapper interface in `Application.Core`
4. **`GetTranslationsQueryHandler<TProjection> where TProjection : ITranslationDto`** - Generic handler in `Application.Core`
5. **Auto-registration** - Projection mappers are automatically discovered and registered via reflection

The `ITranslationDto` constraint ensures that MediatR only registers handlers for valid translation projections, preventing unnecessary type registrations.

## How It Works

1. MediatR automatically registers the generic handler `GetTranslationsQueryHandler<TProjection>`
2. When you create a query with a specific projection type (e.g., `GetTranslationsQuery<SimpleTranslationDto>`), MediatR creates a concrete instance of the handler for that type
3. The handler injects the corresponding `ITranslationProjectionMapper<TProjection>` mapper
4. The mapper provides a `Selector` expression that EF Core uses for database-level projection

## Creating a Custom Projection

### Step 1: Create a Projection DTO (in Application.Contracts)

**IMPORTANT:** Your DTO must implement `ITranslationDto` to be compatible with the generic query.

```csharp
// InstanceManager.Application.Contracts/Modules/Translations/MyCustomTranslationDto.cs
namespace InstanceManager.Application.Contracts.Modules.Translations;

public record MyCustomTranslationDto : ITranslationDto
{
    public Guid Id { get; set; }
    public string TranslationName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    // Add only the fields you need!
}
```

### Step 2: Create a Projection Mapper (in Application.Core)

```csharp
// InstanceManager.Application.Core/Modules/Translations/Mappers/MyCustomTranslationMapper.cs
using System.Linq.Expressions;
using InstanceManager.Application.Contracts.Modules.Translations;

namespace InstanceManager.Application.Core.Modules.Translations.Mappers;

public class MyCustomTranslationMapper : ITranslationProjectionMapper<MyCustomTranslationDto>
{
    public Expression<Func<Translation, MyCustomTranslationDto>> GetSelector()
    {
        return t => new MyCustomTranslationDto
        {
            Id = t.Id,
            TranslationName = t.TranslationName,
            Content = t.Content
        };
    }
}
```

That's it! The mapper will be automatically discovered and registered.

### Step 3: Use the Query

#### From Blazor Frontend:

```csharp
@inject IRequestSender RequestSender

@code {
    private async Task LoadSimpleTranslations()
    {
        var query = new GetTranslationsQuery<SimpleTranslationDto>
        {
            Pagination = new PaginationParameters { Page = 1, PageSize = 20 },
            Ordering = new OrderingParameters { OrderBy = "TranslationName" },
            Filtering = new FilteringParameters()
        };

        var result = await RequestSender.SendAsync(query);
        // result is PaginatedList<SimpleTranslationDto>
    }

    // Or use the AllItems helper for non-paginated results
    private async Task LoadAllSimpleTranslations()
    {
        var query = GetTranslationsQuery<SimpleTranslationDto>.AllItems("TranslationName", "asc");
        var result = await RequestSender.SendAsync(query);
        // result.Items contains all translations as SimpleTranslationDto
    }
}
```

#### From Backend (e.g., another handler):

```csharp
public class MyHandler
{
    private readonly IMediator _mediator;

    public async Task DoSomething()
    {
        var query = new GetTranslationsQuery<SimpleTranslationDto>();
        var result = await _mediator.Send(query);
        // result is PaginatedList<SimpleTranslationDto>
    }
}
```

## Example Projection: SimpleTranslationDto

The solution includes an example projection `SimpleTranslationDto` with its mapper:

- **DTO**: `InstanceManager.Application.Contracts/Modules/Translations/SimpleTranslationDto.cs`
- **Mapper**: `InstanceManager.Application.Core/Modules/Translations/Mappers/SimpleTranslationMapper.cs`

This demonstrates how to create a lightweight projection with only essential fields.

## Benefits

1. **Performance** - Only select the fields you need, reducing data transfer
2. **Database-level projection** - The selector expression is translated to SQL by EF Core
3. **Type-safe** - Full compile-time type checking with generic constraints
4. **Automatic registration** - No manual service registration needed
5. **Reusable** - Define projections once, use them anywhere
6. **Supports all query features** - Filtering, ordering, pagination, and authorization all work automatically
7. **Constrained generics** - The `ITranslationDto` constraint prevents MediatR from attempting to register handlers for every possible type, improving startup performance and clarity

## Advanced Usage

### With Filtering

```csharp
var query = new GetTranslationsQuery<SimpleTranslationDto>
{
    Filtering = new FilteringParameters
    {
        QueryFilters = new List<IQueryFilter>
        {
            new TranslationCultureFilter { CultureName = "en-US" }
        }
    }
};
```

### With Custom Ordering

```csharp
var query = new GetTranslationsQuery<SimpleTranslationDto>
{
    Ordering = new OrderingParameters
    {
        OrderBy = "TranslationName",
        OrderDirection = "desc"
    }
};
```

### Non-Paginated Results

```csharp
// Returns all items without pagination
var query = GetTranslationsQuery<SimpleTranslationDto>.AllItems("TranslationName", "asc");
var result = await RequestSender.SendAsync(query);
```

## Implementation Details

- **MediatR**: Automatically registers `GetTranslationsQueryHandler<TProjection>` as an open generic (requires `RegisterGenericHandlers = true` in MediatR configuration)
- **Service Registration**: `RegisterProjectionMappers()` in `ServiceCollectionExtensions.cs` automatically discovers and registers all `ITranslationProjectionMapper<>` implementations
- **Query Service**: Uses the existing `TranslationsQueryService` for authorization, filtering, and ordering
- **EF Core**: The selector expression is translated to SQL for optimal performance
- **Generic Constraint**: The `where TProjection : ITranslationDto` constraint on the query, handler, and mapper interface ensures only valid translation projections are used

## Troubleshooting

### "The type 'X' cannot be used as type parameter 'TProjection'"

This error means your DTO doesn't implement `ITranslationDto`. Add it to your record:

```csharp
public record MyDto : ITranslationDto { ... }
```

### "No handler found for GetTranslationsQuery<MyDto>"

Ensure you have:
1. Created a mapper implementing `ITranslationProjectionMapper<MyDto>`
2. The mapper is in the `InstanceManager.Application.Core` assembly (so reflection can find it)
3. MediatR configuration has `RegisterGenericHandlers = true`

## Creating Projections for Other Entities

This pattern can be extended to other entities:

1. Create a generic interface `IEntityProjectionMapper<TEntity, TProjection>`
2. Create a generic handler similar to `GetTranslationsQueryHandler<TProjection>`
3. Update the registration logic to handle multiple entity types

The current implementation is specific to translations but serves as a template for other entities.
