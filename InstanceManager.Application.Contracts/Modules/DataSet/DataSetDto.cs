namespace InstanceManager.Application.Contracts.Modules.DataSet;

public record DataSetDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// List of user/application identity IDs that have access to this DataSet.
    /// If empty, the DataSet has public access (no restrictions).
    /// </summary>
    public ICollection<string> AllowedIdentityIds { get; set; } = new List<string>();

    public ICollection<Guid> IncludedDataSetIds { get; set; } = new List<Guid>();

    public ICollection<DataSetDto> IncludedDataSets { get; set; } = new List<DataSetDto>();

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}
