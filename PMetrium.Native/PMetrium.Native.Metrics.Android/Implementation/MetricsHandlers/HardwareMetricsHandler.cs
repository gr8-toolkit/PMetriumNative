using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using InfluxDB.Client.Writes;
using PMetrium.Native.Metrics.Android.Contracts;
using PMetrium.Native.Metrics.Android.Implementation.Helpers;
using PMetrium.Native.Metrics.Android.Implementation.Helpers.Extensions;
using Serilog;
using static PMetrium.Native.Metrics.Android.Implementation.Helpers.PlatformOSHelper;

namespace PMetrium.Native.Metrics.Android.Implementation.MetricsHandlers
{
    internal class HardwareMetricsHandler
    {
        private InfluxDBSync _influxDbSync;
        private DeviceContext _deviceContext;

        public HardwareMetricsHandler(DeviceContext deviceContext, InfluxDBSync influxDbSync)
        {
            _influxDbSync = influxDbSync;
            _deviceContext = deviceContext;
        }

        public void ExtractAndSaveMetrics(CancellationToken token)
        {
            var device = _deviceContext.DeviceParameters.Device;

            Log.Information($"[{device}] HardwareMetricsHandler - start to handle hardware metrics");

            var tasks = new List<Task>();

            tasks.Add(Task.Run(async () => await ExtractAndSaveCpuMetrics(token)));
            tasks.Add(Task.Run(async () => await ExtractAndSaveRamMetrics(token)));
            tasks.Add(Task.Run(async () => await ExtractAndSaveNetworkMetrics(token)));
            tasks.Add(Task.Run(async () => await ExtractAndSaveBatteryMetrics(token)));
            tasks.Add(Task.Run(async () => await ExtractAndSaveFramesMetrics(token)));

            Task.WaitAll(tasks.ToArray());

            Log.Information($"[{device}] HardwareMetricsHandler - stop to handle hardware metrics");
        }

        private async Task ExtractAndSaveCpuMetrics(CancellationToken token)
        {
            var cpuTotalRaw = (await ExtractDataFromFileOnPhone("cpu_total.txt", token))
                .Replace("%cpu", "", true, CultureInfo.CurrentCulture);
            var cpuUsageTotalRaw = (await ExtractDataFromFileOnPhone("cpu_usage_total.txt", token))
                .Replace("%idle", "", true, CultureInfo.CurrentCulture);
            var cpuUsageAppRaw = await ExtractDataFromFileOnPhone("cpu_usage_app.txt", token);
            var cpuTotal = double.Parse(cpuTotalRaw);

            var cpuUsageTotalMetrics = ParseOneMetric(cpuUsageTotalRaw.Split("\r\n"));
            var points = new List<PointData>();

            foreach (var metric in cpuUsageTotalMetrics)
            {
                points.Add(_influxDbSync.GeneratePoint(
                    "android.cpu.usage.total",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    Math.Round((cpuTotal - metric.firstMetric) / cpuTotal * 100d, 2),
                    "percentage"));
            }

            var cpuUsageAppMetrics = ParseOneMetric(cpuUsageAppRaw.Split("\r\n"));

            foreach (var metric in cpuUsageAppMetrics)
            {
                points.Add(_influxDbSync.GeneratePoint(
                    "android.cpu.usage.app",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    Math.Round(metric.firstMetric / cpuTotal * 100d, 2),
                    "percentage"));
            }

            await _influxDbSync.SavePoints(points.ToArray());

            Log.Debug(
                $"[{_deviceContext.DeviceParameters.Device}] HardwareMetricsHandler - stop to handle CPU metrics");
        }

