using InstanceManager.Application.Core.Common;

namespace InstanceManager.Application.Core.Modules.ProjectInstance;

/// <summary>
/// Query service for ProjectInstance entities
/// </summary>
public class ProjectInstancesQueryService : QueryService<ProjectInstance, Guid>
{
    public ProjectInstancesQueryService(IFilterHandlerRegistry filterHandlerRegistry)
        : base(filterHandlerRegistry)
    {
    }
}
