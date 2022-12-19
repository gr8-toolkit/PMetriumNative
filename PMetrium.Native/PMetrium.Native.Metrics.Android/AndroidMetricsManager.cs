using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using PMetrium.Native.Common.Helpers;
using PMetrium.Native.Common.Helpers.Extensions;
using PMetrium.Native.Metrics.Android.Contracts;
using PMetrium.Native.Metrics.Android.MetricsHandlers;
using Serilog;
using static PMetrium.Native.Common.Helpers.PlatformOSHelper;

namespace PMetrium.Native.Metrics.Android;

public interface IAndroidMetricsManager
{
    Task Start(string device,
        string applicationName,
        bool cpuApp,
        bool cpuTotal,
        bool ramTotal,
        bool ramApp,
        bool networkTotal,
        bool networkApp,
        bool batteryApp,
        bool framesApp,
        string space,
        string group,
        string label);

    Task<AndroidPerformanceResults> Stop(string device);
}

public class AndroidMetricsManager : IAndroidMetricsManager
{
    private InfluxDBSync _influxDb;
    private ConcurrentDictionary<string, AndroidDeviceContext> _devicesContexts = new();

    public AndroidMetricsManager(InfluxDBSync influxDb)
    {
        _influxDb = influxDb;
    }

    public async Task Start(string device,
        string applicationName,
        bool cpuApp,
        bool cpuTotal,
        bool ramTotal,
        bool ramApp,
        bool networkTotal,
        bool networkApp,
        bool batteryApp,
        bool framesApp,
        string space,
        string group,
        string label)
    {
        var deviceContext = await ProvideDeviceContext(
            device,
            applicationName,
            cpuApp,
            cpuTotal,
            ramTotal,
            ramApp,
            networkTotal,
            networkApp,
            batteryApp,
            framesApp,
            space,
            group,
            label);

        _devicesContexts.AddOrUpdate(device, deviceContext,
            (oldDevice, oldDeviceContexts) =>
            {
                oldDeviceContexts?.Process?.Kill();
                oldDeviceContexts?.Process?.Close();
                return deviceContext;
            });

        var process = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? CreateProcess($"{WorkingDirectory}\\Scripts\\Bat\\start.bat",
                $"{device} \"{PrepareScriptParameters(deviceContext)}\"")
            : CreateProcess($"{WorkingDirectory}/Scripts/Shell/start.sh",
                $"{device} \"{PrepareScriptParameters(deviceContext)}\"");

        var logsToTrack = new List<string>();

        process.OutputDataReceived += (sender, outLine) =>
        {
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                var log = outLine.Data;
                logsToTrack.Add(log);
                Log.Debug($"[Android: {device}] {log}");
            }
        };

