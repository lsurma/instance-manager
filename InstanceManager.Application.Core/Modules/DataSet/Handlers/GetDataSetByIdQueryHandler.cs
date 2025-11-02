using InstanceManager.Application.Contracts.Modules.DataSet;
using InstanceManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Modules.DataSet.Handlers;

public class GetDataSetByIdQueryHandler : IRequestHandler<GetDataSetByIdQuery, DataSetDto?>
{
    private readonly InstanceManagerDbContext _context;

    public GetDataSetByIdQueryHandler(InstanceManagerDbContext context)
    {
        _context = context;
    }

    public async Task<DataSetDto?> Handle(GetDataSetByIdQuery request, CancellationToken cancellationToken)
    {
        var dataSet = await _context.DataSets
            .Include(ds => ds.Includes)
            .FirstOrDefaultAsync(ds => ds.Id == request.Id, cancellationToken);

        return dataSet?.ToDto();
    }
}
