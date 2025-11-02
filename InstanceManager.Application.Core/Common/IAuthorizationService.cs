namespace InstanceManager.Application.Core.Common;

/// <summary>
/// Service for handling authorization and access control
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Gets the list of DataSet IDs that the current user has access to
    /// </summary>
    Task<List<Guid>> GetAccessibleDataSetIdsAsync(CancellationToken cancellationToken = default);
}
