namespace InstanceManager.Application.Contracts.Common;

/// <summary>
/// Generic search filter for full-text search across entity properties
/// </summary>
public class SearchFilter : IQueryFilter
{
    public string Name => "Search";

    public string? SearchTerm { get; set; }

    public bool IsActive() => !string.IsNullOrWhiteSpace(SearchTerm);
}