        deviceContext.Process = process;
        process.StartForDevice(device);

        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromMinutes(2));

        while (!logsToTrack.Any(x => x.Contains("READY_TO_TRACK_METRICS")) && !source.IsCancellationRequested)
            await Task.Delay(10);

        if (source.IsCancellationRequested)
        {
            process.Kill();
            process.Close();
            _devicesContexts.Remove(device, out _);
            throw new Exception(
                $"[Android: {device}] Failed to start measurement because of missing confirmation from the phone");
        }

        Log.Information($"[Android: {device}] AndroidMetricsManager - measurement has started");

        await _influxDb.SaveAnnotationToInfluxDBAsync(
            "android.annotations",
            "[ Test STARTED ]",
            deviceContext.AnnotationTags,
            deviceContext.CommonTags);
    }

    public async Task<AndroidPerformanceResults> Stop(string device)
    {
        if (!_devicesContexts.TryGetValue(device, out var deviceContext))
            throw new Exception($"[Android: {device}] Process for hardware metrics not found");

        _devicesContexts.Remove(device, out _);

        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(60));
        var token = source.Token;

        var androidPerformanceResults = new AndroidPerformanceResults();

        var applicationMetricsHandler = new ApplicationMetricsHandler(
            deviceContext,
            _influxDb,
            androidPerformanceResults);
        await applicationMetricsHandler.ExtractAndSaveMetrics(token);

        var process = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? CreateProcess($"{WorkingDirectory}\\Scripts\\Bat\\stop.bat", device)
            : CreateProcess($"{WorkingDirectory}/Scripts/Shell/stop.sh", device);

        process.OutputDataReceived += (sender, outLine) =>
        {
            if (!string.IsNullOrEmpty(outLine.Data))
                Log.Information($"[Android: {device}] {outLine.Data}");
        };

        process.StartForDevice(device);
        await deviceContext.Process.WaitForExitAsync(token);
        await process.WaitForExitAsync(token);

        await _influxDb.SaveAnnotationToInfluxDBAsync(
            "android.annotations",
            "[ Test FINISHED ]",
            deviceContext.AnnotationTags,
            deviceContext.CommonTags);

        new HardwareMetricsHandler(deviceContext, _influxDb, androidPerformanceResults)
            .ExtractAndSaveMetrics(token);

        Log.Information($"[Android: {device}] AndroidMetricsManager - measurement has stopped");

        return androidPerformanceResults;
    }

    private async Task<AndroidDeviceContext> ProvideDeviceContext(
        string device,
        string applicationName,
        bool cpuApp,
        bool cpuTotal,
        bool ramTotal,
        bool ramApp,
        bool networkTotal,
        bool networkApp,
        bool batteryApp,
        bool framesApp,
        string space,
        string group,
        string label)
    {
        var deviceContext = new AndroidDeviceContext();
        var source = new CancellationTokenSource();

        var deviceManufacturer = await CreateProcess(
                "adb",
                $"-s {device} shell \"getprop ro.product.manufacturer\"")
            .StartForDeviceAndGetOutput(device, source.Token);
        var deviceModel = await CreateProcess(
                "adb",
                $"-s {device} shell \"getprop ro.product.model\"")
            .StartForDeviceAndGetOutput(device, source.Token);

        deviceContext.DeviceName = $"{deviceManufacturer} {deviceModel}";
        deviceContext.AndroidVersion = await CreateProcess(
                "adb",
                $"-s {device} shell \"getprop ro.build.version.release\"")
            .StartForDeviceAndGetOutput(device, source.Token);

        source.CancelAfter(TimeSpan.FromSeconds(10));

        deviceContext.DeviceParameters = new AndroidDeviceParameters()
        {
            App = applicationName,
            BatteryApp = batteryApp,
            CpuApp = cpuApp,
            CpuTotal = cpuTotal,
            Device = device,
            FramesApp = framesApp,
            RamApp = ramApp,
            RamTotal = ramTotal,
            NetworkTotal = networkTotal,
            NetworkApp = networkApp,
            Space = space,
            Group = group,
            Label = label
        };

        var commonTags = new Dictionary<string, string>()
        {
            { "space", $"{deviceContext.DeviceParameters.Space}" },
            { "group", $"{deviceContext.DeviceParameters.Group}" },
            { "label", $"{deviceContext.DeviceParameters.Label}" },
            { "androidVersion", $"{deviceContext.AndroidVersion}" },
            { "deviceName", $"{deviceContext.DeviceName}" },
            { "device", $"{deviceContext.DeviceParameters.Device}" },
            { "application", $"{deviceContext.DeviceParameters.App}" }
        };

        deviceContext.AnnotationTags = commonTags;
        deviceContext.CommonTags = commonTags;

        Log.Debug(
            $"[Android: {device}] Created device context: {JsonConvert.SerializeObject(deviceContext, Formatting.Indented)}");

        return deviceContext;
    }

    private string PrepareScriptParameters(AndroidDeviceContext deviceContext)
    {
        var finalParameters = $"--app={deviceContext.DeviceParameters.App} " +
                              $"--cpuApp={deviceContext.DeviceParameters.CpuApp.ToString().ToLower()} " +
                              $"--cpuTotal={deviceContext.DeviceParameters.CpuTotal.ToString().ToLower()} " +
                              $"--ramTotal={deviceContext.DeviceParameters.RamTotal.ToString().ToLower()} " +
                              $"--ramApp={deviceContext.DeviceParameters.RamApp.ToString().ToLower()} " +
                              $"--networkTotal={deviceContext.DeviceParameters.NetworkTotal.ToString().ToLower()} " +
                              $"--networkApp={deviceContext.DeviceParameters.NetworkApp.ToString().ToLower()} " +
                              $"--batteryApp={deviceContext.DeviceParameters.BatteryApp.ToString().ToLower()} " +
                              $"--framesApp={deviceContext.DeviceParameters.FramesApp.ToString().ToLower()}";

        Log.Debug(
            $"[Android: {deviceContext.DeviceParameters.Device}] Parameters for phoneMetrics.sh: {finalParameters}");

        return finalParameters;
    }
}