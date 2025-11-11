using System.Linq.Expressions;
using InstanceManager.Application.Contracts.Modules.Translations;

namespace InstanceManager.Application.Core.Modules.Translations;

/// <summary>
/// Static projection selectors for Translation entities
/// Add a new static method here for each projection type you want to support
/// </summary>
public static class TranslationProjections
{
    /// <summary>
    /// Full TranslationDto projection with all fields
    /// </summary>
    public static Expression<Func<Translation, TranslationDto>> ToTranslationDto()
    {
        return t => new TranslationDto
        {
            Id = t.Id,
            InternalGroupName = t.InternalGroupName,
            ResourceName = t.ResourceName,
            TranslationName = t.TranslationName,
            CultureName = t.CultureName,
            Content = t.Content,
            DataSetId = t.DataSetId,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            CreatedBy = t.CreatedBy
        };
    }

    /// <summary>
    /// Simple TranslationDto projection with only essential fields
    /// </summary>
    public static Expression<Func<Translation, SimpleTranslationDto>> ToSimpleTranslationDto()
    {
        return t => new SimpleTranslationDto
        {
            Id = t.Id,
            TranslationName = t.TranslationName,
            CultureName = t.CultureName,
            Content = t.Content
        };
    }

    /// <summary>
    /// Gets the appropriate selector for the given projection type
    /// Returns the expression as object - you'll need to cast it to the correct type in the handler
    /// </summary>
    public static object? GetSelectorFor(Type projectionType)
    {
        if (projectionType == typeof(TranslationDto))
        {
            return ToTranslationDto();
        }

        if (projectionType == typeof(SimpleTranslationDto))
        {
            return ToSimpleTranslationDto();
        }

        throw new NotSupportedException($"No projection selector defined for type {projectionType.Name}. " +
                                       $"Add a static method to TranslationProjections class.");
    }
}