        private async Task ExtractAndSaveRamMetrics(CancellationToken token)
        {
            var ramTotalRaw = await ExtractDataFromFileOnPhone("ram_total.txt", token);
            var ramTotal = double.Parse(ramTotalRaw);
            var ramUsageTotalRaw = await ExtractDataFromFileOnPhone("ram_usage_total.txt", token);
            var ramUsageAppRaw = await ExtractDataFromFileOnPhone("ram_usage_app.txt", token);

            var ramUsageTotalMetrics = ParseOneMetric(ramUsageTotalRaw.Split("\r\n"));
            var points = new List<PointData>();

            foreach (var metric in ramUsageTotalMetrics)
            {
                points.Add(_influxDbSync.GeneratePoint(
                    $"android.ram.total",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    ramTotal,
                    "byte"));

                points.Add(_influxDbSync.GeneratePoint(
                    $"android.ram.usage.total",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    ramTotal - (metric.firstMetric * 1024),
                    "byte"));
            }

            var ramUsageAppMetrics = ParseTwoMetrics(ramUsageAppRaw.Split("\r\n"));

            foreach (var metric in ramUsageAppMetrics)
            {
                points.Add(_influxDbSync.GeneratePoint(
                    $"android.ram.usage.app.pss",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.firstMetric * 1024d,
                    "byte"));

                points.Add(_influxDbSync.GeneratePoint(
                    $"android.ram.usage.app.private",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.secondMetric * 1024d,
                    "byte"));
            }

            await _influxDbSync.SavePoints(points.ToArray());

            Log.Debug(
                $"[{_deviceContext.DeviceParameters.Device}] HardwareMetricsHandler - stop to handle RAM metrics");
        }

