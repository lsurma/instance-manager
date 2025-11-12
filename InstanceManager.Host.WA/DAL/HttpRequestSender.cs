using System.Net;
using System.Text.Json;
using InstanceManager.Application.Contracts;
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
        var requestAsJson = JsonSerializer.Serialize(request);
        var urlEncodedRequest = WebUtility.UrlEncode(requestAsJson);
        var requestName = GetRequestName(request.GetType());

        try
        {
            var data = await _httpClient.GetAsync<TResponse>($"query/{requestName}?body={urlEncodedRequest}", cancellationToken);
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

    /// <summary>
    /// Generates a request name for the given type, including generic type arguments
    /// Format: "GetTranslationsQuery&lt;SimpleTranslationDto&gt;" for generic types
    /// </summary>
    private static string GetRequestName(Type requestType)
    {
        if (!requestType.IsGenericType)
        {
            return requestType.Name;
        }

        // For generic types, format as: TypeName<Arg1,Arg2>
        var genericTypeName = requestType.Name;
        var backtickIndex = genericTypeName.IndexOf('`');
        if (backtickIndex > 0)
        {
            genericTypeName = genericTypeName.Substring(0, backtickIndex);
        }

        var genericArgs = requestType.GetGenericArguments();
        var argNames = string.Join(",", genericArgs.Select(t => t.Name));

        return $"{genericTypeName}<{argNames}>";
    }
}