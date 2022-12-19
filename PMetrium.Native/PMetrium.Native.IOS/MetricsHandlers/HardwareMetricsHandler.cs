using System.Xml;
using InfluxDB.Client.Writes;
using PMetrium.Native.Common.Helpers;
using PMetrium.Native.IOS.Contracts;
using Serilog;
using TestXml.Contracts;
using TestXml.Contracts.Enums;

namespace PMetrium.Native.IOS.MetricsHandlers;

internal class HardwareMetricsHandler
{
    private InfluxDBSync _influxDbSync;
    private IOSDeviceContext _deviceContext;
    private IOSPerformanceResults _iosPerformanceResults;

    public HardwareMetricsHandler(
        IOSDeviceContext deviceContext,
        InfluxDBSync influxDbSync,
        IOSPerformanceResults iosPerformanceResults)
    {
        _influxDbSync = influxDbSync;
        _deviceContext = deviceContext;
        _iosPerformanceResults = iosPerformanceResults;
    }

    public void ExtractAndSaveMetrics(CancellationToken token)
    {
         var device = _deviceContext.DeviceParameters.Device;
        
         Log.Information($"[IOS: {device}] HardwareMetricsHandler - start to handle hardware metrics");
        
         var tasks = new List<Task>();
        
         tasks.Add(Task.Run(async () => await ExtractAndSaveSystemMetrics(token)));
         tasks.Add(Task.Run(async () => await ExtractAndSaveFpsMetrics(token)));
         tasks.Add(Task.Run(async () => await ExtractAndSaveProcessNetworkMetrics(token)));
         tasks.Add(Task.Run(async () => await ExtractAndSaveProcessMetrics(token)));
        
         Task.WaitAll(tasks.ToArray());
        
        Log.Information($"[IOS: {device}] HardwareMetricsHandler - stop to handle hardware metrics");
    }

