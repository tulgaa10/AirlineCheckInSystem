using Airline.BlazorApp;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://192.168.137.1:7257/") });

        builder.Services.AddSingleton(sp => new FlightHubService("http://192.168.137.1:7257/flighthub"));

        await builder.Build().RunAsync();
    }
}