using InstanceManager.Application.Contracts.Modules.ProjectInstance;

namespace InstanceManager.Application.Core.Modules.ProjectInstance;

public static class ProjectInstanceMappingExtensions
{
    public static ProjectInstanceDto ToDto(this ProjectInstance instance)
    {
        return new ProjectInstanceDto
        {
            Id = instance.Id,
            Name = instance.Name,
            Description = instance.Description,
            MainHost = instance.MainHost,
            Notes = instance.Notes,
            ParentProjectId = instance.ParentProjectId,
            CreatedAt = instance.CreatedAt,
            UpdatedAt = instance.UpdatedAt,
            CreatedBy = instance.CreatedBy
        };
    }

    public static List<ProjectInstanceDto> ToDto(this List<ProjectInstance> instances)
    {
        var dtoMap = new Dictionary<Guid, ProjectInstanceDto>();

        // First pass: create all DTOs
        foreach (var instance in instances)
        {
            dtoMap[instance.Id] = instance.ToDto();
        }

        // Map dto parents
        foreach (var instance in dtoMap.Values)
        {
            instance.ParentProject = instance.ParentProjectId.HasValue && dtoMap.TryGetValue(instance.ParentProjectId.Value, out var value)
                ? value
                : null;
        }
        
        // Map children recursively
        foreach (var instance in dtoMap.Values.Where(dto => dto.ParentProjectId == null))
        {
            MapChildren(instance, dtoMap);
        }

        return dtoMap.Values.ToList();
    }
    
    public static void MapChildren(ProjectInstanceDto dto, Dictionary<Guid, ProjectInstanceDto> dtoMap)
    {
        dto.ChildProjects = dtoMap.Values
            .Where(childDto => childDto.ParentProjectId == dto.Id)
            .ToList();

        foreach (var child in dto.ChildProjects)
        {
            MapChildren(child, dtoMap);
        }
    }

}
