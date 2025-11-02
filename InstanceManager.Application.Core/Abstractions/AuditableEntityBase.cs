namespace InstanceManager.Application.Core.Abstractions;

public abstract class AuditableEntityBase : EntityBase, IAuditableEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public required string CreatedBy { get; set; }
}
