using System.Net.Http.Json;
using InstanceManager.Application.Contracts;

namespace InstanceManager.Host.WA.DAL;

public class HttpRequestSender : IRequestSender
{
    private readonly HttpClient _httpClient;
    
    public HttpRequestSender(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<TResponse> SendAsync<TResponse>(object request, CancellationToken cancellationToken = default)
    {
        var requestAsJson = System.Text.Json.JsonSerializer.Serialize(request);
        var urlEncodedRequest = System.Net.WebUtility.UrlEncode(requestAsJson);
        var requestName = request.GetType().Name;
        var data = await _httpClient.GetFromJsonAsync<TResponse>($"Query?request={requestName}&body={urlEncodedRequest}", cancellationToken);
        return data!;
    }
}