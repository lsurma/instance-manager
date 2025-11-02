using System.Text.Json.Serialization;

namespace InstanceManager.Application.Contracts.Common;

public record FilteringParameters
{
    public string? SearchTerm { get; set; }
    
    [JsonConverter(typeof(QueryFilterListJsonConverter))]
    public List<IQueryFilter> QueryFilters { get; set; } = new();

    /// <summary>
    /// Checks if text search filtering is specified
    /// </summary>
    public bool HasFilter() => !string.IsNullOrWhiteSpace(SearchTerm);
    
    /// <summary>
    /// Checks if any query filters are active
    /// </summary>
    public bool HasQueryFilters() => QueryFilters.Any(f => f.IsActive());

    /// <summary>
    /// Gets the lowercase search term for case-insensitive comparison
    /// </summary>
    public string GetLowerSearchTerm() => SearchTerm?.ToLower() ?? string.Empty;
}
