using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LeaderAnalytics.LeaderPivot.Blazor;
using Microsoft.AspNetCore.Components.Web;

namespace LeaderPivot.BlazorDemo;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddLeaderPivot();
        await builder.Build().RunAsync();
    }
}
