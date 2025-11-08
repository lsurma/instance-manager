using InstanceManager.Application.Core.Common;
using InstanceManager.Application.Core.Data;

namespace InstanceManager.Application.Core.Modules.DataSet;

/// <summary>
/// Query service for DataSet entities
/// </summary>
public class DataSetsQueryService : QueryService<DataSet, Guid>
{
    private readonly InstanceManagerDbContext _context;

    public DataSetsQueryService(
        InstanceManagerDbContext context,
        IFilterHandlerRegistry filterHandlerRegistry)
        : base(filterHandlerRegistry)
    {
        _context = context;
    }

    protected override IQueryable<DataSet> DefaultQuery => _context.DataSets;
}
