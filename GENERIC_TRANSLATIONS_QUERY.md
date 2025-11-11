# Generic Translations Query

This document explains how to use the generic translations query pattern to retrieve translations with custom projections.

## Overview

The generic query pattern allows you to retrieve translations with any custom projection type, reducing data transfer and improving performance by selecting only the fields you need.

## Architecture

The generic query system consists of:

1. **`ITranslationDto`** - Marker interface in `Application.Contracts` that all translation projection DTOs must implement
2. **`GetTranslationsQuery<TProjection> where TProjection : ITranslationDto`** - Generic query in `Application.Contracts`
3. **`TranslationProjections`** - Static class in `Application.Core` containing selector expressions for each projection type
4. **`GetTranslationsQueryHandler<TProjection> where TProjection : ITranslationDto`** - Generic handler in `Application.Core`

The `ITranslationDto` constraint ensures that MediatR only registers handlers for valid translation projections, preventing unnecessary type registrations.

### Why Static Methods Instead of Dynamic Expression Building?

The key requirement is that EF Core must be able to translate your projection expressions to SQL. When you try to build expressions dynamically at runtime using `Expression.Convert`, `Expression.Invoke`, or `Expression.Constant`, you create expression trees that EF Core cannot translate.

**This FAILS** (EF Core cannot translate):
```csharp
var expr = (Translation t) => new TranslationDto { ... };
var cast = Expression.Convert(
    Expression.Invoke(Expression.Constant(expr), ...),
    typeof(TProjection)
);
var exprT = Expression.Lambda<Func<Translation, TProjection>>(cast, ...);
```

**This WORKS** (EF Core can translate):
```csharp
// Define selector as a static method that returns the expression directly
public static Expression<Func<Translation, TranslationDto>> ToTranslationDto()
{
    return t => new TranslationDto { ... };
}

// Use it via cast (no dynamic expression building)
var selector = (Expression<Func<Translation, TProjection>>)TranslationProjections.GetSelectorFor(typeof(TProjection));
```

The static method approach works because the actual expression (`t => new TranslationDto { ... }`) is created at compile time and is a proper expression tree that EF Core understands. The cast simply reinterprets the type at the C# level without modifying the expression tree.

## How It Works

1. MediatR automatically registers the generic handler `GetTranslationsQueryHandler<TProjection>` (because `RegisterGenericHandlers = true`)
2. When you send a query with a specific projection type (e.g., `GetTranslationsQuery<SimpleTranslationDto>`), MediatR creates a concrete instance of the handler for that type
3. The handler calls `TranslationProjections.GetSelectorFor(typeof(TProjection))` to get the appropriate selector expression
4. The selector expression is used by EF Core for database-level projection to SQL

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

### Step 2: Add a Static Selector Method (in TranslationProjections.cs)

```csharp
// InstanceManager.Application.Core/Modules/Translations/TranslationProjections.cs
public static class TranslationProjections
{
    // ... existing methods ...

    /// <summary>
    /// My custom projection
    /// </summary>
    public static Expression<Func<Translation, MyCustomTranslationDto>> ToMyCustomTranslationDto()
    {
        return t => new MyCustomTranslationDto
        {
            Id = t.Id,
            TranslationName = t.TranslationName,
            Content = t.Content
        };
    }

    public static object? GetSelectorFor(Type projectionType)
    {
        if (projectionType == typeof(TranslationDto))
        {
            return ToTranslationDto();
        }

        if (projectionType == typeof(SimpleTranslationDto))
        {
            return ToSimpleTranslationDto();
        }

        // ADD YOUR NEW PROJECTION HERE
        if (projectionType == typeof(MyCustomTranslationDto))
        {
            return ToMyCustomTranslationDto();
        }

        throw new NotSupportedException($"No projection selector defined for type {projectionType.Name}. " +
                                       $"Add a static method to TranslationProjections class.");
    }
}
```

That's it! No registration needed - just add the static method and the case in `GetSelectorFor`.

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

