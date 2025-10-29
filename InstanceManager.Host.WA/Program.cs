using InstanceManager.Application.Contracts;
using InstanceManager.Host.WA;
using InstanceManager.Host.WA.DAL;
using InstanceManager.Host.WA.Services;
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
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<NavigationHelper>();

builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();