using InstanceManager.Application.Contracts.Common;

namespace InstanceManager.Application.Contracts.Modules.Translations;

/// <summary>
/// Base class for translation filters with automatic naming
/// </summary>
public abstract class TranslationFilterBase<TFilter> : IQueryFilter where TFilter : TranslationFilterBase<TFilter>
{
    public string Name => $"Translation.{typeof(TFilter).Name.Replace("Filter", "")}";
    
    public abstract bool IsActive();
}