## Example Projections

The solution includes two example projections in `TranslationProjections.cs`:

1. **`ToTranslationDto()`** - Full projection with all fields
2. **`ToSimpleTranslationDto()`** - Lightweight projection with only 4 essential fields (Id, TranslationName, CultureName, Content)

Files:
- **DTOs**: `InstanceManager.Application.Contracts/Modules/Translations/TranslationDto.cs` and `SimpleTranslationDto.cs`
- **Selectors**: `InstanceManager.Application.Core/Modules/Translations/TranslationProjections.cs`

This demonstrates how to create projections with different field sets.

## Benefits

1. **Performance** - Only select the fields you need, reducing data transfer
2. **Database-level projection** - The selector expression is translated to SQL by EF Core
3. **Type-safe** - Full compile-time type checking with generic constraints
4. **Simple** - Just add a static method and a case in `GetSelectorFor` - no interfaces or complex registration
5. **Reusable** - Define projections once, use them anywhere
6. **Supports all query features** - Filtering, ordering, pagination, and authorization all work automatically
7. **Constrained generics** - The `ITranslationDto` constraint prevents MediatR from attempting to register handlers for every possible type, improving startup performance
8. **EF Core compatible** - Uses compile-time expressions that EF Core can translate, avoiding runtime expression building issues

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
- **Static Selectors**: `TranslationProjections` class contains static methods that return compile-time expression trees
- **Type Resolution**: Handler uses `GetSelectorFor(Type)` to get the appropriate selector based on `TProjection`
- **Query Service**: Uses the existing `TranslationsQueryService` for authorization, filtering, and ordering
- **EF Core**: The selector expression is a proper compile-time expression tree that EF Core can translate to SQL
- **Generic Constraint**: The `where TProjection : ITranslationDto` constraint ensures only valid translation projections are used

### How Generic Types are Serialized Over HTTP

The system handles generic types through a custom serialization format:

1. **Frontend (HttpRequestSender)**:
   - Detects if the request type is generic (e.g., `GetTranslationsQuery<SimpleTranslationDto>`)
   - Formats it as: `GetTranslationsQuery<SimpleTranslationDto>`
   - Sends this as the URL parameter: `/api/query/GetTranslationsQuery<SimpleTranslationDto>?body={...}`

2. **Backend (RequestRegistry)**:
   - Parses the request name format: `TypeName<GenericArg>`
   - Looks up the open generic type definition: `GetTranslationsQuery<>`
   - Resolves the generic argument type from the Contracts assembly: `SimpleTranslationDto`
   - Constructs the closed generic type: `GetTranslationsQuery<SimpleTranslationDto>`
   - Returns this type to the QueryController for deserialization

This approach allows the dynamic request routing system to work seamlessly with generic types without requiring explicit endpoint registration.

## Troubleshooting

### "The type 'X' cannot be used as type parameter 'TProjection'"

This error means your DTO doesn't implement `ITranslationDto`. Add it to your record:

```csharp
public record MyDto : ITranslationDto { ... }
```

### "No handler found for GetTranslationsQuery<MyDto>"

Ensure you have:
1. Added a static method to `TranslationProjections` that returns `Expression<Func<Translation, MyDto>>`
2. Added the corresponding case in `GetSelectorFor(Type)` method
3. MediatR configuration has `RegisterGenericHandlers = true`

### "No projection selector defined for type MyDto"

You forgot to add the case in `GetSelectorFor(Type)`. Add:
```csharp
if (projectionType == typeof(MyDto))
{
    return ToMyDto();
}
```

## Creating Projections for Other Entities

This pattern can be extended to other entities:

1. Create a generic interface `IEntityProjectionMapper<TEntity, TProjection>`
2. Create a generic handler similar to `GetTranslationsQueryHandler<TProjection>`
3. Update the registration logic to handle multiple entity types

The current implementation is specific to translations but serves as a template for other entities.
