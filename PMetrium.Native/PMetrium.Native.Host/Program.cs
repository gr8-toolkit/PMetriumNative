using System.Runtime.InteropServices;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using PMetrium.Native.Common.Helpers;
using PMetrium.Native.Common.Helpers.Extensions;
using PMetrium.Native.Host.Config;
using PMetrium.Native.IOS;
using PMetrium.Native.Metrics.Android;
using Serilog;
using Serilog.Events;
using static PMetrium.Native.Common.Helpers.PlatformOSHelper;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables();
Config.Instance = builder.Configuration.Get<ConfigModel>();

var loggerConfiguration = new LoggerConfiguration()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}");

loggerConfiguration.MinimumLevel.Is(ParseEnum<LogEventLevel>(Config.Instance.LogLevel));
loggerConfiguration.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);
loggerConfiguration.MinimumLevel.Override("System.Net.Http.HttpClient.GatewayClient", LogEventLevel.Warning);
loggerConfiguration.MinimumLevel.Override("Microsoft.AspNetCore.Http.Connections", LogEventLevel.Warning);

var logger = loggerConfiguration.CreateLogger();
Log.Logger = logger;
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services
    .AddControllersWithViews()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.Converters.Add(new StringEnumConverter()));
builder.Services.AddSwaggerGenNewtonsoftSupport();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.ToString());
    c.UseInlineDefinitionsForEnums();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PMetrium Native v2.0",
        Description = "Written by Mykola Panasiuk. Parimatch Tech</br>" +
                      "<a href='https://github.com/parimatch-tech/PMetriumNative'>GitHub</a></br>" +
                      "<a href='https://parimatch-tech.github.io/PMetriumNative'>Documentation</a>"
    });
});

builder.Services.AddSingleton<IAndroidMetricsManager>(new AndroidMetricsManager(new InfluxDBSync(
    Config.Instance.InfluxDB.Url,
    Config.Instance.InfluxDB.User,
    Config.Instance.InfluxDB.Password,
    Config.Instance.InfluxDB.DataBaseName)));

builder.Services.AddSingleton<IIOSMetricsManager>(new IOSMetricsManager(new InfluxDBSync(
    Config.Instance.InfluxDB.Url,
    Config.Instance.InfluxDB.User,
    Config.Instance.InfluxDB.Password,
    Config.Instance.InfluxDB.DataBaseName)));

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    await CreateProcess("chmod", $"+x {WorkingDirectory}/Scripts/Shell/logcat.sh").StartProcessAndWait();
    await CreateProcess("chmod", $"+x {WorkingDirectory}/Scripts/Shell/phoneMetrics.sh").StartProcessAndWait();
    await CreateProcess("chmod", $"+x {WorkingDirectory}/Scripts/Shell/readFile.sh").StartProcessAndWait();
    await CreateProcess("chmod", $"+x {WorkingDirectory}/Scripts/Shell/start.sh").StartProcessAndWait();
    await CreateProcess("chmod", $"+x {WorkingDirectory}/Scripts/Shell/stop.sh").StartProcessAndWait();
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.DefaultModelsExpandDepth(-1));
app.UseAuthorization();
app.MapControllers();
app.Run($"http://localhost:{Config.Instance.HostPort}");

static T ParseEnum<T>(string value)
{
    return (T)Enum.Parse(typeof(T), value, true);
}