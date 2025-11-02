using System.Text.Json.Serialization;

namespace InstanceManager.Application.Contracts.Common;

public record FilteringParameters
{
    [JsonConverter(typeof(QueryFilterListJsonConverter))]
    public List<IQueryFilter> QueryFilters { get; set; } = new();

    /// <summary>
    /// Checks if any query filters are active
    /// </summary>
    public bool HasQueryFilters() => QueryFilters.Any(f => f.IsActive());
}
