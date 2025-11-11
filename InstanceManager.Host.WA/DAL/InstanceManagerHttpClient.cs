using System.Net.Http.Json;

namespace InstanceManager.Host.WA.DAL;

/// <summary>
/// Typed HttpClient for InstanceManager API.
/// This client automatically includes access tokens for authenticated requests.
/// Configured with BaseAddressAuthorizationMessageHandler.
/// </summary>
public class InstanceManagerHttpClient
{
    private readonly HttpClient _httpClient;

    public InstanceManagerHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TResponse?> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<TResponse>(requestUri, cancellationToken);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest content, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(requestUri, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    public async Task<HttpResponseMessage> PostAsync<TRequest>(string requestUri, TRequest content, CancellationToken cancellationToken = default)
    {
        return await _httpClient.PostAsJsonAsync(requestUri, content, cancellationToken);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string requestUri, TRequest content, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(requestUri, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    public async Task<HttpResponseMessage> PutAsync<TRequest>(string requestUri, TRequest content, CancellationToken cancellationToken = default)
    {
        return await _httpClient.PutAsJsonAsync(requestUri, content, cancellationToken);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        return await _httpClient.DeleteAsync(requestUri, cancellationToken);
    }

    public async Task<TResponse?> DeleteAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }
}

