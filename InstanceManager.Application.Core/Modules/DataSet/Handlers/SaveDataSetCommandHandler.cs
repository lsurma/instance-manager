using InstanceManager.Application.Contracts.Modules.DataSet;
using InstanceManager.Application.Core.Common;
using InstanceManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InstanceManager.Application.Core.Modules.DataSet.Handlers;

public class SaveDataSetCommandHandler : IRequestHandler<SaveDataSetCommand, Guid>
{
    private readonly InstanceManagerDbContext _context;
    private readonly DataSetsQueryService _queryService;

    public SaveDataSetCommandHandler(InstanceManagerDbContext context, DataSetsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<Guid> Handle(SaveDataSetCommand request, CancellationToken cancellationToken)
    {
        DataSet? dataSet;

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            // Update existing - use QueryService for consistent querying
            var options = new QueryOptions<DataSet, Guid>
            {
                IncludeFunc = q => q.Include(ds => ds.Includes)
            };

            dataSet = await _queryService.GetByIdAsync(
                request.Id.Value,
                options: options,
                cancellationToken: cancellationToken
            );

            if (dataSet == null)
            {
                throw new KeyNotFoundException($"DataSet with Id {request.Id} not found.");
            }

            dataSet.Name = request.Name;
            dataSet.Description = request.Description;
            dataSet.Notes = request.Notes;

            // Update includes
            var existingIncludes = dataSet.Includes.ToList();
            var newIncludeIds = request.IncludedDataSetIds.ToList();

            // Remove includes that are no longer in the list
            foreach (var include in existingIncludes)
            {
                if (!newIncludeIds.Contains(include.IncludedDataSetId))
                {
                    dataSet.Includes.Remove(include);
                }
            }

            // Add new includes
            var existingIncludeIds = existingIncludes.Select(i => i.IncludedDataSetId).ToList();
            foreach (var newIncludeId in newIncludeIds)
            {
                if (!existingIncludeIds.Contains(newIncludeId))
                {
                    dataSet.Includes.Add(new DataSetInclude
                    {
                        ParentDataSetId = dataSet.Id,
                        IncludedDataSetId = newIncludeId,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
                }
            }
        }
        else
        {
            // Create new
            dataSet = new DataSet
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Notes = request.Notes,
                CreatedBy = string.Empty // Will be set by DbContext
            };

            // Add includes
            foreach (var includeId in request.IncludedDataSetIds)
            {
                dataSet.Includes.Add(new DataSetInclude
                {
                    ParentDataSetId = dataSet.Id,
                    IncludedDataSetId = includeId,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            _context.DataSets.Add(dataSet);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return dataSet.Id;
    }
}
