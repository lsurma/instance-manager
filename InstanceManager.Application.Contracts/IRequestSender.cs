namespace InstanceManager.Application.Contracts;

public interface IRequestSender
{
    public Task<TResponse> SendAsync<TResponse>(object request, CancellationToken cancellationToken = default);
}