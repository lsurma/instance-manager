namespace InstanceManager.Application.Contracts.Modules.Translations;

/// <summary>
/// Filter by DataSet ID
/// </summary>
public class DataSetIdFilter : TranslationFilterBase<DataSetIdFilter>
{
    public Guid? Value { get; set; }
    
    public override bool IsActive() => Value.HasValue;
}

/// <summary>
/// Filter by Culture Name
/// </summary>
public class CultureNameFilter : TranslationFilterBase<CultureNameFilter>
{
    public string? Value { get; set; }
    
    public override bool IsActive() => !string.IsNullOrWhiteSpace(Value);
}
