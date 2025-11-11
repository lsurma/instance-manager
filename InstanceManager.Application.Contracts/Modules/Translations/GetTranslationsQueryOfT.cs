using InstanceManager.Application.Contracts.Common;

namespace InstanceManager.Application.Contracts.Modules.Translations;

/// <summary>
/// Generic query for retrieving translations with custom projection
/// </summary>
/// <typeparam name="TProjection">The type to project translation data into</typeparam>
public class GetTranslationsQuery<TProjection> : PaginatedQuery<TProjection>
    where TProjection : ITranslationDto
{
    /// <summary>
    /// Creates a query that returns all items without pagination
    /// </summary>
    public static GetTranslationsQuery<TProjection> AllItems(string? orderBy = null, string? orderDirection = null)
    {
        return AllItems<GetTranslationsQuery<TProjection>>(orderBy, orderDirection);
    }
}
