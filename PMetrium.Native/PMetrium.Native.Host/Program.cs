using System.Runtime.InteropServices;
using Newtonsoft.Json;
using PMetrium.Native.Host.Settings;
using PMetrium.Native.Metrics.Android.Implementation;
using PMetrium.Native.Metrics.Android.Implementation.Helpers;
using PMetrium.Native.Metrics.Android.Implementation.Helpers.Extensions;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using WireMock;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using WireMock.Types;
using WireMock.Util;
using static PMetrium.Native.Metrics.Android.Implementation.Helpers.PlatformOSHelper;

var loggerConfiguration = new LoggerConfiguration()
    .WriteTo.Console(
        theme: AnsiConsoleTheme.Sixteen,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

switch (Settings.Instance.LogLevel)
{
    case "Warning":
        loggerConfiguration.MinimumLevel.Warning();
        break;
    case "Debug":
        loggerConfiguration.MinimumLevel.Debug();
        break;
    case "Error":
        loggerConfiguration.MinimumLevel.Error();
        break;
    case "Fatal":
        loggerConfiguration.MinimumLevel.Fatal();
        break;
    case "Verbose":
        loggerConfiguration.MinimumLevel.Verbose();
        break;
    default:
        loggerConfiguration.MinimumLevel.Information();
        break;
}

Log.Logger = loggerConfiguration.CreateLogger();

var server = WireMockServer.Start(new WireMockServerSettings()
{
    StartAdminInterface = true,
    Port = Settings.Instance.WireMockPort
});

var performanceMetricsManager = new PerformanceMetricsManager(new InfluxDBSync(
    Settings.Instance.InfluxDB.Url,
    Settings.Instance.InfluxDB.User,
    Settings.Instance.InfluxDB.Password,
    Settings.Instance.InfluxDB.DataBaseName));

// Just in case to add +x for shell scripts
if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    await CreateProcess("chmod", "+x Files/Scripts/Shell/healthCheck.sh").StartProcessAndWait();
    await CreateProcess("chmod", "+x Files/Scripts/Shell/logcat.sh").StartProcessAndWait();
    await CreateProcess("chmod", "+x Files/Scripts/Shell/phoneMetrics.sh").StartProcessAndWait();
    await CreateProcess("chmod", "+x Files/Scripts/Shell/readFile.sh").StartProcessAndWait();
    await CreateProcess("chmod", "+x Files/Scripts/Shell/start.sh").StartProcessAndWait();
    await CreateProcess("chmod", "+x Files/Scripts/Shell/stop.sh").StartProcessAndWait();
}

RegisterHealthCheckMethod();
RegisterStartMethod();
RegisterStopMethod();

Log.Information($"Host has been started on - {server.Url}");
await Task.Delay(-1);

void RegisterHealthCheckMethod()
{
    server.Given(Request.Create()
            .WithPath(path => path.Contains("HealthCheck"))
            .UsingGet())
        .RespondWith(Response.Create()
            .WithCallback(async requestMessage =>
            {
                CancellationTokenSource source = new CancellationTokenSource();
                source.CancelAfter(TimeSpan.FromSeconds(10));

                var result = await performanceMetricsManager.HealthCheck(source.Token);

                if (result)
                {
                    return new ResponseMessage()
                    {
                        StatusCode = 200,
                        BodyData = new BodyData()
                        {
                            BodyAsString = "OK",
                            DetectedBodyType = BodyType.String
                        }
                    };
                }

                return new ResponseMessage()
                {
                    StatusCode = 500,
                    BodyData = new BodyData()
                    {
                        BodyAsString = "FAIL",
                        DetectedBodyType = BodyType.String
                    }
                };
            }));
}

void RegisterStartMethod()
{
    server.Given(Request.Create()
            .WithPath(path => path.Contains("Start"))
            .UsingGet())
        .RespondWith(Response.Create()
            .WithCallback(async requestMessage =>
            {
                var parameters = GetRequestParameters(requestMessage);

                Log.Debug($"HTTP Request to Start with parameters: " +
                          $"{JsonConvert.SerializeObject(parameters, Formatting.Indented)}");

                await performanceMetricsManager.Start(parameters, CancellationToken.None);

                return new ResponseMessage()
                {
                    StatusCode = 200
                };
            }));
}

void RegisterStopMethod()
{
    server.Given(Request.Create()
            .WithPath(path => path.Contains("Stop"))
            .UsingGet())
        .RespondWith(Response.Create()
            .WithCallback(async requestMessage =>
            {
                var parameters = GetRequestParameters(requestMessage);

                Log.Debug($"HTTP Request to Stop with parameters: " +
                          $"{JsonConvert.SerializeObject(parameters, Formatting.Indented)}");

                var source = new CancellationTokenSource();
                source.CancelAfter(TimeSpan.FromSeconds(60));

                await Task.Delay(Settings.Instance.AppEventsTimeout);
                await performanceMetricsManager.Stop(parameters, source.Token);

                return new ResponseMessage()
                {
                    StatusCode = 200
                };
            }));
}

Dictionary<string, string> GetRequestParameters(RequestMessage requestMessage)
{
    Dictionary<string, string> parameters = new();

    foreach (var pair in requestMessage.Query)
        parameters.Add(pair.Key, pair.Value[0]);

    return parameters;
}