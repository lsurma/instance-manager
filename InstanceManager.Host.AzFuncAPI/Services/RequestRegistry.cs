using MediatR;
using System.Reflection;

namespace InstanceManager.Host.AzFuncAPI.Services;

public class RequestRegistry
{
    private readonly Dictionary<string, Type> _requestTypes = new(StringComparer.OrdinalIgnoreCase);

    public RequestRegistry()
    {
        ScanForRequests();
    }

    private void ScanForRequests()
    {
        var contractsAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "InstanceManager.Application.Contracts");

        if (contractsAssembly == null)
        {
            return;
        }

        var requestTypes = contractsAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)));

        foreach (var type in requestTypes)
        {
            _requestTypes[type.Name] = type;
        }
    }

    public Type? GetRequestType(string requestName)
    {
        _requestTypes.TryGetValue(requestName, out var type);
        return type;
    }

    public IEnumerable<string> GetAllRequestNames()
    {
        return _requestTypes.Keys;
    }
}
