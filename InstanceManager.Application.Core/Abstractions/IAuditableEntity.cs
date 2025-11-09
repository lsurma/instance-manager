namespace InstanceManager.Application.Core.Abstractions;

public interface IAuditableEntity : IEntity
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    string CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
}
