namespace InstanceManager.Application.Core.Abstractions;

public abstract class EntityBase : IEntity
{
    public Guid Id { get; set; }
}
