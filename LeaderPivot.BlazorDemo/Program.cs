using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LeaderAnalytics.LeaderPivot.Blazor;
using Serilog;

namespace LeaderPivot.BlazorDemo;

public class Program
{
    public static async Task Main(string[] args)
    {
        string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "production";
        string logFolder = "."; // fallback location if we cannot read config
        Exception startupEx = null;
        IConfigurationRoot appConfig = null;

        try
        {
            if (string.IsNullOrEmpty(environmentName))
                throw new Exception("ASPNETCORE_ENVIRONMENT environment variableis not set.");

            var cfg = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile($"appsettings.{environmentName}.json", optional: false)
                    .Build();

            logFolder = appConfig["Logging:LogFolder"];
        }
        catch (Exception ex)
        {
            startupEx = ex;
        }
        finally
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logFolder, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
        }

        if (startupEx != null)
        {
            Log.Fatal("An exception occured during startup configuration.  Program execution will not continue.");
            Log.Fatal(startupEx.ToString());
            Log.CloseAndFlush();
            System.Threading.Thread.Sleep(2000);
            return;
        }

        try
        {
            Log.Information("Program LeaderPivot.BlazorDemo started");
            Log.Information("Environment is: {env}", environmentName);
            Log.Information("Log files will be written to {logRoot}", logFolder);
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddLeaderPivot();
            await builder.Build().RunAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
        }
        finally
        {
            Log.CloseAndFlush();
            await Task.Delay(2000);
        }
    }
}
