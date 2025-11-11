using InstanceManager.Authentication.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InstanceManager.Application.Core.Common;

/// <summary>
/// MediatR pipeline behavior that logs all requests with user context information.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var user = _currentUserService.GetCurrentUser();

        _logger.LogInformation(
            "Executing request: {RequestName} | User: {User} | Auth: {AuthMethod} | UserId: {UserId}",
            requestName,
            user.ToLogString(),
            user.AuthenticationMethod,
            user.UserId);

        try
        {
            var response = await next();

            _logger.LogInformation(
                "Completed request: {RequestName} | User: {User}",
                requestName,
                user.ToLogString());

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Request failed: {RequestName} | User: {User} | Error: {ErrorMessage}",
                requestName,
                user.ToLogString(),
                ex.Message);

            throw;
        }
    }
}
