namespace InstanceManager.Application.Core.Abstractions;

/// <summary>
/// Base interface for entities with a generic primary key type
/// </summary>
public interface IEntity<TPrimaryKey> where TPrimaryKey : notnull
{
    TPrimaryKey Id { get; set; }
}

/// <summary>
/// Base interface for entities with Guid primary key (backward compatibility)
/// </summary>
public interface IEntity : IEntity<Guid>
{
}
