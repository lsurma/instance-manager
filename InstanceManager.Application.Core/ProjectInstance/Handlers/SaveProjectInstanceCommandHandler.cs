using InstanceManager.Application.Contracts.ProjectInstance;
using InstanceManager.Application.Core.Data;
using MediatR;

namespace InstanceManager.Application.Core.ProjectInstance.Handlers;

public class SaveProjectInstanceCommandHandler : IRequestHandler<SaveProjectInstanceCommand, Guid>
{
    private readonly InstanceManagerDbContext _context;

    public SaveProjectInstanceCommandHandler(InstanceManagerDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(SaveProjectInstanceCommand request, CancellationToken cancellationToken)
    {
        ProjectInstance instance;

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            // Update existing
            instance = await _context.ProjectInstances.FindAsync([request.Id.Value], cancellationToken);
            if (instance == null)
            {
                throw new KeyNotFoundException($"ProjectInstance with Id {request.Id} not found.");
            }

            instance.Name = request.Name;
            instance.Description = request.Description;
            instance.MainHost = request.MainHost;
            instance.Notes = request.Notes;
            instance.ParentProjectId = request.ParentProjectId;
            instance.UpdatedAt = DateTimeOffset.UtcNow;
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
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = request.CreatedBy
            };

            _context.ProjectInstances.Add(instance);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return instance.Id;
    }
}
