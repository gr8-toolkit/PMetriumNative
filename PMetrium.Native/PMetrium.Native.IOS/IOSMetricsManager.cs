using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;  
using Newtonsoft.Json;
using PMetrium.Native.Common.Contracts;
using PMetrium.Native.Common.Contracts.IOS.XMLClasses;
using PMetrium.Native.Common.Helpers;
using PMetrium.Native.Common.Helpers.Extensions;
using PMetrium.Native.IOS.MetricsHandlers;
using Serilog;
using static PMetrium.Native.Common.Helpers.PlatformOSHelper;

namespace PMetrium.Native.IOS;

public interface IIOSMetricsManager
{
    Task Start(
        string device,
        string applicationName,
        string space,
        string group,
        string label);

    Task<IOSPerformanceResults> Stop(string device);
}

public class IOSMetricsManager : IIOSMetricsManager
{
    private InfluxDBSync _influxDb;
    private ConcurrentDictionary<string, IOSDeviceContext> _devicesContexts = new();

    public IOSMetricsManager(InfluxDBSync influxDb)
    {
        _influxDb = influxDb;
    }

    public async Task Start(
        string device,
        string applicationName,
        string space,
        string group,
        string label)
    {
        var deviceContext = await ProvideDeviceContext(
            device,
            applicationName,
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

        await CreateProcess($"rm", $"-rf {device}.trace").StartProcessAndWait();

        var process = CreateProcess(
            $"xctrace",
            $"record --template PMetriumNative --device-name {device} --no-prompt --all-processes --output {device}.trace");

        var logsToTrack = new ConcurrentBag<string>();

        process.OutputDataReceived += (sender, outLine) =>
        {
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                var log = outLine.Data;
                logsToTrack.Add(log);
                Log.Debug($"[IOS: {device}] {log}");
            }
        };

        deviceContext.Process = process;
        process.StartForDevice(device);

        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromMinutes(1));

        while (!logsToTrack.Any(x => x.Contains("Ctrl-C")) && !source.IsCancellationRequested)
            await Task.Delay(10);

        if (source.IsCancellationRequested)
        {
            process.Kill();
            process.Close();
            _devicesContexts.Remove(device, out _);
            throw new Exception(
                $"[IOS: {device}] Failed to start measurement because of missing confirmation from the phone");
        }

