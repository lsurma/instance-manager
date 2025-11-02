namespace InstanceManager.Application.Contracts.Common;

public record FilteringParameters
{
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Checks if filtering is specified
    /// </summary>
    public bool HasFilter() => !string.IsNullOrWhiteSpace(SearchTerm);

    /// <summary>
    /// Gets the lowercase search term for case-insensitive comparison
    /// </summary>
    public string GetLowerSearchTerm() => SearchTerm?.ToLower() ?? string.Empty;
}
