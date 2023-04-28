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
string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "production";
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

    // Add services to the container.
    builder.Services.AddRazorPages();
    
    if (serverSideBlazor)
    {
        builder.Services.AddServerSideBlazor();     // Server
    }
    else
    {
        builder.Services.AddControllersWithViews(); // WASM
    }
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
    }

    app.UseHttpsRedirection();
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();
    app.UseRouting();

    if (serverSideBlazor)
    {
        app.MapBlazorHub();                     // Server
        app.MapFallbackToPage("/_Host");        // Server
    }
    else
    {
        app.MapRazorPages();                    // WASM
        app.MapControllers();                   // WASM
        app.MapFallbackToFile("index.html");    // WASM
    }

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

