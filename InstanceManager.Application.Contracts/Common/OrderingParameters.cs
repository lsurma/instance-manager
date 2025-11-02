namespace InstanceManager.Application.Contracts.Common;

public record OrderingParameters
{
    public string? OrderBy { get; set; }
    
    public string? OrderDirection { get; set; }

    /// <summary>
    /// Gets the ordering direction in Dynamic LINQ format (ascending/descending)
    /// </summary>
    public string GetOrderDirection()
    {
        return string.IsNullOrEmpty(OrderDirection) || OrderDirection.ToLower() == "asc" 
            ? "ascending" 
            : "descending";
    }

    /// <summary>
    /// Checks if ordering is specified
    /// </summary>
    public bool HasOrdering() => !string.IsNullOrEmpty(OrderBy);
}
