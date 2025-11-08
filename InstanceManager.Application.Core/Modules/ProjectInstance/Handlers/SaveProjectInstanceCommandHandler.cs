using InstanceManager.Application.Contracts.Modules.ProjectInstance;
using InstanceManager.Application.Core.Data;
using MediatR;

namespace InstanceManager.Application.Core.Modules.ProjectInstance.Handlers;

public class SaveProjectInstanceCommandHandler : IRequestHandler<SaveProjectInstanceCommand, Guid>
{
    private readonly InstanceManagerDbContext _context;
    private readonly ProjectInstancesQueryService _queryService;

    public SaveProjectInstanceCommandHandler(InstanceManagerDbContext context, ProjectInstancesQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<Guid> Handle(SaveProjectInstanceCommand request, CancellationToken cancellationToken)
    {
        ProjectInstance? instance;

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            // Update existing - use QueryService for consistent querying
            instance = await _queryService.GetByIdAsync(
                request.Id.Value,
                cancellationToken: cancellationToken
            );

            if (instance == null)
            {
                throw new KeyNotFoundException($"ProjectInstance with Id {request.Id} not found.");
            }

            instance.Name = request.Name;
            instance.Description = request.Description;
            instance.MainHost = request.MainHost;
            instance.Notes = request.Notes;
            instance.ParentProjectId = request.ParentProjectId;
        }
        else
        {
            // Create new
            instance = new ProjectInstance
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                MainHost = request.MainHost,
                Notes = request.Notes,
                ParentProjectId = request.ParentProjectId,
                CreatedBy = string.Empty // Will be set by DbContext
            };

            _context.ProjectInstances.Add(instance);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return instance.Id;
    }
}
