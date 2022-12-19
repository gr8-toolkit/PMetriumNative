using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using InfluxDB.Client.Writes;
using PMetrium.Native.Common.Helpers;
using PMetrium.Native.Common.Helpers.Extensions;
using PMetrium.Native.Metrics.Android.Contracts;
using Serilog;
using static PMetrium.Native.Common.Helpers.PlatformOSHelper;

namespace PMetrium.Native.Metrics.Android.MetricsHandlers;

internal class HardwareMetricsHandler
{
    private InfluxDBSync _influxDbSync;
    private AndroidDeviceContext _deviceContext;
    private AndroidPerformanceResults _androidPerformanceResults;

    public HardwareMetricsHandler(
        AndroidDeviceContext deviceContext,
        InfluxDBSync influxDbSync,
        AndroidPerformanceResults androidPerformanceResults)
    {
        _influxDbSync = influxDbSync;
        _deviceContext = deviceContext;
        _androidPerformanceResults = androidPerformanceResults;
    }

    public void ExtractAndSaveMetrics(CancellationToken token)
    {
        var device = _deviceContext.DeviceParameters.Device;

        Log.Information($"[Android: {device}] HardwareMetricsHandler - start to handle hardware metrics");

        var tasks = new List<Task>();

        tasks.Add(Task.Run(async () => await ExtractAndSaveCpuMetrics(token)));
        tasks.Add(Task.Run(async () => await ExtractAndSaveRamMetrics(token)));
        tasks.Add(Task.Run(async () => await ExtractAndSaveNetworkMetrics(token)));
        tasks.Add(Task.Run(async () => await ExtractAndSaveBatteryMetrics(token)));
        tasks.Add(Task.Run(async () => await ExtractAndSaveFramesMetrics(token)));

        Task.WaitAll(tasks.ToArray());

        Log.Information($"[Android: {device}] HardwareMetricsHandler - stop to handle hardware metrics");
    }

    private async Task ExtractAndSaveCpuMetrics(CancellationToken token)
    {
        try
        {
            var cpuTotalRaw = (await ExtractDataFromFileOnPhone("cpu_total.txt", token))
                .Replace("%cpu", "", true, CultureInfo.CurrentCulture);
            var cpuUsageTotalRaw = (await ExtractDataFromFileOnPhone("cpu_usage_total.txt", token))
                .Replace("%idle", "", true, CultureInfo.CurrentCulture);
            var cpuUsageAppRaw = await ExtractDataFromFileOnPhone("cpu_usage_app.txt", token);
            var cpuTotal = double.Parse(cpuTotalRaw);
            var points = new List<PointData>();
            var cpuUsageTotalMetrics = ParseOneMetric(cpuUsageTotalRaw.Split("\r\n"));
            var cpuUsageTotalForStatistic = new List<double>();

            foreach (var metric in cpuUsageTotalMetrics)
            {
                var value = Math.Round((cpuTotal - metric.firstMetric) / cpuTotal * 100d, 2);

                points.Add(_influxDbSync.GeneratePoint(
                    "android.cpu.usage.total",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    value,
                    "percentage"));

                cpuUsageTotalForStatistic.Add(value);
            }

            if (cpuUsageTotalForStatistic.Count > 0)
            {
                _androidPerformanceResults.Cpu.TotalCpu_percentage.Avg = Math.Round(cpuUsageTotalForStatistic.Average(), 2);
                _androidPerformanceResults.Cpu.TotalCpu_percentage.Min = cpuUsageTotalForStatistic.Min();
                _androidPerformanceResults.Cpu.TotalCpu_percentage.Max = cpuUsageTotalForStatistic.Max();
                
                _androidPerformanceResults.Cpu.TotalCpu_percentage.P50 = Percentile(cpuUsageTotalForStatistic.ToArray(), 50);
                _androidPerformanceResults.Cpu.TotalCpu_percentage.P75 = Percentile(cpuUsageTotalForStatistic.ToArray(), 75);
                _androidPerformanceResults.Cpu.TotalCpu_percentage.P80 = Percentile(cpuUsageTotalForStatistic.ToArray(), 80);
                _androidPerformanceResults.Cpu.TotalCpu_percentage.P90 = Percentile(cpuUsageTotalForStatistic.ToArray(), 90);
                _androidPerformanceResults.Cpu.TotalCpu_percentage.P95 = Percentile(cpuUsageTotalForStatistic.ToArray(), 95);
                _androidPerformanceResults.Cpu.TotalCpu_percentage.P99 = Percentile(cpuUsageTotalForStatistic.ToArray(), 99);
            }

            var cpuUsageAppMetrics = ParseOneMetric(cpuUsageAppRaw.Split("\r\n"));
            var cpuUsageApplicationForStatistic = new List<double>();

            foreach (var metric in cpuUsageAppMetrics)
            {
                var value = Math.Round(metric.firstMetric / cpuTotal * 100d, 2);

                points.Add(_influxDbSync.GeneratePoint(
                    "android.cpu.usage.app",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    value,
                    "percentage"));

                cpuUsageApplicationForStatistic.Add(value);
            }

            if (cpuUsageApplicationForStatistic.Count > 0)
            {
                _androidPerformanceResults.Cpu.ApplicationCpu_percentage.Avg = Math.Round(cpuUsageApplicationForStatistic.Average(), 2);
                _androidPerformanceResults.Cpu.ApplicationCpu_percentage.Min = cpuUsageApplicationForStatistic.Min();
                _androidPerformanceResults.Cpu.ApplicationCpu_percentage.Max = cpuUsageApplicationForStatistic.Max();
                
                _androidPerformanceResults.Cpu.ApplicationCpu_percentage.P50 = Percentile(cpuUsageApplicationForStatistic.ToArray(), 50);
                _androidPerformanceResults.Cpu.ApplicationCpu_percentage.P75 = Percentile(cpuUsageApplicationForStatistic.ToArray(), 75);
                _androidPerformanceResults.Cpu.ApplicationCpu_percentage.P80 = Percentile(cpuUsageApplicationForStatistic.ToArray(), 80);
                _androidPerformanceResults.Cpu.ApplicationCpu_percentage.P90 = Percentile(cpuUsageApplicationForStatistic.ToArray(), 90);
                _androidPerformanceResults.Cpu.ApplicationCpu_percentage.P95 = Percentile(cpuUsageApplicationForStatistic.ToArray(), 95);
                _androidPerformanceResults.Cpu.ApplicationCpu_percentage.P99 = Percentile(cpuUsageApplicationForStatistic.ToArray(), 99);
            }

            await _influxDbSync.SavePoints(points.ToArray());

            Log.Debug(
                $"[Android: {_deviceContext.DeviceParameters.Device}] HardwareMetricsHandler - stop to handle CPU metrics");
        }
        catch (Exception e)
        {
            Log.Error(
                $"[Android: {_deviceContext.DeviceParameters.Device}] HardwareMetricsHandler - failed to handle CPU metrics. StackTrace: \n {e.Message}\n {e.StackTrace}");
        }
    }

