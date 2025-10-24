using System.Net.Http.Json;
using InstanceManager.Application.Contracts;
using MediatR;

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
        var data = await _httpClient.GetFromJsonAsync<TResponse>($"query/{requestName}?body={urlEncodedRequest}", cancellationToken);
        return data!;
    }
    
    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return SendAsync<TResponse>((object)request, cancellationToken);
    }
}