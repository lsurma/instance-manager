using InstanceManager.Application.Contracts;
using InstanceManager.Host.WA;
using InstanceManager.Host.WA.DAL;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:7233/api/")
});
builder.Services.AddScoped<IRequestSender, HttpRequestSender>();

builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();