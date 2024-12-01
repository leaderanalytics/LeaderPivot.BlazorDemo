/* 
 * When running locally change
 * <base href="/blazor/" />
 * to
 * <base href="/" />
 * in wwwroot/index.html in client project
 * 
 */
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using LeaderPivot.BlazorDemo.Server.Components;
using LeaderAnalytics.LeaderPivot.Blazor;
using MudBlazor.Services;
using LeaderAnalytics.MessageBox.Blazor;
namespace LeaderPivot.BlazorDemo.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "production";
        AppConfig environmentConfig = new AppConfig(environmentName);
        string logFolder = "."; // fallback location if we cannot read config
        Exception? startupEx = null;
        IConfigurationRoot? appConfig = null;

        try
        {
            appConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: false)
                .AddEnvironmentVariables().Build();

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
            bool serverSideBlazor = false;
            Log.Information("Program LeaderPivot.BlazorDemo.Server started");
            Log.Information("Environment is: {env}", environmentName);
            Log.Information("Log files will be written to {logRoot}", logFolder);
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton(environmentConfig); // Used in App.razor
            builder.Services.AddLeaderPivot();
            builder.Services.AddRazorPages();
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents()
                .AddAuthenticationStateSerialization();
            builder.Services.AddMudServices();
            builder.Services.AddMessageBoxBlazor();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseHttpsRedirection();
            app.MapStaticAssets();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode();


            app.Run();
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