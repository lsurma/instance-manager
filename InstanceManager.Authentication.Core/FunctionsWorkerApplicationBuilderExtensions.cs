using InstanceManager.Authentication.Core.Authentication;
using InstanceManager.Authentication.Core.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InstanceManager.Authentication.Core;

public static class FunctionsWorkerApplicationBuilderExtensions
{
    public static IFunctionsWorkerApplicationBuilder UseInstanceManagerAuthentication(this IFunctionsWorkerApplicationBuilder builder)
    {
        builder.UseWhen<ApimBypassMiddleware>(context =>
        {
            var settings = context.InstanceServices.GetRequiredService<AuthenticationSettings>();
            return settings.Apim.TrustApim;
        });

        builder.UseMiddleware<FunctionsAuthorizationMiddleware>();

        return builder;
    }
}