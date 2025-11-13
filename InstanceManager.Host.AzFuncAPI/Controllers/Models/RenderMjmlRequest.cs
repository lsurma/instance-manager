using System.Text.Json.Serialization;

namespace InstanceManager.Host.AzFuncAPI.Controllers.Models;

public class RenderMjmlRequest
{
    [JsonPropertyName("html")]
    public string? Html { get; set; }

    [JsonPropertyName("variables")]
    public string? Variables { get; set; }
}
