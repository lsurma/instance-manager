namespace InstanceManager.Application.Contracts.Common;

/// <summary>
/// Base interface for individual query filters. Each filter is a separate class.
/// Filter definitions live in Contracts, but filter handlers live in Core project.
/// Must have a parameterless constructor for registry discovery.
/// </summary>
public interface IQueryFilter
{
    /// <summary>
    /// Human-readable name used for serialization/deserialization.
    /// Should be a constant value for the filter type.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Indicates whether this filter has a value set and should be applied
    /// </summary>
    bool IsActive();
}
