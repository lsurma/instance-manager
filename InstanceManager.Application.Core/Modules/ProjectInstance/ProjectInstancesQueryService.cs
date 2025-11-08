using InstanceManager.Application.Core.Common;
using InstanceManager.Application.Core.Data;

namespace InstanceManager.Application.Core.Modules.ProjectInstance;

/// <summary>
/// Query service for ProjectInstance entities
/// </summary>
public class ProjectInstancesQueryService : QueryService<ProjectInstance, Guid>
{
    private readonly InstanceManagerDbContext _context;

    public ProjectInstancesQueryService(
        InstanceManagerDbContext context,
        IFilterHandlerRegistry filterHandlerRegistry)
        : base(filterHandlerRegistry)
    {
        _context = context;
    }

    protected override IQueryable<ProjectInstance> DefaultQuery => _context.ProjectInstances;
}
