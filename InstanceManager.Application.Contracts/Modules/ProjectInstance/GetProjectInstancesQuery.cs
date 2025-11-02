using InstanceManager.Application.Contracts.Common;

namespace InstanceManager.Application.Contracts.Modules.ProjectInstance;

public class GetProjectInstancesQuery : PaginatedQuery<ProjectInstanceDto>
{
    /// <summary>
    /// Creates a query that returns all items without pagination
    /// </summary>
    public static GetProjectInstancesQuery AllItems(string? orderBy = null, string? orderDirection = null)
    {
        return AllItems<GetProjectInstancesQuery>(orderBy, orderDirection);
    }
}