    private async Task ExtractAndSaveSystemMetrics(CancellationToken token)
    {
        var device = _deviceContext.DeviceParameters.Device;
        
        try
        {
            var xmlNodeList = GetXmlNodeList($"system-{device}.xml");
            var metrics = ExtractMetrics<SystemMetrics>(xmlNodeList!);
            var points = new List<PointData>();

            var cpuStatistic = new List<double>();
            var ramStatistic = new List<long>();
            var diskReadBytesPerSecStatistic = new List<long>();
            var diskWrittenBytesPerSecStatistic = new List<long>();
            var diskReadsPerSecondStatistic = new List<double>();
            var diskWritesPerSecondStatistic = new List<double>();
            var networkDataReceivedPerSecondStatistic = new List<long>();
            var networkDataSentPerSecondStatistic = new List<long>();
            var networkPacketsInPerSecondStatistic = new List<double>();
            var networkPacketsOutPerSecondStatistic = new List<double>();

            foreach (var metric in metrics)
            {
                var timestamp = GetTimestamp(metric.First(x => x.MetricType == SystemMetrics.StartTime).MetricData.Text);
                var rawCpu = metric.Find(x => x.MetricType == SystemMetrics.TotalCpuLoad)?.MetricData.Text;

                if (rawCpu != null)
                {
                    var cpu = Math.Round(Double.Parse(rawCpu), 2);
                    cpuStatistic.Add(cpu);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.system.cpu",
                        _deviceContext.CommonTags,
                        timestamp,
                        cpu,
                        "percentage"));
                }

                var ramRaw = metric.Find(x => x.MetricType == SystemMetrics.TotalMemoryUsed)?.MetricData.Text;

                if (ramRaw != null)
                {
                    var ram = long.Parse(ramRaw);
                    ramStatistic.Add(ram);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.system.ram",
                        _deviceContext.CommonTags,
                        timestamp,
                        ram,
                        "bytes"));
                }

                var diskReadBytesPerSecRaw =
                    metric.Find(x => x.MetricType == SystemMetrics.DiskDataReadPerSecond)?.MetricData.Text;

                if (diskReadBytesPerSecRaw != null)
                {
                    var diskReadBytesPerSec = long.Parse(diskReadBytesPerSecRaw);
                    diskReadBytesPerSecStatistic.Add(diskReadBytesPerSec);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.system.disk.data.read",
                        _deviceContext.CommonTags,
                        timestamp,
                        diskReadBytesPerSec,
                        "bytes"));
                }
                
                var diskWrittenBytesPerSecRaw =
                    metric.Find(x => x.MetricType == SystemMetrics.DiskDataWrittenPerSecond)?.MetricData.Text;

                if (diskWrittenBytesPerSecRaw != null)
                {
                    var diskWrittenBytesPerSec = long.Parse(diskWrittenBytesPerSecRaw);
                    diskWrittenBytesPerSecStatistic.Add(diskWrittenBytesPerSec);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.system.disk.data.written",
                        _deviceContext.CommonTags,
                        timestamp,
                        diskWrittenBytesPerSec,
                        "bytes"));
                }
                
                var diskReadsPerSecondRaw =
                    metric.Find(x => x.MetricType == SystemMetrics.DiskReadsPerSecond)?.MetricData.Text;

                if (diskReadsPerSecondRaw != null)
                {
                    var diskReadsPerSecond = Math.Round(Double.Parse(diskReadsPerSecondRaw), 2);
                    diskReadsPerSecondStatistic.Add(diskReadsPerSecond);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.system.disk.ops.read",
                        _deviceContext.CommonTags,
                        timestamp,
                        diskReadsPerSecond,
                        "count"));
                }
                
                var diskWrittenPerSecondRaw =
                    metric.Find(x => x.MetricType == SystemMetrics.DiskWritesPerSecond)?.MetricData.Text;

                if (diskWrittenPerSecondRaw != null)
                {
                    var diskWrittenPerSecond = Math.Round(Double.Parse(diskWrittenPerSecondRaw), 2);
                    diskWritesPerSecondStatistic.Add(diskWrittenPerSecond);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.system.disk.ops.written",
                        _deviceContext.CommonTags,
                        timestamp,
                        diskWrittenPerSecond,
                        "count"));
                }
                
                var networkDataReceivedPerSecondRaw =
                    metric.Find(x => x.MetricType == SystemMetrics.NetworkDataReceivedPerSecond)?.MetricData.Text;

                if (networkDataReceivedPerSecondRaw != null)
                {
                    var networkDataReceivedPerSecond = long.Parse(networkDataReceivedPerSecondRaw);
                    networkDataReceivedPerSecondStatistic.Add(networkDataReceivedPerSecond);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.system.network.data.read",
                        _deviceContext.CommonTags,
                        timestamp,
                        networkDataReceivedPerSecond,
                        "bytes"));
                }
                
                var networkDataSentPerSecondRaw =
                    metric.Find(x => x.MetricType == SystemMetrics.NetworkDataSentPerSecond)?.MetricData.Text;

                if (networkDataSentPerSecondRaw != null)
                {
                    var networkDataSentPerSecond = long.Parse(networkDataSentPerSecondRaw);
                    networkDataSentPerSecondStatistic.Add(networkDataSentPerSecond);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.system.network.data.sent",
                        _deviceContext.CommonTags,
                        timestamp,
                        networkDataSentPerSecond,
                        "bytes"));
                }
                
                var networkPacketsInPerSecondRaw =
                    metric.Find(x => x.MetricType == SystemMetrics.NetworkPacketsInPerSecond)?.MetricData.Text;

                if (networkPacketsInPerSecondRaw != null)
                {
                    var networkPacketsInPerSecond = Math.Round(double.Parse(networkPacketsInPerSecondRaw), 2);
                    networkPacketsInPerSecondStatistic.Add(networkPacketsInPerSecond);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.system.network.packets.in",
                        _deviceContext.CommonTags,
                        timestamp,
                        networkPacketsInPerSecond,
                        "count"));
                }
                
                var networkPacketsOutPerSecondRaw =
                    metric.Find(x => x.MetricType == SystemMetrics.NetworkPacketsOutPerSecond)?.MetricData.Text;

                if (networkPacketsOutPerSecondRaw != null)
                {
                    var networkPacketsOutPerSecond = Math.Round(double.Parse(networkPacketsOutPerSecondRaw), 2);
                    networkPacketsOutPerSecondStatistic.Add(networkPacketsOutPerSecond);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.system.network.packets.out",
                        _deviceContext.CommonTags,
                        timestamp,
                        networkPacketsOutPerSecond,
                        "count"));
                }
            }

            if (cpuStatistic.Count > 0)
            {
                _iosPerformanceResults.System.Cpu.UsedCpu_percentage.Avg = Math.Round(cpuStatistic.Average(), 2);
                _iosPerformanceResults.System.Cpu.UsedCpu_percentage.Max = Math.Round(cpuStatistic.Max(), 2);
                _iosPerformanceResults.System.Cpu.UsedCpu_percentage.Min = Math.Round(cpuStatistic.Min(), 2);
                
                _iosPerformanceResults.System.Cpu.UsedCpu_percentage.P50 = Percentile(cpuStatistic.ToArray(), 50);
                _iosPerformanceResults.System.Cpu.UsedCpu_percentage.P75 = Percentile(cpuStatistic.ToArray(), 75);
                _iosPerformanceResults.System.Cpu.UsedCpu_percentage.P80 = Percentile(cpuStatistic.ToArray(), 80);
                _iosPerformanceResults.System.Cpu.UsedCpu_percentage.P90 = Percentile(cpuStatistic.ToArray(), 90);
                _iosPerformanceResults.System.Cpu.UsedCpu_percentage.P95 = Percentile(cpuStatistic.ToArray(), 95);
                _iosPerformanceResults.System.Cpu.UsedCpu_percentage.P99 = Percentile(cpuStatistic.ToArray(), 99);
            }
            
            if (ramStatistic.Count > 0)
            {
                _iosPerformanceResults.System.Ram.UsedRam_bytes.Avg = Math.Round(ramStatistic.Average(), 0);
                _iosPerformanceResults.System.Ram.UsedRam_bytes.Max = ramStatistic.Max();
                _iosPerformanceResults.System.Ram.UsedRam_bytes.Min = ramStatistic.Min();
                
                _iosPerformanceResults.System.Ram.UsedRam_bytes.P50 = Percentile(ramStatistic.ToArray(), 50);
                _iosPerformanceResults.System.Ram.UsedRam_bytes.P75 = Percentile(ramStatistic.ToArray(), 75);
                _iosPerformanceResults.System.Ram.UsedRam_bytes.P80 = Percentile(ramStatistic.ToArray(), 80);
                _iosPerformanceResults.System.Ram.UsedRam_bytes.P90 = Percentile(ramStatistic.ToArray(), 90);
                _iosPerformanceResults.System.Ram.UsedRam_bytes.P95 = Percentile(ramStatistic.ToArray(), 95);
                _iosPerformanceResults.System.Ram.UsedRam_bytes.P99 = Percentile(ramStatistic.ToArray(), 99);
            }
            
            if (diskReadBytesPerSecStatistic.Count > 0)
            {
                _iosPerformanceResults.System.Disk.DataRead_bytes_per_sec.Avg = Math.Round(diskReadBytesPerSecStatistic.Average(), 0);
                _iosPerformanceResults.System.Disk.DataRead_bytes_per_sec.Max = diskReadBytesPerSecStatistic.Max();
                _iosPerformanceResults.System.Disk.DataRead_bytes_per_sec.Min = diskReadBytesPerSecStatistic.Min();
                
                _iosPerformanceResults.System.Disk.DataRead_bytes_per_sec.P50 = Percentile(diskReadBytesPerSecStatistic.ToArray(), 50);
                _iosPerformanceResults.System.Disk.DataRead_bytes_per_sec.P75 = Percentile(diskReadBytesPerSecStatistic.ToArray(), 75);
                _iosPerformanceResults.System.Disk.DataRead_bytes_per_sec.P80 = Percentile(diskReadBytesPerSecStatistic.ToArray(), 80);
                _iosPerformanceResults.System.Disk.DataRead_bytes_per_sec.P90 = Percentile(diskReadBytesPerSecStatistic.ToArray(), 90);
                _iosPerformanceResults.System.Disk.DataRead_bytes_per_sec.P95 = Percentile(diskReadBytesPerSecStatistic.ToArray(), 95);
                _iosPerformanceResults.System.Disk.DataRead_bytes_per_sec.P99 = Percentile(diskReadBytesPerSecStatistic.ToArray(), 99);
            }
            
            if (diskWrittenBytesPerSecStatistic.Count > 0)
            {
                _iosPerformanceResults.System.Disk.DataWritten_bytes_per_sec.Avg = Math.Round(diskWrittenBytesPerSecStatistic.Average(), 0);
                _iosPerformanceResults.System.Disk.DataWritten_bytes_per_sec.Max = diskWrittenBytesPerSecStatistic.Max();
                _iosPerformanceResults.System.Disk.DataWritten_bytes_per_sec.Min = diskWrittenBytesPerSecStatistic.Min();
                
                _iosPerformanceResults.System.Disk.DataWritten_bytes_per_sec.P50 = Percentile(diskWrittenBytesPerSecStatistic.ToArray(), 50);
                _iosPerformanceResults.System.Disk.DataWritten_bytes_per_sec.P75 = Percentile(diskWrittenBytesPerSecStatistic.ToArray(), 75);
                _iosPerformanceResults.System.Disk.DataWritten_bytes_per_sec.P80 = Percentile(diskWrittenBytesPerSecStatistic.ToArray(), 80);
                _iosPerformanceResults.System.Disk.DataWritten_bytes_per_sec.P90 = Percentile(diskWrittenBytesPerSecStatistic.ToArray(), 90);
                _iosPerformanceResults.System.Disk.DataWritten_bytes_per_sec.P95 = Percentile(diskWrittenBytesPerSecStatistic.ToArray(), 95);
                _iosPerformanceResults.System.Disk.DataWritten_bytes_per_sec.P99 = Percentile(diskWrittenBytesPerSecStatistic.ToArray(), 99);
            }
            
            if (diskReadsPerSecondStatistic.Count > 0)
            {
                _iosPerformanceResults.System.Disk.ReadsIn_per_sec.Avg = Math.Round(diskReadsPerSecondStatistic.Average(), 2);
                _iosPerformanceResults.System.Disk.ReadsIn_per_sec.Max = Math.Round(diskReadsPerSecondStatistic.Max(), 2);
                _iosPerformanceResults.System.Disk.ReadsIn_per_sec.Min = Math.Round(diskReadsPerSecondStatistic.Min(), 2);
                
                _iosPerformanceResults.System.Disk.ReadsIn_per_sec.P50 = Percentile(diskReadsPerSecondStatistic.ToArray(), 50);
                _iosPerformanceResults.System.Disk.ReadsIn_per_sec.P75 = Percentile(diskReadsPerSecondStatistic.ToArray(), 75);
                _iosPerformanceResults.System.Disk.ReadsIn_per_sec.P80 = Percentile(diskReadsPerSecondStatistic.ToArray(), 80);
                _iosPerformanceResults.System.Disk.ReadsIn_per_sec.P90 = Percentile(diskReadsPerSecondStatistic.ToArray(), 90);
                _iosPerformanceResults.System.Disk.ReadsIn_per_sec.P95 = Percentile(diskReadsPerSecondStatistic.ToArray(), 95);
                _iosPerformanceResults.System.Disk.ReadsIn_per_sec.P99 = Percentile(diskReadsPerSecondStatistic.ToArray(), 99);
            }
            
            if (diskWritesPerSecondStatistic.Count > 0)
            {
                _iosPerformanceResults.System.Disk.WritesOut_per_sec.Avg = Math.Round(diskWritesPerSecondStatistic.Average(), 2);
                _iosPerformanceResults.System.Disk.WritesOut_per_sec.Max = Math.Round(diskWritesPerSecondStatistic.Max(), 2);
                _iosPerformanceResults.System.Disk.WritesOut_per_sec.Min = Math.Round(diskWritesPerSecondStatistic.Min(), 2);
                
                _iosPerformanceResults.System.Disk.WritesOut_per_sec.P50 = Percentile(diskWritesPerSecondStatistic.ToArray(), 50);
                _iosPerformanceResults.System.Disk.WritesOut_per_sec.P75 = Percentile(diskWritesPerSecondStatistic.ToArray(), 75);
                _iosPerformanceResults.System.Disk.WritesOut_per_sec.P80 = Percentile(diskWritesPerSecondStatistic.ToArray(), 80);
                _iosPerformanceResults.System.Disk.WritesOut_per_sec.P90 = Percentile(diskWritesPerSecondStatistic.ToArray(), 90);
                _iosPerformanceResults.System.Disk.WritesOut_per_sec.P95 = Percentile(diskWritesPerSecondStatistic.ToArray(), 95);
                _iosPerformanceResults.System.Disk.WritesOut_per_sec.P99 = Percentile(diskWritesPerSecondStatistic.ToArray(), 99);
            }
            
            if (networkDataReceivedPerSecondStatistic.Count > 0)
            {
                _iosPerformanceResults.System.SystemNetwork.DataReceived_bytes_per_sec.Avg = Math.Round(networkDataReceivedPerSecondStatistic.Average(), 0);
                _iosPerformanceResults.System.SystemNetwork.DataReceived_bytes_per_sec.Max = networkDataReceivedPerSecondStatistic.Max();
                _iosPerformanceResults.System.SystemNetwork.DataReceived_bytes_per_sec.Min = networkDataReceivedPerSecondStatistic.Min();
                
                _iosPerformanceResults.System.SystemNetwork.DataReceived_bytes_per_sec.P50 = Percentile(networkDataReceivedPerSecondStatistic.ToArray(), 50);
                _iosPerformanceResults.System.SystemNetwork.DataReceived_bytes_per_sec.P75 = Percentile(networkDataReceivedPerSecondStatistic.ToArray(), 75);
                _iosPerformanceResults.System.SystemNetwork.DataReceived_bytes_per_sec.P80 = Percentile(networkDataReceivedPerSecondStatistic.ToArray(), 80);
                _iosPerformanceResults.System.SystemNetwork.DataReceived_bytes_per_sec.P90 = Percentile(networkDataReceivedPerSecondStatistic.ToArray(), 90);
                _iosPerformanceResults.System.SystemNetwork.DataReceived_bytes_per_sec.P95 = Percentile(networkDataReceivedPerSecondStatistic.ToArray(), 95);
                _iosPerformanceResults.System.SystemNetwork.DataReceived_bytes_per_sec.P99 = Percentile(networkDataReceivedPerSecondStatistic.ToArray(), 99);
            }
            
            if (networkDataSentPerSecondStatistic.Count > 0)
            {
                _iosPerformanceResults.System.SystemNetwork.DataSent_bytes_per_sec.Avg = Math.Round(networkDataSentPerSecondStatistic.Average(), 0);
                _iosPerformanceResults.System.SystemNetwork.DataSent_bytes_per_sec.Max = networkDataSentPerSecondStatistic.Max();
                _iosPerformanceResults.System.SystemNetwork.DataSent_bytes_per_sec.Min = networkDataSentPerSecondStatistic.Min();
                
                _iosPerformanceResults.System.SystemNetwork.DataSent_bytes_per_sec.P50 = Percentile(networkDataSentPerSecondStatistic.ToArray(), 50);
                _iosPerformanceResults.System.SystemNetwork.DataSent_bytes_per_sec.P75 = Percentile(networkDataSentPerSecondStatistic.ToArray(), 75);
                _iosPerformanceResults.System.SystemNetwork.DataSent_bytes_per_sec.P80 = Percentile(networkDataSentPerSecondStatistic.ToArray(), 80);
                _iosPerformanceResults.System.SystemNetwork.DataSent_bytes_per_sec.P90 = Percentile(networkDataSentPerSecondStatistic.ToArray(), 90);
                _iosPerformanceResults.System.SystemNetwork.DataSent_bytes_per_sec.P95 = Percentile(networkDataSentPerSecondStatistic.ToArray(), 95);
                _iosPerformanceResults.System.SystemNetwork.DataSent_bytes_per_sec.P99 = Percentile(networkDataSentPerSecondStatistic.ToArray(), 99);
            }
            
            if (networkPacketsInPerSecondStatistic.Count > 0)
            {
                _iosPerformanceResults.System.SystemNetwork.PacketsIn_per_sec.Avg = Math.Round(networkPacketsInPerSecondStatistic.Average(), 2);
                _iosPerformanceResults.System.SystemNetwork.PacketsIn_per_sec.Max = Math.Round(networkPacketsInPerSecondStatistic.Max(), 2);
                _iosPerformanceResults.System.SystemNetwork.PacketsIn_per_sec.Min = Math.Round(networkPacketsInPerSecondStatistic.Min(), 2);
                
                _iosPerformanceResults.System.SystemNetwork.PacketsIn_per_sec.P50 = Percentile(networkPacketsInPerSecondStatistic.ToArray(), 50);
                _iosPerformanceResults.System.SystemNetwork.PacketsIn_per_sec.P75 = Percentile(networkPacketsInPerSecondStatistic.ToArray(), 75);
                _iosPerformanceResults.System.SystemNetwork.PacketsIn_per_sec.P80 = Percentile(networkPacketsInPerSecondStatistic.ToArray(), 80);
                _iosPerformanceResults.System.SystemNetwork.PacketsIn_per_sec.P90 = Percentile(networkPacketsInPerSecondStatistic.ToArray(), 90);
                _iosPerformanceResults.System.SystemNetwork.PacketsIn_per_sec.P95 = Percentile(networkPacketsInPerSecondStatistic.ToArray(), 95);
                _iosPerformanceResults.System.SystemNetwork.PacketsIn_per_sec.P99 = Percentile(networkPacketsInPerSecondStatistic.ToArray(), 99);
            }
            
            if (networkPacketsOutPerSecondStatistic.Count > 0)
            {
                _iosPerformanceResults.System.SystemNetwork.PacketsOut_per_sec.Avg = Math.Round(networkPacketsOutPerSecondStatistic.Average(), 2);
                _iosPerformanceResults.System.SystemNetwork.PacketsOut_per_sec.Max = Math.Round(networkPacketsOutPerSecondStatistic.Max(), 2);
                _iosPerformanceResults.System.SystemNetwork.PacketsOut_per_sec.Min = Math.Round(networkPacketsOutPerSecondStatistic.Min(), 2);
                
                _iosPerformanceResults.System.SystemNetwork.PacketsOut_per_sec.P50 = Percentile(networkPacketsOutPerSecondStatistic.ToArray(), 50);
                _iosPerformanceResults.System.SystemNetwork.PacketsOut_per_sec.P75 = Percentile(networkPacketsOutPerSecondStatistic.ToArray(), 75);
                _iosPerformanceResults.System.SystemNetwork.PacketsOut_per_sec.P80 = Percentile(networkPacketsOutPerSecondStatistic.ToArray(), 80);
                _iosPerformanceResults.System.SystemNetwork.PacketsOut_per_sec.P90 = Percentile(networkPacketsOutPerSecondStatistic.ToArray(), 90);
                _iosPerformanceResults.System.SystemNetwork.PacketsOut_per_sec.P95 = Percentile(networkPacketsOutPerSecondStatistic.ToArray(), 95);
                _iosPerformanceResults.System.SystemNetwork.PacketsOut_per_sec.P99 = Percentile(networkPacketsOutPerSecondStatistic.ToArray(), 99);
            }

            await _influxDbSync.SavePoints(points.ToArray());

            Log.Debug($"[IOS: {device}] HardwareMetricsHandler - stop to handle System metrics");
        }
        catch(Exception e)
        {
            Log.Error($"[IOS: {device}] HardwareMetricsHandler - failed to handle System metrics. StackTrace: " +
                      $"\n {e.Message}\n {e.StackTrace}");
        }
    }

    private async Task ExtractAndSaveFpsMetrics(CancellationToken token)
    {
        var device = _deviceContext.DeviceParameters.Device;

        try
        {
            var xmlNodeList = GetXmlNodeList($"fps-{device}.xml");
            var metrics = ExtractMetrics<FpsMetrics>(xmlNodeList);
            var points = new List<PointData>();
            var fpsStatistic = new List<double>();
            var gpuStatistic = new List<double>();
           
            foreach (var metric in metrics)
            {
                var timestamp = GetTimestamp(metric.First(x => x.MetricType == FpsMetrics.StartTime).MetricData.Text);
                var fpsRaw = metric.Find(x => x.MetricType == FpsMetrics.Fps)?.MetricData.Text;

                if (fpsRaw != null)
                {
                    var fps = Math.Round(double.Parse(fpsRaw), 2);
                    fpsStatistic.Add(fps);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.system.fps",
                        _deviceContext.CommonTags,
                        timestamp,
                        fps,
                        "count"));
                }
                
                var gpuRaw = metric.Find(x => x.MetricType == FpsMetrics.GpuPercentage)?.MetricData.Text;

                if (gpuRaw != null)
                {
                    var gpu = Math.Round(double.Parse(gpuRaw), 2);
                    gpuStatistic.Add(gpu);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.system.gpu",
                        _deviceContext.CommonTags,
                        timestamp,
                        gpu,
                        "percentage"));
                }
            }
            
            if (fpsStatistic.Count > 0)
            {
                _iosPerformanceResults.System.Frames.Fps.Avg = Math.Round(fpsStatistic.Average(), 2);
                _iosPerformanceResults.System.Frames.Fps.Max = fpsStatistic.Max();
                _iosPerformanceResults.System.Frames.Fps.Min = fpsStatistic.Min();
                
                _iosPerformanceResults.System.Frames.Fps.P50 = Percentile(fpsStatistic.ToArray(), 50);
                _iosPerformanceResults.System.Frames.Fps.P75 = Percentile(fpsStatistic.ToArray(), 75);
                _iosPerformanceResults.System.Frames.Fps.P80 = Percentile(fpsStatistic.ToArray(), 80);
                _iosPerformanceResults.System.Frames.Fps.P90 = Percentile(fpsStatistic.ToArray(), 90);
                _iosPerformanceResults.System.Frames.Fps.P95 = Percentile(fpsStatistic.ToArray(), 95);
                _iosPerformanceResults.System.Frames.Fps.P99 = Percentile(fpsStatistic.ToArray(), 99);
            }
            
            if (gpuStatistic.Count > 0)
            {
                _iosPerformanceResults.System.Gpu.GpuUtilization_percentage.Avg = Math.Round(gpuStatistic.Average(), 2);
                _iosPerformanceResults.System.Gpu.GpuUtilization_percentage.Max = gpuStatistic.Max();
                _iosPerformanceResults.System.Gpu.GpuUtilization_percentage.Min = gpuStatistic.Min();
                
                _iosPerformanceResults.System.Gpu.GpuUtilization_percentage.P50 = Percentile(gpuStatistic.ToArray(), 50);
                _iosPerformanceResults.System.Gpu.GpuUtilization_percentage.P75 = Percentile(gpuStatistic.ToArray(), 75);
                _iosPerformanceResults.System.Gpu.GpuUtilization_percentage.P80 = Percentile(gpuStatistic.ToArray(), 80);
                _iosPerformanceResults.System.Gpu.GpuUtilization_percentage.P90 = Percentile(gpuStatistic.ToArray(), 90);
                _iosPerformanceResults.System.Gpu.GpuUtilization_percentage.P95 = Percentile(gpuStatistic.ToArray(), 95);
                _iosPerformanceResults.System.Gpu.GpuUtilization_percentage.P99 = Percentile(gpuStatistic.ToArray(), 99);
            }
            
            await _influxDbSync.SavePoints(points.ToArray());

            Log.Debug($"[IOS: {device}] HardwareMetricsHandler - stop to handle Fps and Gpu metrics");
        }
        catch(Exception e)
        {
            Log.Error($"[IOS: {device}] HardwareMetricsHandler - failed to handle Fps and Gpu metrics. StackTrace: " +
                      $"\n {e.Message}\n {e.StackTrace}");
        }
    }

    private async Task ExtractAndSaveProcessNetworkMetrics(CancellationToken token)
    {
        if (string.IsNullOrEmpty(_deviceContext.DeviceParameters.App))
            return;

        var device = _deviceContext.DeviceParameters.Device;

        try
        {
            var xmlNodeList = GetXmlNodeList($"network-{device}.xml");
            int? processId = FindProcessId(xmlNodeList, _deviceContext.DeviceParameters.App);

            if (processId == null)
                return;

            var metrics = ExtractMetrics<ProcessNetworkMetrics>(xmlNodeList)
                .FindAll(x => x.Any(y => y.MetricData.Id == processId));
            var parsedMetrics = new List<ProcessNetworkHolder>();
            var points = new List<PointData>();
            var dataInStatistic = new List<long>();
            var dataOutStatistic = new List<long>();
            var packetsInStatistic = new List<int>();
            var packetsOutStatistic = new List<int>();

            foreach (var metric in metrics)
            {
                var timestamp =
                    GetTimestamp(metric.First(x => x.MetricType == ProcessNetworkMetrics.StartTime).MetricData.Text);
                var networkInterface = metric.Find(x => x.MetricType == ProcessNetworkMetrics.NetworkInterface)?.MetricData
                    .Text;
                var protocol = metric.Find(x => x.MetricType == ProcessNetworkMetrics.Protocol)?.MetricData.Text;
                var bytesInRaw = metric.Find(x => x.MetricType == ProcessNetworkMetrics.BytesInPerSec)?.MetricData.Text;
                long bytesIn = string.IsNullOrEmpty(bytesInRaw) ? 0 : long.Parse(bytesInRaw);
                var bytesOutRaw = metric.Find(x => x.MetricType == ProcessNetworkMetrics.BytesOutPerSec)?.MetricData.Text;
                long bytesOut = string.IsNullOrEmpty(bytesOutRaw) ? 0 : long.Parse(bytesOutRaw);
                var packetsInRaw = metric.Find(x => x.MetricType == ProcessNetworkMetrics.PacketsInPerSec)?.MetricData.Text;
                int packetsIn = string.IsNullOrEmpty(packetsInRaw) ? 0 : int.Parse(packetsInRaw);
                var packetsOutRaw = metric.Find(x => x.MetricType == ProcessNetworkMetrics.PacketsOutPerSec)?.MetricData
                    .Text;
                int packetsOut = string.IsNullOrEmpty(packetsOutRaw) ? 0 : int.Parse(packetsOutRaw);

                parsedMetrics.Add(new ProcessNetworkHolder
                {
                    Timestamp = timestamp,
                    NetworkInterface = networkInterface,
                    Protocol = protocol,
                    BytesIn = bytesIn,
                    BytesOut = bytesOut,
                    PacketsIn = packetsIn,
                    PacketsOut = packetsOut
                });
            }

            while (parsedMetrics.Count > 0)
            {
                var currentTimestamp = parsedMetrics[0].Timestamp;
                var matchesByTimestamp = parsedMetrics.FindAll(x => x.Timestamp == currentTimestamp);
                var matchesByNetworkInterface = matchesByTimestamp.GroupBy(x => x.NetworkInterface);

                foreach (var matchByNetworkInterface in matchesByNetworkInterface)
                {
                    long totalBytesIn = 0, totalBytesOut = 0;
                    int totalPacketsIn = 0, totalPacketsOut = 0;

                    var tempTags = _deviceContext.CommonTags.ToDictionary(p => p.Key, p => p.Value);
                    tempTags.Add("interface", matchByNetworkInterface.Key);
                    
                    foreach (var match in matchByNetworkInterface)
                    {
                        totalBytesIn += match.BytesIn;
                        totalBytesOut += match.BytesOut;
                        totalPacketsIn += match.PacketsIn;
                        totalPacketsOut += match.PacketsOut;
                    }
                    
                    dataInStatistic.Add(totalBytesIn);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.app.network.data.rx",
                        tempTags,
                        currentTimestamp,
                        totalBytesIn,
                        "bytes"));
                    
                    dataOutStatistic.Add(totalBytesOut);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.app.network.data.tx",
                        tempTags,
                        currentTimestamp,
                        totalBytesOut,
                        "bytes"));
                    
                    packetsInStatistic.Add(totalPacketsIn);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.app.network.packets.in",
                        tempTags,
                        currentTimestamp,
                        totalPacketsIn,
                        "count"));
                    
                    packetsOutStatistic.Add(totalPacketsOut);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.app.network.packets.out",
                        tempTags,
                        currentTimestamp,
                        totalPacketsOut,
                        "count"));
                }
                
                parsedMetrics.RemoveAll(x => x.Timestamp == currentTimestamp);
            }
            
            if (dataInStatistic.Count > 0)
            {
                _iosPerformanceResults.Application.ApplicationNetwork.DataReceived_bytes_per_sec.Avg = Math.Round(dataInStatistic.Average(), 0);
                _iosPerformanceResults.Application.ApplicationNetwork.DataReceived_bytes_per_sec.Max = dataInStatistic.Max();
                _iosPerformanceResults.Application.ApplicationNetwork.DataReceived_bytes_per_sec.Min = dataInStatistic.Min();
                
                _iosPerformanceResults.Application.ApplicationNetwork.DataReceived_bytes_per_sec.P50 = Percentile(dataInStatistic.ToArray(), 50);
                _iosPerformanceResults.Application.ApplicationNetwork.DataReceived_bytes_per_sec.P75 = Percentile(dataInStatistic.ToArray(), 75);
                _iosPerformanceResults.Application.ApplicationNetwork.DataReceived_bytes_per_sec.P80 = Percentile(dataInStatistic.ToArray(), 80);
                _iosPerformanceResults.Application.ApplicationNetwork.DataReceived_bytes_per_sec.P90 = Percentile(dataInStatistic.ToArray(), 90);
                _iosPerformanceResults.Application.ApplicationNetwork.DataReceived_bytes_per_sec.P95 = Percentile(dataInStatistic.ToArray(), 95);
                _iosPerformanceResults.Application.ApplicationNetwork.DataReceived_bytes_per_sec.P99 = Percentile(dataInStatistic.ToArray(), 99);
            }
            
            if (dataOutStatistic.Count > 0)
            {
                _iosPerformanceResults.Application.ApplicationNetwork.DataSent_bytes_per_sec.Avg = Math.Round(dataOutStatistic.Average(), 0);
                _iosPerformanceResults.Application.ApplicationNetwork.DataSent_bytes_per_sec.Max = dataOutStatistic.Max();
                _iosPerformanceResults.Application.ApplicationNetwork.DataSent_bytes_per_sec.Min = dataOutStatistic.Min();
                
                _iosPerformanceResults.Application.ApplicationNetwork.DataSent_bytes_per_sec.P50 = Percentile(dataOutStatistic.ToArray(), 50);
                _iosPerformanceResults.Application.ApplicationNetwork.DataSent_bytes_per_sec.P75 = Percentile(dataOutStatistic.ToArray(), 75);
                _iosPerformanceResults.Application.ApplicationNetwork.DataSent_bytes_per_sec.P80 = Percentile(dataOutStatistic.ToArray(), 80);
                _iosPerformanceResults.Application.ApplicationNetwork.DataSent_bytes_per_sec.P90 = Percentile(dataOutStatistic.ToArray(), 90);
                _iosPerformanceResults.Application.ApplicationNetwork.DataSent_bytes_per_sec.P95 = Percentile(dataOutStatistic.ToArray(), 95);
                _iosPerformanceResults.Application.ApplicationNetwork.DataSent_bytes_per_sec.P99 = Percentile(dataOutStatistic.ToArray(), 99);
            }
            
            if (packetsInStatistic.Count > 0)
            {
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsIn_per_sec.Avg = Math.Round(packetsInStatistic.Average(), 0);
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsIn_per_sec.Max = packetsInStatistic.Max();
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsIn_per_sec.Min = packetsInStatistic.Min();
                
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsIn_per_sec.P50 = Percentile(packetsInStatistic.ToArray(), 50);
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsIn_per_sec.P75 = Percentile(packetsInStatistic.ToArray(), 75);
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsIn_per_sec.P80 = Percentile(packetsInStatistic.ToArray(), 80);
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsIn_per_sec.P90 = Percentile(packetsInStatistic.ToArray(), 90);
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsIn_per_sec.P95 = Percentile(packetsInStatistic.ToArray(), 95);
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsIn_per_sec.P99 = Percentile(packetsInStatistic.ToArray(), 99);
            }
            
            if (packetsOutStatistic.Count > 0)
            {
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsOut_per_sec.Avg = Math.Round(packetsOutStatistic.Average(), 0);
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsOut_per_sec.Max = packetsOutStatistic.Max();
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsOut_per_sec.Min = packetsOutStatistic.Min();
                
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsOut_per_sec.P50 = Percentile(packetsOutStatistic.ToArray(), 50);
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsOut_per_sec.P75 = Percentile(packetsOutStatistic.ToArray(), 75);
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsOut_per_sec.P80 = Percentile(packetsOutStatistic.ToArray(), 80);
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsOut_per_sec.P90 = Percentile(packetsOutStatistic.ToArray(), 90);
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsOut_per_sec.P95 = Percentile(packetsOutStatistic.ToArray(), 95);
                _iosPerformanceResults.Application.ApplicationNetwork.PacketsOut_per_sec.P99 = Percentile(packetsOutStatistic.ToArray(), 99);
            }
            
            await _influxDbSync.SavePoints(points.ToArray());

            Log.Debug($"[IOS: {device}] HardwareMetricsHandler - stop to handle Process Network metrics");
        }
        catch(Exception e)
        {
            Log.Error($"[IOS: {device}] HardwareMetricsHandler - failed to handle ProcessNetwork metrics. StackTrace: " +
                      $"\n {e.Message}\n {e.StackTrace}");
        }
    }
    
    class ProcessNetworkHolder
    {
        public DateTime Timestamp{ get; set; }
        public string NetworkInterface{ get; set; }
        public string Protocol{ get; set; }
        public long BytesIn{ get; set; }
        public long BytesOut{ get; set; }
        public int PacketsIn{ get; set; }
        public int PacketsOut{ get; set; }
    }

    private async Task ExtractAndSaveProcessMetrics(CancellationToken token)
    {
        if (string.IsNullOrEmpty(_deviceContext.DeviceParameters.App))
            return;

        var device = _deviceContext.DeviceParameters.Device;

        try
        {
            var xmlNodeList = GetXmlNodeList($"process-{device}.xml");
            int? processId = FindProcessId(xmlNodeList, _deviceContext.DeviceParameters.App);

            if (processId == null)
                return;

            var metrics = ExtractMetrics<ProcessMetrics>(xmlNodeList)
                .FindAll(x => x.Any(y => y.MetricData.Id == processId));
            
            var points = new List<PointData>();
            var cpuStatistic = new List<double>();
            var ramStatistic = new List<long>();

            foreach (var metric in metrics)
            {
                var timestamp = GetTimestamp(metric.First(x => x.MetricType == ProcessMetrics.StartTime).MetricData.Text);
                var rawCpu = metric.Find(x => x.MetricType == ProcessMetrics.Cpu)?.MetricData.Text;

                if (rawCpu != null)
                {
                    var cpu = Math.Round(Double.Parse(rawCpu), 2);
                    cpuStatistic.Add(cpu);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.app.cpu",
                        _deviceContext.CommonTags,
                        timestamp,
                        cpu,
                        "percentage"));
                }

                var ramRaw = metric.Find(x => x.MetricType == ProcessMetrics.Memory)?.MetricData.Text;

                if (ramRaw != null)
                {
                    var ram = long.Parse(ramRaw);
                    ramStatistic.Add(ram);
                    points.Add(_influxDbSync.GeneratePoint(
                        "ios.app.ram",
                        _deviceContext.CommonTags,
                        timestamp,
                        ram,
                        "bytes"));
                }
            }
            
            if (cpuStatistic.Count > 0)
            {
                _iosPerformanceResults.Application.Cpu.UsedCpu_percentage.Avg = Math.Round(cpuStatistic.Average(), 2);
                _iosPerformanceResults.Application.Cpu.UsedCpu_percentage.Max = Math.Round(cpuStatistic.Max(), 2);
                _iosPerformanceResults.Application.Cpu.UsedCpu_percentage.Min = Math.Round(cpuStatistic.Min(), 2);
                
                _iosPerformanceResults.Application.Cpu.UsedCpu_percentage.P50 = Percentile(cpuStatistic.ToArray(), 50);
                _iosPerformanceResults.Application.Cpu.UsedCpu_percentage.P75 = Percentile(cpuStatistic.ToArray(), 75);
                _iosPerformanceResults.Application.Cpu.UsedCpu_percentage.P80 = Percentile(cpuStatistic.ToArray(), 80);
                _iosPerformanceResults.Application.Cpu.UsedCpu_percentage.P90 = Percentile(cpuStatistic.ToArray(), 90);
                _iosPerformanceResults.Application.Cpu.UsedCpu_percentage.P95 = Percentile(cpuStatistic.ToArray(), 95);
                _iosPerformanceResults.Application.Cpu.UsedCpu_percentage.P99 = Percentile(cpuStatistic.ToArray(), 99);
            }
            
            if (ramStatistic.Count > 0)
            {
                _iosPerformanceResults.Application.Ram.UsedRam_bytes.Avg = Math.Round(ramStatistic.Average(), 0);
                _iosPerformanceResults.Application.Ram.UsedRam_bytes.Max = ramStatistic.Max();
                _iosPerformanceResults.Application.Ram.UsedRam_bytes.Min = ramStatistic.Min();
                
                _iosPerformanceResults.Application.Ram.UsedRam_bytes.P50 = Percentile(ramStatistic.ToArray(), 50);
                _iosPerformanceResults.Application.Ram.UsedRam_bytes.P75 = Percentile(ramStatistic.ToArray(), 75);
                _iosPerformanceResults.Application.Ram.UsedRam_bytes.P80 = Percentile(ramStatistic.ToArray(), 80);
                _iosPerformanceResults.Application.Ram.UsedRam_bytes.P90 = Percentile(ramStatistic.ToArray(), 90);
                _iosPerformanceResults.Application.Ram.UsedRam_bytes.P95 = Percentile(ramStatistic.ToArray(), 95);
                _iosPerformanceResults.Application.Ram.UsedRam_bytes.P99 = Percentile(ramStatistic.ToArray(), 99);
            }
            
            await _influxDbSync.SavePoints(points.ToArray());

            Log.Debug($"[IOS: {device}] HardwareMetricsHandler - stop to handle Process metrics");
        }
        catch(Exception e)
        {
            Log.Error($"[IOS: {device}] HardwareMetricsHandler - failed to handle Process metrics. StackTrace: " +
                      $"\n {e.Message}\n {e.StackTrace}");
        }
    }

    public XmlNodeList? GetXmlNodeList(string fileName)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(fileName);
        return doc.SelectNodes("trace-query-result/node/row");
    }

    private int? FindProcessId(XmlNodeList xmlNodeList, string processName)
    {
        foreach (XmlNode xmlNode in xmlNodeList)
        {
            var xmlLines = ((XmlNode)xmlNode).ChildNodes;

            foreach (XmlNode xmlLine in xmlLines)
            {
                var attributes = XmlAttributesToList(xmlLine);

                if (
                    xmlLine.Name == "process" &&
                    attributes.FirstOrDefault(x => x.Name == "fmt")?.Value != null &&
                    (bool)attributes.FirstOrDefault(x => x.Name == "fmt")?.Value
                        .Contains(processName))
                {
                    var value = attributes.FirstOrDefault(x => x.Name == "id")?.Value;

                    if (value != null)
                        return int.Parse(value);
                }
            }
        }

        return null;
    }

    private void NormalizeMetrics<T>(List<List<IOSMetric<T>>> metrics)
    {
        var xmlLinesWithIds = new Dictionary<int, XmlLine>();

        foreach (var metricBatch in metrics)
        {
            foreach (var metric in metricBatch)
            {
                if (metric.MetricData.Id != null)
                    xmlLinesWithIds.Add((int)metric.MetricData.Id, metric.MetricData);

                var internalXmlLines = metric.MetricData.InternalXmlLines.FindAll(x => x.Id != null);

                foreach (var internalXmlLine in internalXmlLines)
                    xmlLinesWithIds.Add((int)internalXmlLine.Id!, internalXmlLine);
            }
        }

        foreach (var metricBatch in metrics)
        {
            foreach (var metric in metricBatch)
            {
                if (metric.MetricData.Id == null)
                {
                    var xmlLine = xmlLinesWithIds[(int)metric.MetricData.Ref!];

                    metric.MetricData.Id = xmlLine.Id;
                    metric.MetricData.Ref = xmlLine.Ref;
                    metric.MetricData.Text = xmlLine.Text;
                }
            }
        }
    }

    private List<List<IOSMetric<T>>> ExtractMetrics<T>(XmlNodeList xmlNodeList) where T : struct, IConvertible
    {
        var metrics = new List<List<IOSMetric<T>>>();

        foreach (var xmlRow in xmlNodeList)
        {
            var xmlLines = ((XmlNode)xmlRow).ChildNodes;
            var metricsBatch = new List<IOSMetric<T>>();

            foreach (T metric in Enum.GetValues(typeof(T)))
                metricsBatch.Add(ParseMetric(xmlLines, metric)!);

            metricsBatch.RemoveAll(x => x == null);
            metrics.Add(metricsBatch);
        }

        NormalizeMetrics(metrics);
        return metrics;
    }

    IOSMetric<T>? ParseMetric<T>(XmlNodeList xmlNodeList, T metric) where T : struct, IConvertible
    {
        var device = _deviceContext.DeviceParameters.Device;

        try
        {
            if (!typeof(T).IsEnum)
            {
                Log.Error($"[IOS: {device}] Metric should be as Enum: {metric}");
                return null;
            }

            var xmlNode = xmlNodeList[(int)Enum.Parse(typeof(T), metric.ToString()!)];

            if (xmlNode?.Name == "sentinel" || xmlNode == null)
                return null;

            var xmlLine = ParseXmlLine(xmlNode);

            if (xmlLine != null)
                return new IOSMetric<T>
                {
                    MetricType = metric,
                    MetricData = xmlLine
                };
        }
        catch (Exception e)
        {
            Log.Error($"[IOS: {device}] Cannot parse metric: {metric}, StackTrace: \n{e}");
        }

        return null;
    }

    XmlLine? ParseXmlLine(XmlNode xmlNode)
    {
        if (xmlNode.Name == "sentinel")
            return null;

        var attributes = XmlAttributesToList(xmlNode);

        var idStr = attributes.FirstOrDefault(x => x.Name == "id")?.Value;
        var id = idStr == null ? (int?)null : int.Parse(idStr);

        var refStr = attributes.FirstOrDefault(x => x.Name == "ref")?.Value;
        var @ref = refStr == null ? (int?)null : int.Parse(refStr);

        var childNodes = xmlNode.ChildNodes;
        var internalXmlLines = new List<XmlLine>();

        foreach (XmlNode childNode in childNodes)
        {
            var internalXmlLine = ParseXmlLine(childNode);

            if (internalXmlLine != null)
                internalXmlLines.Add(internalXmlLine);
        }

        return new XmlLine
        {
            Id = id,
            Ref = @ref,
            Text = xmlNode.InnerText,
            InternalXmlLines = internalXmlLines
        };
    }

    private List<XmlAttribute> XmlAttributesToList(XmlNode xmlNode)
    {
        var attributes = xmlNode.Attributes;
        var attributesList = new List<XmlAttribute>();

        if (attributes == null)
            return attributesList;

        foreach (XmlAttribute attribute in attributes)
            attributesList.Add(attribute);

        return attributesList;
    }
    
    private DateTime GetTimestamp(string rawTextTimestamp)
    {
        var relativeTimestampMs = long.Parse(rawTextTimestamp) / 1000000;
        return _deviceContext.StartTime.AddMilliseconds(relativeTimestampMs);
    }
    
    T Percentile<T>(T[] sequence, int percentile)
    {
        Array.Sort(sequence);
        var index = Math.Ceiling(percentile / 100d * sequence.Length) - 1;
        return sequence[(int)index];
    }
}