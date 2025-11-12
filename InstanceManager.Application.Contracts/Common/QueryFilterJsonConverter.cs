using System.Text.Json;
using System.Text.Json.Serialization;

namespace InstanceManager.Application.Contracts.Common;

public static class QueryFilterTypeRegistry
{
    private static readonly Dictionary<string, Type> _filterTypes = new();
    
    static QueryFilterTypeRegistry()
    {
        // Auto-discover all IQueryFilter implementations in the assembly
        var assembly = typeof(IQueryFilter).Assembly;
        var filterTypes = assembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract && typeof(IQueryFilter).IsAssignableFrom(t))
            .ToList();
        
        foreach (var type in filterTypes)
        {
            // Create temporary instance to get the Name (cheap operation, done once at startup)
            var instance = Activator.CreateInstance(type) as IQueryFilter;
            if (instance != null)
            {
                _filterTypes[instance.Name] = type;
            }
        }
    }
    
    public static Type? GetTypeByName(string name)
    {
        return _filterTypes.TryGetValue(name, out var type) ? type : null;
    }
}

public class QueryFilterListJsonConverter : JsonConverter<List<IQueryFilter>>
{
    public override List<IQueryFilter>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = new List<IQueryFilter>();
        
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token");
        }
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }
            
            var converter = new QueryFilterJsonConverter();
            var filter = converter.Read(ref reader, typeof(IQueryFilter), options);
            if (filter != null)
            {
                list.Add(filter);
            }
        }
        
        return list;
    }
    
    public override void Write(Utf8JsonWriter writer, List<IQueryFilter> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        
        var converter = new QueryFilterJsonConverter();
        foreach (var filter in value)
        {
            converter.Write(writer, filter, options);
        }
        
        writer.WriteEndArray();
    }
}

public class QueryFilterJsonConverter : JsonConverter<IQueryFilter>
{
    public override IQueryFilter? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;
        
        // Look for the Name property to determine the type
        if (root.TryGetProperty("Name", out var nameProperty))
        {
            var filterName = nameProperty.GetString();
            if (!string.IsNullOrEmpty(filterName))
            {
                var type = QueryFilterTypeRegistry.GetTypeByName(filterName);
                if (type != null)
                {
                    return (IQueryFilter?)JsonSerializer.Deserialize(root.GetRawText(), type, options);
                }
            }
        }
        
        throw new JsonException("Unable to determine IQueryFilter type. Missing or invalid 'Name' property.");
    }

    public override void Write(Utf8JsonWriter writer, IQueryFilter value, JsonSerializerOptions options)
    {
        // Use default serialization - Name property is included automatically
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
