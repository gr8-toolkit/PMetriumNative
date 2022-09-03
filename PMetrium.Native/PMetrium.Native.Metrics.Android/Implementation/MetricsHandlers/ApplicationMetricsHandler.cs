using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using InfluxDB.Client.Writes;
using Newtonsoft.Json;
using PMetrium.Native.Metrics.Android.Contracts;
using PMetrium.Native.Metrics.Android.Implementation.Helpers;
using PMetrium.Native.Metrics.Android.Implementation.Helpers.Extensions;
using Serilog;
using static PMetrium.Native.Metrics.Android.Implementation.Helpers.PlatformOSHelper;

namespace PMetrium.Native.Metrics.Android.Implementation.MetricsHandlers;

public class ApplicationMetricsHandler
{
    private InfluxDBSync _influxDbSync;
    private DeviceContext _deviceContext;

    public ApplicationMetricsHandler(DeviceContext deviceContext, InfluxDBSync influxDbSync)
    {
        _influxDbSync = influxDbSync;
        _deviceContext = deviceContext;
    }

    public async Task ExtractAndSaveMetrics(CancellationToken token)
    {
        var device = _deviceContext.DeviceParameters.Device;

        Log.Information($"[{device}] ApplicationMetricsHandler - start to handle application events");

        var process = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? CreateProcess("Files\\Scripts\\Bat\\logcat.bat", device)
            : CreateProcess("Files/Scripts/Shell/logcat.sh", device);

        var logs = new List<string?>();

        process.OutputDataReceived += (sender, outLine) =>
        {
            if (!string.IsNullOrEmpty(outLine.Data))
                logs.Add(outLine.Data);
        };

        process.StartForDevice(device);
        await process.WaitForExitAsync(token);

        logs.RemoveAll(x => !x.Contains("PERFORMANCE_TESTING"));
        var metricsLogs = logs.Select(x => Regex.Replace(x, "^.*PERFORMANCE_TESTING: ", "")).ToList();
        metricsLogs = metricsLogs.Select(x => Regex.Replace(x, "Event with \\d+ ", "Event ")).ToList();

        var points = new List<PointData>();
        long previousTimestamp = 0;
        var dateForEvents = DateTime.UtcNow;

        Log.Debug(
            $"[{device}] ApplicationMetricsHandler - events: {JsonConvert.SerializeObject(metricsLogs, Formatting.Indented)}");

        for (int i = 1; i <= metricsLogs.Count; i++)
        {
            var timestamp = Regex.Match(metricsLogs[i - 1], "\\d+$").Value;
            var dateTime = DateTime.UnixEpoch.AddMilliseconds(long.Parse(timestamp));

            if (previousTimestamp == 0)
            {
                // -1 here is needed for the visualization on Grafana
                previousTimestamp = long.Parse(timestamp) - 1;
            }

            var text = $"{i}. {Regex.Replace(metricsLogs[i - 1], "\\d+$", "")}";

            await _influxDbSync.SaveAnnotationToInfluxDBAsync(
                "android.annotations",
                "[ AppEvent ]",
                _deviceContext.AnnotationTags,
                _deviceContext.CommonTags,
                dateTime,
                text);

            var eventTags = _deviceContext.CommonTags.ToDictionary(entry => entry.Key, entry => entry.Value);
            eventTags.Add("event", text);

            points.Add(_influxDbSync.GeneratePoint(
                "android.events.app",
                eventTags,
                dateForEvents,
                long.Parse(timestamp) - previousTimestamp,
                "latency"));
        }

        await _influxDbSync.SavePoints(points.ToArray());

        Log.Information($"[{device}] ApplicationMetricsHandler - saved events: {points.Count}");
    }
}