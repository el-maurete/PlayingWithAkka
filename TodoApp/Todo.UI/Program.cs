using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Todo.UI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(_ => new HttpClient
{
    BaseAddress = new Uri(
        Environment.GetEnvironmentVariable("API_URL")
        ?? "http://localhost:5000")
});

await builder.Build().RunAsync();
