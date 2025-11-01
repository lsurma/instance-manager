using InstanceManager.Application.Contracts.DataSet;
using InstanceManager.Application.Core.Data;
using MediatR;

namespace InstanceManager.Application.Core.DataSet.Handlers;

public class DeleteDataSetCommandHandler : IRequestHandler<DeleteDataSetCommand, bool>
{
    private readonly InstanceManagerDbContext _context;

    public DeleteDataSetCommandHandler(InstanceManagerDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteDataSetCommand request, CancellationToken cancellationToken)
    {
        var dataSet = await _context.DataSets.FindAsync([request.Id], cancellationToken);

        if (dataSet == null)
        {
            return false;
        }

        _context.DataSets.Remove(dataSet);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
