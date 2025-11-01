using InstanceManager.Application.Contracts.DataSet;

namespace InstanceManager.Application.Core.DataSet;

public static class DataSetMappingExtensions
{
    public static DataSetDto ToDto(this DataSet dataSet)
    {
        return new DataSetDto
        {
            Id = dataSet.Id,
            Name = dataSet.Name,
            Description = dataSet.Description,
            Notes = dataSet.Notes,
            IncludedDataSetIds = dataSet.Includes.Select(i => i.IncludedDataSetId).ToList(),
            CreatedAt = dataSet.CreatedAt,
            UpdatedAt = dataSet.UpdatedAt,
            CreatedBy = dataSet.CreatedBy
        };
    }

    public static List<DataSetDto> ToDto(this List<DataSet> dataSets)
    {
        var dtoMap = new Dictionary<Guid, DataSetDto>();

        // First pass: create all DTOs
        foreach (var dataSet in dataSets)
        {
            dtoMap[dataSet.Id] = dataSet.ToDto();
        }

        // Second pass: populate IncludedDataSets navigation property
        foreach (var dataSet in dataSets)
        {
            var dto = dtoMap[dataSet.Id];
            dto.IncludedDataSets = dataSet.Includes
                .Where(i => dtoMap.ContainsKey(i.IncludedDataSetId))
                .Select(i => dtoMap[i.IncludedDataSetId])
                .ToList();
        }

        return dtoMap.Values.ToList();
    }
}