    private async Task ExtractAndSaveRamMetrics(CancellationToken token)
    {
        try
        {
            var ramTotalRaw = await ExtractDataFromFileOnPhone("ram_total.txt", token);
            var ramTotal = double.Parse(ramTotalRaw);
            var ramUsageTotalRaw = await ExtractDataFromFileOnPhone("ram_usage_total.txt", token);
            var ramUsageAppRaw = await ExtractDataFromFileOnPhone("ram_usage_app.txt", token);
            var points = new List<PointData>();

            var ramUsageTotalMetrics = ParseOneMetric(ramUsageTotalRaw.Split("\r\n"));

            if (ramUsageTotalMetrics.Count > 0)
            {
                var ramUsageTotalForStatistic = ramUsageTotalMetrics.Select(x => ramTotal - x.firstMetric * 1024d);
                _androidPerformanceResults.Ram.TotalUsedRam_bytes.Avg = Math.Round(ramUsageTotalForStatistic.Average(), 0);
                _androidPerformanceResults.Ram.TotalUsedRam_bytes.Min = Math.Round(ramUsageTotalForStatistic.Min(), 0);
                _androidPerformanceResults.Ram.TotalUsedRam_bytes.Max = Math.Round(ramUsageTotalForStatistic.Max(), 0);
                
                _androidPerformanceResults.Ram.TotalUsedRam_bytes.P50 = Percentile(ramUsageTotalForStatistic.ToArray(), 50);
                _androidPerformanceResults.Ram.TotalUsedRam_bytes.P75 = Percentile(ramUsageTotalForStatistic.ToArray(), 75);
                _androidPerformanceResults.Ram.TotalUsedRam_bytes.P80 = Percentile(ramUsageTotalForStatistic.ToArray(), 80);
                _androidPerformanceResults.Ram.TotalUsedRam_bytes.P90 = Percentile(ramUsageTotalForStatistic.ToArray(), 90);
                _androidPerformanceResults.Ram.TotalUsedRam_bytes.P95 = Percentile(ramUsageTotalForStatistic.ToArray(), 95);
                _androidPerformanceResults.Ram.TotalUsedRam_bytes.P99 = Percentile(ramUsageTotalForStatistic.ToArray(), 99);
            }

            _androidPerformanceResults.Ram.SystemRam_bytes = ramTotal;

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
                    ramTotal - (metric.firstMetric * 1024d),
                    "byte"));
            }

            var ramUsageAppMetrics = ParseTwoMetrics(ramUsageAppRaw.Split("\r\n"));

            if (ramUsageAppMetrics.Count > 0)
            {
                var ramUsageAppPSSForStatistic = ramUsageAppMetrics.Select(x => Math.Round(x.firstMetric * 1024d, 0));
                _androidPerformanceResults.Ram.ApplicationPSSRam_bytes.Avg = Math.Round(ramUsageAppPSSForStatistic.Average(), 0);
                _androidPerformanceResults.Ram.ApplicationPSSRam_bytes.Min = Math.Round(ramUsageAppPSSForStatistic.Min(), 0);
                _androidPerformanceResults.Ram.ApplicationPSSRam_bytes.Max = ramUsageAppPSSForStatistic.Max();
                
                _androidPerformanceResults.Ram.ApplicationPSSRam_bytes.P50 = Percentile(ramUsageAppPSSForStatistic.ToArray(), 50);
                _androidPerformanceResults.Ram.ApplicationPSSRam_bytes.P75 = Percentile(ramUsageAppPSSForStatistic.ToArray(), 75);
                _androidPerformanceResults.Ram.ApplicationPSSRam_bytes.P80 = Percentile(ramUsageAppPSSForStatistic.ToArray(), 80);
                _androidPerformanceResults.Ram.ApplicationPSSRam_bytes.P90 = Percentile(ramUsageAppPSSForStatistic.ToArray(), 90);
                _androidPerformanceResults.Ram.ApplicationPSSRam_bytes.P95 = Percentile(ramUsageAppPSSForStatistic.ToArray(), 95);
                _androidPerformanceResults.Ram.ApplicationPSSRam_bytes.P99 = Percentile(ramUsageAppPSSForStatistic.ToArray(), 99);

                var ramUsageAppPrivateForStatistic = ramUsageAppMetrics.Select(x => Math.Round(x.secondMetric * 1024d, 0));
                _androidPerformanceResults.Ram.ApplicationPrivateRam_bytes.Avg = Math.Round(ramUsageAppPrivateForStatistic.Average(), 0);
                _androidPerformanceResults.Ram.ApplicationPrivateRam_bytes.Min = Math.Round(ramUsageAppPrivateForStatistic.Min(), 0);
                _androidPerformanceResults.Ram.ApplicationPrivateRam_bytes.Max = Math.Round(ramUsageAppPrivateForStatistic.Max(), 0);
                
                _androidPerformanceResults.Ram.ApplicationPrivateRam_bytes.P50 = Percentile(ramUsageAppPrivateForStatistic.ToArray(), 50);
                _androidPerformanceResults.Ram.ApplicationPrivateRam_bytes.P75 = Percentile(ramUsageAppPrivateForStatistic.ToArray(), 75);
                _androidPerformanceResults.Ram.ApplicationPrivateRam_bytes.P80 = Percentile(ramUsageAppPrivateForStatistic.ToArray(), 80);
                _androidPerformanceResults.Ram.ApplicationPrivateRam_bytes.P90 = Percentile(ramUsageAppPrivateForStatistic.ToArray(), 90);
                _androidPerformanceResults.Ram.ApplicationPrivateRam_bytes.P95 = Percentile(ramUsageAppPrivateForStatistic.ToArray(), 95);
                _androidPerformanceResults.Ram.ApplicationPrivateRam_bytes.P99 = Percentile(ramUsageAppPrivateForStatistic.ToArray(), 99);
            }

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
                $"[Android: {_deviceContext.DeviceParameters.Device}] HardwareMetricsHandler - stop to handle RAM metrics");
        }
        catch (Exception e)
        {
            Log.Error(
                $"[Android: {_deviceContext.DeviceParameters.Device}] HardwareMetricsHandler - failed to handle RAM metrics. StackTrace: \n {e.Message}\n {e.StackTrace}");
        }
    }

    private async Task ExtractAndSaveNetworkMetrics(CancellationToken token)
    {
        try
        {
            var networkUsageTotalRaw = await ExtractDataFromFileOnPhone("network_usage_total.txt", token);
            var networkUsageAppRaw = await ExtractDataFromFileOnPhone("network_usage_app.txt", token);
            var points = new List<PointData>();
            var networkUsageTotalMetrics = ParseFourMetrics(networkUsageTotalRaw.Split("\r\n"));

            if (networkUsageTotalMetrics.Count > 0)
            {
                _androidPerformanceResults.Network.NetworkTotal.MobileTotal.Total.Rx_bytes = networkUsageTotalMetrics.Select(x => x.firstMetric).Max();
                _androidPerformanceResults.Network.NetworkTotal.MobileTotal.Total.Tx_bytes = networkUsageTotalMetrics.Select(x => x.secondMetric).Max();
                _androidPerformanceResults.Network.NetworkTotal.WiFiTotal.Total.Rx_bytes = networkUsageTotalMetrics.Select(x => x.thirdMetric).Max();
                _androidPerformanceResults.Network.NetworkTotal.WiFiTotal.Total.Tx_bytes = networkUsageTotalMetrics.Select(x => x.fourthMetric).Max();
            }

            var previousMetric = (dateTime: DateTime.MinValue, firstMetric: 0d, secondMetric: 0d, thirdMetric: 0d,
                fourthMetric: 0d);

            var networkSpeedTotalMobileRxForStatistic = new List<double>();
            var networkSpeedTotalMobileTxForStatistic = new List<double>();
            var networkSpeedTotalWiFiRxForStatistic = new List<double>();
            var networkSpeedTotalWiFiTxForStatistic = new List<double>();

            foreach (var metric in networkUsageTotalMetrics)
            {
                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.mobile.all.total.tx",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.secondMetric,
                    "byte"));

                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.mobile.all.total.rx",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.firstMetric,
                    "byte"));

                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.wifi.all.total.tx",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.fourthMetric,
                    "byte"));

                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.wifi.all.total.rx",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.thirdMetric,
                    "byte"));

                if (previousMetric.dateTime == DateTime.MinValue)
                {
                    points.Add(_influxDbSync.GeneratePoint(
                        $"android.network.mobile.speed.total.tx",
                        _deviceContext.CommonTags,
                        metric.dateTime,
                        metric.secondMetric,
                        "byte"));

                    networkSpeedTotalMobileTxForStatistic.Add(metric.secondMetric);

                    points.Add(_influxDbSync.GeneratePoint(
                        $"android.network.mobile.speed.total.rx",
                        _deviceContext.CommonTags,
                        metric.dateTime,
                        metric.firstMetric,
                        "byte"));

                    networkSpeedTotalMobileRxForStatistic.Add(metric.firstMetric);

                    points.Add(_influxDbSync.GeneratePoint(
                        $"android.network.wifi.speed.total.tx",
                        _deviceContext.CommonTags,
                        metric.dateTime,
                        metric.fourthMetric,
                        "byte"));

                    networkSpeedTotalWiFiTxForStatistic.Add(metric.fourthMetric);

                    points.Add(_influxDbSync.GeneratePoint(
                        $"android.network.wifi.speed.total.rx",
                        _deviceContext.CommonTags,
                        metric.dateTime,
                        metric.thirdMetric,
                        "byte"));

                    networkSpeedTotalWiFiRxForStatistic.Add(metric.thirdMetric);

                    previousMetric.dateTime = metric.dateTime;
                    previousMetric.firstMetric = metric.firstMetric;
                    previousMetric.secondMetric = metric.secondMetric;
                    previousMetric.thirdMetric = metric.thirdMetric;
                    previousMetric.fourthMetric = metric.fourthMetric;

                    continue;
                }

                var deltaTime = (int)(metric.dateTime - previousMetric.dateTime).TotalSeconds;

                if (deltaTime == 0)
                    continue;

                var speed = Math.Round((metric.secondMetric - previousMetric.secondMetric) / deltaTime, 2);
                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.mobile.speed.total.tx",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    speed,
                    "byte"));
                networkSpeedTotalMobileTxForStatistic.Add(speed);

                speed = Math.Round((metric.firstMetric - previousMetric.firstMetric) / deltaTime, 2);
                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.mobile.speed.total.rx",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    speed,
                    "byte"));
                networkSpeedTotalMobileRxForStatistic.Add(speed);

                speed = Math.Round((metric.fourthMetric - previousMetric.fourthMetric) / deltaTime, 2);
                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.wifi.speed.total.tx",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    speed,
                    "byte"));
                networkSpeedTotalWiFiTxForStatistic.Add(speed);

                speed = Math.Round((metric.thirdMetric - previousMetric.thirdMetric) / deltaTime, 2);
                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.wifi.speed.total.rx",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    speed,
                    "byte"));
                networkSpeedTotalWiFiRxForStatistic.Add(speed);

                previousMetric.dateTime = metric.dateTime;
                previousMetric.firstMetric = metric.firstMetric;
                previousMetric.secondMetric = metric.secondMetric;
                previousMetric.thirdMetric = metric.thirdMetric;
                previousMetric.fourthMetric = metric.fourthMetric;
            }

            if (networkSpeedTotalMobileRxForStatistic.Count > 0)
            {
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Rx_bytes_per_sec.Avg = Math.Round(networkSpeedTotalMobileRxForStatistic.Average(), 0);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Rx_bytes_per_sec.Min = networkSpeedTotalMobileRxForStatistic.Min();
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Rx_bytes_per_sec.Max = networkSpeedTotalMobileRxForStatistic.Max();
                
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Rx_bytes_per_sec.P50 = Percentile(networkSpeedTotalMobileRxForStatistic.ToArray(), 50);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Rx_bytes_per_sec.P75 = Percentile(networkSpeedTotalMobileRxForStatistic.ToArray(), 75);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Rx_bytes_per_sec.P80 = Percentile(networkSpeedTotalMobileRxForStatistic.ToArray(), 80);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Rx_bytes_per_sec.P90 = Percentile(networkSpeedTotalMobileRxForStatistic.ToArray(), 90);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Rx_bytes_per_sec.P95 = Percentile(networkSpeedTotalMobileRxForStatistic.ToArray(), 95);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Rx_bytes_per_sec.P99 = Percentile(networkSpeedTotalMobileRxForStatistic.ToArray(), 99);
            }

            if (networkSpeedTotalMobileTxForStatistic.Count > 0)
            {
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Tx_bytes_per_sec.Avg = Math.Round(networkSpeedTotalMobileTxForStatistic.Average(), 0);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Tx_bytes_per_sec.Min = networkSpeedTotalMobileTxForStatistic.Min();
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Tx_bytes_per_sec.Max = networkSpeedTotalMobileTxForStatistic.Max();
                
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Tx_bytes_per_sec.P50 = Percentile(networkSpeedTotalMobileTxForStatistic.ToArray(), 50);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Tx_bytes_per_sec.P75 = Percentile(networkSpeedTotalMobileTxForStatistic.ToArray(), 75);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Tx_bytes_per_sec.P80 = Percentile(networkSpeedTotalMobileTxForStatistic.ToArray(), 80);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Tx_bytes_per_sec.P90 = Percentile(networkSpeedTotalMobileTxForStatistic.ToArray(), 90);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Tx_bytes_per_sec.P95 = Percentile(networkSpeedTotalMobileTxForStatistic.ToArray(), 95);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Total.Tx_bytes_per_sec.P99 = Percentile(networkSpeedTotalMobileTxForStatistic.ToArray(), 99);
            }

            if (networkSpeedTotalWiFiRxForStatistic.Count > 0)
            {
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Rx_bytes_per_sec.Avg = Math.Round(networkSpeedTotalWiFiRxForStatistic.Average(), 0);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Rx_bytes_per_sec.Min = networkSpeedTotalWiFiRxForStatistic.Min();
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Rx_bytes_per_sec.Max = networkSpeedTotalWiFiRxForStatistic.Max();
                
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Rx_bytes_per_sec.P50 = Percentile(networkSpeedTotalWiFiRxForStatistic.ToArray(), 50);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Rx_bytes_per_sec.P75 = Percentile(networkSpeedTotalWiFiRxForStatistic.ToArray(), 75);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Rx_bytes_per_sec.P80 = Percentile(networkSpeedTotalWiFiRxForStatistic.ToArray(), 80);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Rx_bytes_per_sec.P90 = Percentile(networkSpeedTotalWiFiRxForStatistic.ToArray(), 90);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Rx_bytes_per_sec.P95 = Percentile(networkSpeedTotalWiFiRxForStatistic.ToArray(), 95);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Rx_bytes_per_sec.P99 = Percentile(networkSpeedTotalWiFiRxForStatistic.ToArray(), 99);
            }

            if (networkSpeedTotalWiFiTxForStatistic.Count > 0)
            {
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Tx_bytes_per_sec.Avg = Math.Round(networkSpeedTotalWiFiTxForStatistic.Average(), 0);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Tx_bytes_per_sec.Min = networkSpeedTotalWiFiTxForStatistic.Min();
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Tx_bytes_per_sec.Max = networkSpeedTotalWiFiTxForStatistic.Max();
                
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Tx_bytes_per_sec.P50 = Percentile(networkSpeedTotalWiFiTxForStatistic.ToArray(), 50);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Tx_bytes_per_sec.P75 = Percentile(networkSpeedTotalWiFiTxForStatistic.ToArray(), 75);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Tx_bytes_per_sec.P80 = Percentile(networkSpeedTotalWiFiTxForStatistic.ToArray(), 80);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Tx_bytes_per_sec.P90 = Percentile(networkSpeedTotalWiFiTxForStatistic.ToArray(), 90);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Tx_bytes_per_sec.P95 = Percentile(networkSpeedTotalWiFiTxForStatistic.ToArray(), 95);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Total.Tx_bytes_per_sec.P99 = Percentile(networkSpeedTotalWiFiTxForStatistic.ToArray(), 99);
            }

            var networkUsageAppMetrics = ParseFourMetrics(networkUsageAppRaw.Split("\r\n"));
            previousMetric = (dateTime: DateTime.MinValue, firstMetric: 0d, secondMetric: 0d, thirdMetric: 0d,
                fourthMetric: 0d);

            if (networkUsageAppMetrics.Count > 0)
            {
                _androidPerformanceResults.Network.NetworkTotal.MobileTotal.Application.Rx_bytes = networkUsageAppMetrics.Select(x => x.firstMetric).Max();
                _androidPerformanceResults.Network.NetworkTotal.MobileTotal.Application.Tx_bytes = networkUsageAppMetrics.Select(x => x.secondMetric).Max();
                _androidPerformanceResults.Network.NetworkTotal.WiFiTotal.Application.Rx_bytes = networkUsageAppMetrics.Select(x => x.thirdMetric).Max();
                _androidPerformanceResults.Network.NetworkTotal.WiFiTotal.Application.Tx_bytes = networkUsageAppMetrics.Select(x => x.fourthMetric).Max();
            }

            var networkSpeedApplicationMobileRxForStatistic = new List<double>();
            var networkSpeedApplicationMobileTxForStatistic = new List<double>();
            var networkSpeedApplicationWiFiRxForStatistic = new List<double>();
            var networkSpeedApplicationWiFiTxForStatistic = new List<double>();

            foreach (var metric in networkUsageAppMetrics)
            {
                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.mobile.all.app.tx",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.secondMetric,
                    "byte"));

                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.mobile.all.app.rx",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.firstMetric,
                    "byte"));

                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.wifi.all.app.tx",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.fourthMetric,
                    "byte"));

                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.wifi.all.app.rx",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    metric.thirdMetric,
                    "byte"));

                if (previousMetric.dateTime == DateTime.MinValue)
                {
                    points.Add(_influxDbSync.GeneratePoint(
                        $"android.network.mobile.speed.app.tx",
                        _deviceContext.CommonTags,
                        metric.dateTime,
                        metric.secondMetric,
                        "byte"));

                    networkSpeedApplicationMobileTxForStatistic.Add(metric.secondMetric);

                    points.Add(_influxDbSync.GeneratePoint(
                        $"android.network.mobile.speed.app.rx",
                        _deviceContext.CommonTags,
                        metric.dateTime,
                        metric.firstMetric,
                        "byte"));

                    networkSpeedApplicationMobileRxForStatistic.Add(metric.firstMetric);

                    points.Add(_influxDbSync.GeneratePoint(
                        $"android.network.wifi.speed.app.tx",
                        _deviceContext.CommonTags,
                        metric.dateTime,
                        metric.fourthMetric,
                        "byte"));

                    networkSpeedApplicationWiFiTxForStatistic.Add(metric.fourthMetric);

                    points.Add(_influxDbSync.GeneratePoint(
                        $"android.network.wifi.speed.app.rx",
                        _deviceContext.CommonTags,
                        metric.dateTime,
                        metric.thirdMetric,
                        "byte"));

                    networkSpeedApplicationWiFiRxForStatistic.Add(metric.thirdMetric);

                    previousMetric.dateTime = metric.dateTime;
                    previousMetric.firstMetric = metric.firstMetric;
                    previousMetric.secondMetric = metric.secondMetric;
                    previousMetric.thirdMetric = metric.thirdMetric;
                    previousMetric.fourthMetric = metric.fourthMetric;

                    continue;
                }

                var deltaTime = (int)(metric.dateTime - previousMetric.dateTime).TotalSeconds;

                if (deltaTime == 0)
                    continue;

                var speed = Math.Round((metric.secondMetric - previousMetric.secondMetric) / deltaTime, 2);
                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.mobile.speed.app.tx",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    speed,
                    "byte"));
                networkSpeedApplicationMobileTxForStatistic.Add(speed);

                speed = Math.Round((metric.firstMetric - previousMetric.firstMetric) / deltaTime, 2);
                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.mobile.speed.app.rx",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    speed,
                    "byte"));
                networkSpeedApplicationMobileRxForStatistic.Add(speed);

                speed = Math.Round((metric.fourthMetric - previousMetric.fourthMetric) / deltaTime, 2);
                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.wifi.speed.app.tx",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    speed,
                    "byte"));
                networkSpeedApplicationWiFiTxForStatistic.Add(speed);

                speed = Math.Round((metric.thirdMetric - previousMetric.thirdMetric) / deltaTime, 2);
                points.Add(_influxDbSync.GeneratePoint(
                    $"android.network.wifi.speed.app.rx",
                    _deviceContext.CommonTags,
                    metric.dateTime,
                    speed,
                    "byte"));
                networkSpeedApplicationWiFiRxForStatistic.Add(speed);

                previousMetric.dateTime = metric.dateTime;
                previousMetric.firstMetric = metric.firstMetric;
                previousMetric.secondMetric = metric.secondMetric;
                previousMetric.thirdMetric = metric.thirdMetric;
                previousMetric.fourthMetric = metric.fourthMetric;
            }

            if (networkSpeedApplicationMobileRxForStatistic.Count > 0)
            {
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Rx_bytes_per_sec.Avg = Math.Round(networkSpeedApplicationMobileRxForStatistic.Average(), 0);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Rx_bytes_per_sec.Min = networkSpeedApplicationMobileRxForStatistic.Min();
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Rx_bytes_per_sec.Max = networkSpeedApplicationMobileRxForStatistic.Max();
                
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Rx_bytes_per_sec.P50 = Percentile(networkSpeedApplicationMobileRxForStatistic.ToArray(), 50);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Rx_bytes_per_sec.P75 = Percentile(networkSpeedApplicationMobileRxForStatistic.ToArray(), 75);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Rx_bytes_per_sec.P80 = Percentile(networkSpeedApplicationMobileRxForStatistic.ToArray(), 80);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Rx_bytes_per_sec.P90 = Percentile(networkSpeedApplicationMobileRxForStatistic.ToArray(), 90);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Rx_bytes_per_sec.P95 = Percentile(networkSpeedApplicationMobileRxForStatistic.ToArray(), 95);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Rx_bytes_per_sec.P99 = Percentile(networkSpeedApplicationMobileRxForStatistic.ToArray(), 99);
            }

            if (networkSpeedApplicationMobileTxForStatistic.Count > 0)
            {
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Tx_bytes_per_sec.Avg = Math.Round(networkSpeedApplicationMobileTxForStatistic.Average(), 0);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Tx_bytes_per_sec.Min = networkSpeedApplicationMobileTxForStatistic.Min();
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Tx_bytes_per_sec.Max = networkSpeedApplicationMobileTxForStatistic.Max();
                
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Tx_bytes_per_sec.P50 = Percentile(networkSpeedApplicationMobileTxForStatistic.ToArray(), 50);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Tx_bytes_per_sec.P75 = Percentile(networkSpeedApplicationMobileTxForStatistic.ToArray(), 75);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Tx_bytes_per_sec.P80 = Percentile(networkSpeedApplicationMobileTxForStatistic.ToArray(), 80);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Tx_bytes_per_sec.P90 = Percentile(networkSpeedApplicationMobileTxForStatistic.ToArray(), 90);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Tx_bytes_per_sec.P95 = Percentile(networkSpeedApplicationMobileTxForStatistic.ToArray(), 95);
                _androidPerformanceResults.Network.NetworkSpeed.MobileSpeed.Application.Tx_bytes_per_sec.P99 = Percentile(networkSpeedApplicationMobileTxForStatistic.ToArray(), 99);
            }

            if (networkSpeedApplicationWiFiRxForStatistic.Count > 0)
            {
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Rx_bytes_per_sec.Avg = Math.Round(networkSpeedApplicationWiFiRxForStatistic.Average(), 0);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Rx_bytes_per_sec.Min = networkSpeedApplicationWiFiRxForStatistic.Min();
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Rx_bytes_per_sec.Max = networkSpeedApplicationWiFiRxForStatistic.Max();
                
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Rx_bytes_per_sec.P50 = Percentile(networkSpeedApplicationWiFiRxForStatistic.ToArray(), 50);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Rx_bytes_per_sec.P75 = Percentile(networkSpeedApplicationWiFiRxForStatistic.ToArray(), 75);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Rx_bytes_per_sec.P80 = Percentile(networkSpeedApplicationWiFiRxForStatistic.ToArray(), 80);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Rx_bytes_per_sec.P90 = Percentile(networkSpeedApplicationWiFiRxForStatistic.ToArray(), 90);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Rx_bytes_per_sec.P95 = Percentile(networkSpeedApplicationWiFiRxForStatistic.ToArray(), 95);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Rx_bytes_per_sec.P99 = Percentile(networkSpeedApplicationWiFiRxForStatistic.ToArray(), 99);
            }

            if (networkSpeedApplicationWiFiTxForStatistic.Count > 0)
            {
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Tx_bytes_per_sec.Avg = Math.Round(networkSpeedApplicationWiFiTxForStatistic.Average(), 0);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Tx_bytes_per_sec.Min = networkSpeedApplicationWiFiTxForStatistic.Min();
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Tx_bytes_per_sec.Max = networkSpeedApplicationWiFiTxForStatistic.Max();
                
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Tx_bytes_per_sec.P50 = Percentile(networkSpeedApplicationWiFiTxForStatistic.ToArray(), 50);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Tx_bytes_per_sec.P75 = Percentile(networkSpeedApplicationWiFiTxForStatistic.ToArray(), 75);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Tx_bytes_per_sec.P80 = Percentile(networkSpeedApplicationWiFiTxForStatistic.ToArray(), 80);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Tx_bytes_per_sec.P90 = Percentile(networkSpeedApplicationWiFiTxForStatistic.ToArray(), 90);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Tx_bytes_per_sec.P95 = Percentile(networkSpeedApplicationWiFiTxForStatistic.ToArray(), 95);
                _androidPerformanceResults.Network.NetworkSpeed.WiFiSpeed.Application.Tx_bytes_per_sec.P99 = Percentile(networkSpeedApplicationWiFiTxForStatistic.ToArray(), 99);
            }

            await _influxDbSync.SavePoints(points.ToArray());

            Log.Debug(
                $"[Android: {_deviceContext.DeviceParameters.Device}] HardwareMetricsHandler - stop to handle NETWORK metrics");
        }
        catch (Exception e)
        {
            Log.Error(
                $"[Android: {_deviceContext.DeviceParameters.Device}] HardwareMetricsHandler - failed to handle NETWORK metrics. StackTrace: \n {e.Message}\n {e.StackTrace}");
        }
    }

    private async Task ExtractAndSaveBatteryMetrics(CancellationToken token)
    {
        try
        {
            var batteryUsageAppRaw = await ExtractDataFromFileOnPhone("battery_app.txt", token);
            var batteryUsageAppMetrics = ParseOneMetric(batteryUsageAppRaw.Split("\r\n"));
            var points = new List<PointData>();

            if (batteryUsageAppMetrics.Count > 0)
            {
                var batteryUsageAppForStatistic = batteryUsageAppMetrics.Select(x => x.firstMetric);
                _androidPerformanceResults.Battery.Application_mAh = batteryUsageAppForStatistic.Max();
            }

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
                $"[Android: {_deviceContext.DeviceParameters.Device}] HardwareMetricsHandler - stop to handle BATTERY metrics");
        }
        catch (Exception e)
        {
            Log.Error(
                $"[Android: {_deviceContext.DeviceParameters.Device}] HardwareMetricsHandler - failed to handle BATTERY metrics. StackTrace: \n {e.Message}\n {e.StackTrace}");
        }
    }

    private async Task ExtractAndSaveFramesMetrics(CancellationToken token)
    {
        try
        {
            var framesAppRaw = await ExtractDataFromFileOnPhone("frames_app.txt", token);
            var framesAppMetrics = ParseFramesMetrics(framesAppRaw
                .Split(new[] { "Total" }, StringSplitOptions.RemoveEmptyEntries));
            var points = new List<PointData>();

            if (framesAppMetrics.Count > 0)
            {
                var renderedFramesAppForStatistic = framesAppMetrics.Select(x => x.firstMetric);
                _androidPerformanceResults.Frames.ApplicationRenderedFrames = renderedFramesAppForStatistic.Max();

                var jankyFramesAppForStatistic = framesAppMetrics.Select(x => x.secondMetric);
                _androidPerformanceResults.Frames.ApplicationJankyFrames = jankyFramesAppForStatistic.Max();
            }

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
                $"[Android: {_deviceContext.DeviceParameters.Device}] HardwareMetricsHandler - stop to handle FRAMES metrics");
        }
        catch (Exception e)
        {
            Log.Error(
                $"[Android: {_deviceContext.DeviceParameters.Device}] HardwareMetricsHandler - failed to handle FRAMES metrics. StackTrace: \n {e.Message}\n {e.StackTrace}");
        }
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
                ? CreateProcess($"{WorkingDirectory}\\Scripts\\Bat\\readFile.bat",
                    $"{_deviceContext.DeviceParameters.Device} {fileName}")
                : CreateProcess($"{WorkingDirectory}/Scripts/Shell/readFile.sh",
                    $"{_deviceContext.DeviceParameters.Device} {fileName}");

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

    private List<(DateTime dateTime, double firstMetric, double secondMetric, double thirdMetric, double
        fourthMetric
        )> ParseFourMetrics(
        string[] metricsRaw)
    {
        var result =
            new List<(DateTime dateTime, double firstMetric, double secondMetric, double thirdMetric, double
                fourthMetric)>();

        foreach (var line in metricsRaw)
        {
            var splitResult = line.Split("_");

            if (splitResult.Length != 5) continue;

            var dateTime = DateTime.UnixEpoch.AddSeconds(long.Parse(splitResult[0]));
            var firstMetric = double.Parse(splitResult[1], NumberStyles.Any, CultureInfo.InvariantCulture);
            var secondMetric = double.Parse(splitResult[2], NumberStyles.Any, CultureInfo.InvariantCulture);
            var thirdMetric = double.Parse(splitResult[3], NumberStyles.Any, CultureInfo.InvariantCulture);
            var fourthMetric = double.Parse(splitResult[4], NumberStyles.Any, CultureInfo.InvariantCulture);

            result.Add((dateTime, firstMetric, secondMetric, thirdMetric, fourthMetric));
        }

        return result;
    }

    T Percentile<T>(T[] sequence, int percentile)
    {
        Array.Sort(sequence);
        var index = Math.Ceiling(percentile / 100d * sequence.Length) - 1;
        return sequence[(int)index];
    }
}