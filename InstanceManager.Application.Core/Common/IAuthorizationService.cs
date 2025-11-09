namespace InstanceManager.Application.Core.Common;

/// <summary>
/// Service for handling authorization and access control
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Checks if the current user has root/admin access to all resources
    /// </summary>
    Task<bool> HasRootAccessAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the current user has access to a specific DataSet
    /// </summary>
    /// <param name="dataSetId">The ID of the DataSet to check access for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user has access, false otherwise</returns>
    Task<bool> CanAccessDataSetAsync(Guid dataSetId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of DataSet IDs that the current user has access to.
    /// Returns a tuple where:
    /// - AllAccessible: true if user has access to ALL datasets (no filtering needed), false otherwise
    /// - AccessibleIds: list of accessible dataset IDs (empty if AllAccessible is true)
    /// </summary>
    Task<(bool AllAccessible, List<Guid> AccessibleIds)> GetAccessibleDataSetIdsAsync(CancellationToken cancellationToken = default);
}
