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
    Log.Information("Program LeaderPivot.BlazorDemo.Server started");
    Log.Information("Environment is: {env}", environmentName);
    Log.Information("Log files will be written to {logRoot}", logFolder);
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();
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
    app.MapRazorPages();
    app.MapControllers();
    app.MapFallbackToFile("index.html");
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

