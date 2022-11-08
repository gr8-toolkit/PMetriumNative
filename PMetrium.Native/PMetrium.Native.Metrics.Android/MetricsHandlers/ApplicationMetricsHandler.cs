using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using InfluxDB.Client.Writes;
using Newtonsoft.Json;
using PMetrium.Native.Common.Contracts;
using PMetrium.Native.Common.Helpers;
using PMetrium.Native.Common.Helpers.Extensions;
using Serilog;
using static PMetrium.Native.Common.Helpers.PlatformOSHelper;

namespace PMetrium.Native.Metrics.Android.MetricsHandlers;

public class ApplicationMetricsHandler
{
    private InfluxDBSync _influxDbSync;
    private AndroidDeviceContext _deviceContext;
    private AndroidPerformanceResults _androidPerformanceResults;

    public ApplicationMetricsHandler(
        AndroidDeviceContext deviceContext,
        InfluxDBSync influxDbSync,
        AndroidPerformanceResults androidPerformanceResults)
    {
        _influxDbSync = influxDbSync;
        _deviceContext = deviceContext;
        _androidPerformanceResults = androidPerformanceResults;
    }

    public async Task ExtractAndSaveMetrics(CancellationToken token)
    {
        var device = _deviceContext.DeviceParameters.Device;

        Log.Information($"[Android: {device}] ApplicationMetricsHandler - start to handle application events");

        var process = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? CreateProcess($"{WorkingDirectory}\\Scripts\\Bat\\logcat.bat", device)
            : CreateProcess($"{WorkingDirectory}/Scripts/Shell/logcat.sh", device);

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
            $"[Android: {device}] ApplicationMetricsHandler - events: {JsonConvert.SerializeObject(metricsLogs, Formatting.Indented)}");

        for (int i = 1; i <= metricsLogs.Count; i++)
        {
            var timestamp = Regex.Match(metricsLogs[i - 1], "\\d+$").Value;
            var dateTime = DateTime.UnixEpoch.AddMilliseconds(long.Parse(timestamp));

            if (previousTimestamp == 0)
                previousTimestamp = long.Parse(timestamp);

            var text = $"{Regex.Replace(metricsLogs[i - 1], "\\d+$", "")}";

            await _influxDbSync.SaveAnnotationToInfluxDBAsync(
                "android.annotations",
                "[ AppEvent ]",
                _deviceContext.AnnotationTags,
                _deviceContext.CommonTags,
                dateTime,
                text);

            var eventTags = _deviceContext.CommonTags.ToDictionary(entry => entry.Key, entry => entry.Value);
            eventTags.Add("event", text);

            var latency = long.Parse(timestamp) - previousTimestamp;
            points.Add(_influxDbSync.GeneratePoint(
                "android.events.app",
                eventTags,
                dateForEvents,
                latency,
                "latency"));

            _androidPerformanceResults.Events.Add(new Event()
            {
                Timestamp = dateTime,
                Name = text,
                Latency_ms = latency
            });
        }

        await _influxDbSync.SavePoints(points.ToArray());

        Log.Information($"[Android: {device}] ApplicationMetricsHandler - saved events: {points.Count}");
    }
}