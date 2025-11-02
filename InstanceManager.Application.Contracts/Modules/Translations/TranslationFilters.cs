using InstanceManager.Application.Contracts.Common;

namespace InstanceManager.Application.Contracts.Modules.Translations;

/// <summary>
/// Filter by DataSet ID
/// </summary>
public class DataSetIdFilter : IQueryFilter
{
    public string Name => "DataSetId";
    
    public Guid? Value { get; set; }
    
    public bool IsActive() => Value.HasValue;
}

/// <summary>
/// Filter by Culture Name
/// </summary>
public class CultureNameFilter : IQueryFilter
{
    public string Name => "CultureName";
    
    public string? Value { get; set; }
    
    public bool IsActive() => !string.IsNullOrWhiteSpace(Value);
}