        Log.Information($"[IOS: {device}] IOSMetricsManager - measurement has started");
    }

    public async Task<IOSPerformanceResults> Stop(string device)
    {
        if (!_devicesContexts.TryGetValue(device, out var deviceContext))
            throw new Exception($"[IOS: {device}] Process for hardware metrics not found");

        _devicesContexts.Remove(device, out _);

        await CreateProcess("kill", $"-SIGINT {deviceContext.Process.Id}").StartProcessAndWait();
        await CreateProcess("kill", $"-9 {deviceContext.EventsProcess.Id}").StartProcessAndWait();

        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(60));
        var token = source.Token;

        await deviceContext.Process.WaitForExitAsync(token);
        await deviceContext.EventsProcess.WaitForExitAsync(token);

        Log.Information($"[IOS: {device}] IOSMetricsManager - measurement has stopped");

        var xmlTraceToc = await CreateProcess(
            $"xctrace",
            $"export --input {device}.trace --toc").StartForDeviceAndGetOutput(device, token);

        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(TraceToc));
        var memStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlTraceToc));
        var traceToc = (TraceToc)serializer.Deserialize(memStream)!;
        memStream.Close();

        deviceContext.StartTime = traceToc.Run[0].Info.Summary.StartDate.ToUniversalTime();
        deviceContext.EndTime = traceToc.Run[0].Info.Summary.EndDate.ToUniversalTime();

        await _influxDb.SaveAnnotationToInfluxDBAsync(
            "ios.annotations",
            "[ Test STARTED ]",
            deviceContext.AnnotationTags,
            deviceContext.CommonTags,
            deviceContext.StartTime);

        await _influxDb.SaveAnnotationToInfluxDBAsync(
            "ios.annotations",
            "[ Test FINISHED ]",
            deviceContext.AnnotationTags,
            deviceContext.CommonTags,
            deviceContext.EndTime);

        await ExportTracesToXML(device);
        
        var isoPerformanceResults = new IOSPerformanceResults();

        var applicationMetricsHandler = new ApplicationMetricsHandler(
            deviceContext,
            _influxDb,
            isoPerformanceResults);
        await applicationMetricsHandler.ExtractAndSaveMetrics(token);

        return new IOSPerformanceResults();
    }

    private async Task ExportTracesToXML(string device)
    {
        await CreateProcess(
                "xctrace",
                $"export --input {device}.trace --output system-{device}.xml --xpath '/trace-toc/run[@number=\"1\"]/data/table[@schema=\"activity-monitor-system\"]'")
            .StartProcessAndWait();

        await CreateProcess(
                "xctrace",
                $"export --input {device}.trace --output process-{device}.xml --xpath '/trace-toc/run[@number=\"1\"]/data/table[@schema=\"activity-monitor-process-live\"]'")
            .StartProcessAndWait();

        await CreateProcess(
                "xctrace",
                $"export --input {device}.trace --output fps-{device}.xml --xpath '/trace-toc/run[@number=\"1\"]/data/table[@schema=\"core-animation-fps-estimate\"]'")
            .StartProcessAndWait();

        await CreateProcess(
                "xctrace",
                $"export --input {device}.trace --output fps-{device}.xml --xpath '/trace-toc/run[@number=\"1\"]/data/table[@schema=\"network-connection-stat\"]'")
            .StartProcessAndWait();
    }

    private async Task<IOSDeviceContext> ProvideDeviceContext(
        string device,
        string applicationName,
        string space,
        string group,
        string label)
    {
        var deviceContext = new IOSDeviceContext();
        var source = new CancellationTokenSource();

        deviceContext.ModelName = await CreateProcess("ideviceinfo", $"-k ProductType {device}")
            .StartForDeviceAndGetOutput(device, source.Token);

        deviceContext.IOSVersion = await CreateProcess("ideviceinfo", $"-k ProductVersion {device}")
            .StartForDeviceAndGetOutput(device, source.Token);

        source.CancelAfter(TimeSpan.FromSeconds(10));

        deviceContext.DeviceParameters = new IOSDeviceParameters()
        {
            App = applicationName,
            Device = device,
            Space = space,
            Group = group,
            Label = label
        };

        var annotationTags = new Dictionary<string, string>()
        {
            { "deviceModel", $"{deviceContext.ModelName}" },
            { "iosVersion", $"{deviceContext.IOSVersion}" },
            { "device", $"{deviceContext.DeviceParameters.Device}" },
            { "application", $"{deviceContext.DeviceParameters.App}" },
            { "space", $"{deviceContext.DeviceParameters.Space}" },
            { "group", $"{deviceContext.DeviceParameters.Group}" },
            { "label", $"{deviceContext.DeviceParameters.Label}" }
        };

        deviceContext.AnnotationTags = annotationTags;

        var commonTags = new Dictionary<string, string>()
        {
            { "space", $"{deviceContext.DeviceParameters.Space}" },
            { "group", $"{deviceContext.DeviceParameters.Group}" },
            { "label", $"{deviceContext.DeviceParameters.Label}" },
            { "iosVersion", $"{deviceContext.IOSVersion}" },
            { "deviceModel", $"{deviceContext.ModelName}" },
            { "device", $"{deviceContext.DeviceParameters.Device}" },
            { "application", $"{deviceContext.DeviceParameters.App}" }
        };

        deviceContext.CommonTags = commonTags;

        Log.Debug(
            $"[IOS: {device}] Created device context: {JsonConvert.SerializeObject(deviceContext, Formatting.Indented)}");

        var eventsProcess = CreateProcess("idevicesyslog", $"-m \"[PMetriumNative]\" {device}");
        var eventsLogs = new List<string>();

        eventsProcess.OutputDataReceived += (sender, outLine) =>
        {
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                var log = outLine.Data;
                eventsLogs.Add(log);
                Log.Debug($"[IOS: {device}] {log}");
            }
        };

        deviceContext.EventsProcess = eventsProcess;
        deviceContext.EventsLogs = eventsLogs;
        eventsProcess.StartForDevice(device);

        return deviceContext;
    }
}