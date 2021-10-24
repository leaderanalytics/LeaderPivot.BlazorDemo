using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LeaderPivot.Blazor;


namespace LeaderPivot.BlazorDemo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string logRoot = "c:\\serilog\\DragDrop\\log";

            //Log.Logger = new LoggerConfiguration()
            //  .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            //  .Enrich.FromLogContext()
            //  .WriteTo.Console()
            //  .WriteTo.File(logRoot, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, buffered: true)
            //  .CreateLogger();


            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddLeaderPivot();
            await builder.Build().RunAsync();
        }
    }
}
