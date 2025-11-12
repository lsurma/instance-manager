using System.Net;
using System.Net.Http.Json;
using InstanceManager.Application.Contracts;
using InstanceManager.Host.WA.Services;
using MediatR;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace InstanceManager.Host.WA.DAL;

public class HttpRequestSender : IRequestSender
{
    private readonly InstanceManagerHttpClient _httpClient;

    public HttpRequestSender(InstanceManagerHttpClient httpClient)
    {
        // HttpClient wrapper that uses the named 'InstanceManager.API' client
        // which is configured with BaseAddressAuthorizationMessageHandler
        // that automatically attaches access tokens to API requests
        _httpClient = httpClient;
    }

    public async Task<TResponse> SendAsync<TResponse>(object request, CancellationToken cancellationToken = default)
    {
        var requestAsJson = System.Text.Json.JsonSerializer.Serialize(request);
        var urlEncodedRequest = System.Net.WebUtility.UrlEncode(requestAsJson);
        var requestName = request.GetType().Name;

        try
        {
            var data = await _httpClient.Client.GetFromJsonAsync<TResponse>($"query/{requestName}?body={urlEncodedRequest}", cancellationToken);
            return data!;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            // Redirect to login if access token is not available
            exception.Redirect();
            throw;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Handle 401 Unauthorized - token might be expired or invalid
            throw new InvalidOperationException("Authentication failed. Please log in again.", ex);
        }
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return SendAsync<TResponse>((object)request, cancellationToken);
    }
}