        private async Task ExtractAndSaveNetworkMetrics(CancellationToken token)
        {
            var networkUsageAppRaw = await ExtractDataFromFileOnPhone("network_usage_app.txt", token);
            var networkUsageAppMetrics = ParseTwoMetrics(networkUsageAppRaw.Split("\r\n"));
            var points = new List<PointData>();

            foreach (var metric in networkUsageAppMetrics)
            {
                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.app.transferred",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    Math.Round(metric.firstMetric * 1000d, 0),
                    "byte"));

                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.app.received",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    Math.Round(metric.secondMetric * 1000d, 0),
                    "byte"));
            }

            await _influxDbSync.SavePoints(points.ToArray());

            Log.Debug(
                $"[{_deviceContext.DeviceParameters.Device}] HardwareMetricsHandler - stop to handle NETWORK metrics");
        }

        private async Task ExtractAndSaveBatteryMetrics(CancellationToken token)
        {
            var batteryUsageAppRaw = await ExtractDataFromFileOnPhone("battery_app.txt", token);
            var batteryUsageAppMetrics = ParseOneMetric(batteryUsageAppRaw.Split("\r\n"));
            var points = new List<PointData>();

            foreach (var metric in batteryUsageAppMetrics)
            {
                points.Add(_influxDbSync.GeneratePoint(
                    "android.battery.usage.app",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.firstMetric,
                    "mAh"));
            }

            await _influxDbSync.SavePoints(points.ToArray());

            Log.Debug(
                $"[{_deviceContext.DeviceParameters.Device}] HardwareMetricsHandler - stop to handle BATTERY metrics");
        }

        private async Task ExtractAndSaveFramesMetrics(CancellationToken token)
        {
            var framesAppRaw = await ExtractDataFromFileOnPhone("frames_app.txt", token);
            var framesAppMetrics = ParseFramesMetrics(framesAppRaw
                .Split(new[] { "Total" }, StringSplitOptions.RemoveEmptyEntries));
            var points = new List<PointData>();

            foreach (var metric in framesAppMetrics)
            {
                points.Add(_influxDbSync.GeneratePoint(
                    $"android.frames.rendered",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.firstMetric,
                    "count"));

                points.Add(_influxDbSync.GeneratePoint(
                    $"android.frames.janky",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.secondMetric,
                    "count"));

                points.Add(_influxDbSync.GeneratePoint(
                    $"android.frames.rendering",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.thirdMetric,
                    "pct50"));

                points.Add(_influxDbSync.GeneratePoint(
                    $"android.frames.rendering",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.fourthMetric,
                    "pct90"));

                points.Add(_influxDbSync.GeneratePoint(
                    $"android.frames.rendering",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.fifthMetric,
                    "pct95"));

                points.Add(_influxDbSync.GeneratePoint(
                    $"android.frames.rendering",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.sixthMetric,
                    "pct99"));
            }

            await _influxDbSync.SavePoints(points.ToArray());

            Log.Debug(
                $"[{_deviceContext.DeviceParameters.Device}] HardwareMetricsHandler - stop to handle FRAMES metrics");
        }

        private List<(
            DateTime dateTime,
            double firstMetric,
            double secondMetric,
            double thirdMetric,
            double fourthMetric,
            double fifthMetric,
            double sixthMetric)> ParseFramesMetrics(string[] metricsRaw)
        {
            var result = new List<(
                DateTime dateTime,
                double firstMetric,
                double secondMetric,
                double thirdMetric,
                double fourthMetric,
                double fifthMetric,
                double sixthMetric)>();

            foreach (var line in metricsRaw)
            {
                var splitResult = line.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                var metricsList = splitResult.Where(x =>
                    x.Contains("frames rendered") ||
                    x.Contains("Janky frames") &&
                    !x.Contains("(legacy)") ||
                    x.Contains("50th percentile") ||
                    x.Contains("90th percentile") ||
                    x.Contains("95th percentile") ||
                    x.Contains("99th percentile") ||
                    long.TryParse(x, out _)).ToList();

                if (metricsList.Count != 7) continue;

                var timestamp = metricsList.Single(x => long.TryParse(x, out _));
                var dateTime = DateTime.UnixEpoch.AddSeconds(long.Parse(timestamp));

                var firstMetric = double.Parse(
                    metricsList.Single(x => x.Contains("frames rendered"))
                        .Replace("frames rendered: ", ""),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture);

                var secondMetric = metricsList
                    .Single(x => x.Contains("Janky frames"))
                    .Replace("Janky frames: ", "");
                secondMetric = Regex.Replace(secondMetric, "\\(\\d+.\\d+%\\)", "");

                var thirdMetric = double.Parse(metricsList
                    .Single(x => x.Contains("50th percentile"))
                    .Replace("50th percentile: ", "")
                    .Replace("ms", ""), NumberStyles.Any, CultureInfo.InvariantCulture);

                var fourthMetric = double.Parse(metricsList
                    .Single(x => x.Contains("90th percentile"))
                    .Replace("90th percentile: ", "")
                    .Replace("ms", ""), NumberStyles.Any, CultureInfo.InvariantCulture);

                var fifthMetric = double.Parse(metricsList
                    .Single(x => x.Contains("95th percentile"))
                    .Replace("95th percentile: ", "")
                    .Replace("ms", ""), NumberStyles.Any, CultureInfo.InvariantCulture);

                var sixthMetric = double.Parse(metricsList
                    .Single(x => x.Contains("99th percentile"))
                    .Replace("99th percentile: ", "")
                    .Replace("ms", ""), NumberStyles.Any, CultureInfo.InvariantCulture);

                result.Add((dateTime, firstMetric, double.Parse(secondMetric), thirdMetric, fourthMetric, fifthMetric,
                    sixthMetric));
            }

            return result;
        }

        private async Task<string> ExtractDataFromFileOnPhone(string fileName, CancellationToken token)
        {
            var result = "";
            if (!token.IsCancellationRequested)
            {
                var process = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? CreateProcess("Files\\Scripts\\Bat\\readFile.bat", $"{_deviceContext.DeviceParameters.Device} {fileName}")
                    : CreateProcess("Files//Scripts/Shell/readFile.sh", $"{_deviceContext.DeviceParameters.Device} {fileName}");

                process.OutputDataReceived += (sender, outLine) =>
                {
                    if (!string.IsNullOrEmpty(outLine.Data))
                        result += outLine.Data + "\r\n";
                };

                process.StartForDevice(_deviceContext.DeviceParameters.Device);
                await process.WaitForExitAsync(token);
            }

            return result;
        }

        private List<(DateTime dateTime, double firstMetric)> ParseOneMetric(string[] metricsRaw)
        {
            var result = new List<(DateTime dateTime, double firstMetric)>();

            foreach (var line in metricsRaw)
            {
                var splitResult = line.Split("_");

                if (splitResult.Length != 2) continue;

                var dateTime = DateTime.UnixEpoch.AddSeconds(long.Parse(splitResult[0]));
                var firstMetric = double.Parse(splitResult[1], NumberStyles.Any, CultureInfo.InvariantCulture);

                result.Add((dateTime, firstMetric));
            }

            return result;
        }

        private List<(DateTime dateTime, double firstMetric, double secondMetric)> ParseTwoMetrics(
            string[] metricsRaw)
        {
            var result = new List<(DateTime dateTime, double firstMetric, double secondMetric)>();

            foreach (var line in metricsRaw)
            {
                var splitResult = line.Split("_");

                if (splitResult.Length != 3) continue;

                var dateTime = DateTime.UnixEpoch.AddSeconds(long.Parse(splitResult[0]));
                var firstMetric = double.Parse(splitResult[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                var secondMetric = double.Parse(splitResult[2], NumberStyles.Any, CultureInfo.InvariantCulture);

                result.Add((dateTime, firstMetric, secondMetric));
            }

            return result;
        }
    }
}