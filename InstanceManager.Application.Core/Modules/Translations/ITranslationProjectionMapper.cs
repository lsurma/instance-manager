using System.Linq.Expressions;
using InstanceManager.Application.Contracts.Modules.Translations;

namespace InstanceManager.Application.Core.Modules.Translations;

/// <summary>
/// Defines a mapper that provides a selector expression for projecting Translation entities to a specific type
/// </summary>
/// <typeparam name="TProjection">The type to project translation data into</typeparam>
public interface ITranslationProjectionMapper<TProjection>
    where TProjection : ITranslationDto
{
    /// <summary>
    /// Gets the expression for projecting a Translation entity to TProjection
    /// This expression will be used in the EF Core query for database-level projection
    /// </summary>
    Expression<Func<Translation, TProjection>> GetSelector();
}
