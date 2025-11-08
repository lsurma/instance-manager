using InstanceManager.Application.Contracts.Modules.DataSet;
using InstanceManager.Application.Core.Data;
using MediatR;

namespace InstanceManager.Application.Core.Modules.DataSet.Handlers;

public class DeleteDataSetCommandHandler : IRequestHandler<DeleteDataSetCommand, bool>
{
    private readonly InstanceManagerDbContext _context;
    private readonly DataSetsQueryService _queryService;

    public DeleteDataSetCommandHandler(InstanceManagerDbContext context, DataSetsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<bool> Handle(DeleteDataSetCommand request, CancellationToken cancellationToken)
    {
        var dataSet = await _queryService.GetByIdAsync(
            request.Id,
            cancellationToken: cancellationToken
        );

        if (dataSet == null)
        {
            return false;
        }

        _context.DataSets.Remove(dataSet);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
