using InstanceManager.Application.Contracts.Modules.DataSet;
using InstanceManager.Application.Core.Common;
using InstanceManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Modules.DataSet.Handlers;

public class GetDataSetByIdQueryHandler : IRequestHandler<GetDataSetByIdQuery, DataSetDto?>
{
    private readonly InstanceManagerDbContext _context;
    private readonly DataSetsQueryService _queryService;

    public GetDataSetByIdQueryHandler(InstanceManagerDbContext context, DataSetsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<DataSetDto?> Handle(GetDataSetByIdQuery request, CancellationToken cancellationToken)
    {
        var options = new QueryOptions<DataSet, Guid>
        {
            AsNoTracking = true,
            IncludeFunc = q => q.Include(ds => ds.Includes)
        };

        var dataSet = await _queryService.GetByIdAsync(
            request.Id,
            options: options,
            cancellationToken: cancellationToken
        );

        return dataSet?.ToDto();
    }
}
