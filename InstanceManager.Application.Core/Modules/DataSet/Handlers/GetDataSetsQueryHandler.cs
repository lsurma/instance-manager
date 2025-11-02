using InstanceManager.Application.Contracts.Common;
using InstanceManager.Application.Contracts.Modules.DataSet;
using InstanceManager.Application.Core.Common;
using InstanceManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Modules.DataSet.Handlers;

public class GetDataSetsQueryHandler : IRequestHandler<GetDataSetsQuery, PaginatedList<DataSetDto>>
{
    private readonly InstanceManagerDbContext _context;
    private readonly IQueryService<DataSet> _queryService;

    public GetDataSetsQueryHandler(InstanceManagerDbContext context, IQueryService<DataSet> queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<PaginatedList<DataSetDto>> Handle(GetDataSetsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DataSets.AsNoTracking();

        query = await _queryService.PrepareQueryAsync(
            query,
            request.Filtering,
            request.Ordering,
            new QueryOptions<DataSet>
            {
                IncludeFunc = q => q.Include(ds => ds.Includes)
            },
            cancellationToken);

        return await _queryService.ExecutePaginatedQueryAsync(
            query,
            request.Pagination,
            dataSets => dataSets.ToDto(),
            cancellationToken);
    }
}
