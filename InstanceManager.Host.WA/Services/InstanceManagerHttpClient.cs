using System.Net.Http;

namespace InstanceManager.Host.WA.Services
{
    public class InstanceManagerHttpClient
    {
        public HttpClient Client { get; }

        public InstanceManagerHttpClient(HttpClient client)
        {
            Client = client;
        }
    }
}
