using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using PMetrium.Native.Metrics.Android.Contracts;
using PMetrium.Native.Metrics.Android.Implementation.Helpers;
using PMetrium.Native.Metrics.Android.Implementation.Helpers.Extensions;
using PMetrium.Native.Metrics.Android.Implementation.MetricsHandlers;
using Serilog;
using static PMetrium.Native.Metrics.Android.Implementation.Helpers.PlatformOSHelper;


namespace PMetrium.Native.Metrics.Android.Implementation
{
    public interface IPerformanceMetrics
    {
        Task Start(Dictionary<string, string> parameters, CancellationToken token);

        Task Stop(Dictionary<string, string> parameters, CancellationToken token);
    }

    public class PerformanceMetricsManager : IPerformanceMetrics
    {
        private InfluxDBSync _influxDb;
        private ConcurrentDictionary<string, DeviceContext> _devicesContexts = new();

        public PerformanceMetricsManager(InfluxDBSync influxDb)
        {
            _influxDb = influxDb;
        }

        public async Task<bool> HealthCheck(CancellationToken token)
        {
            var process = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? CreateProcess("Files\\Scripts\\Bat\\healthCheck.bat", "")
                : CreateProcess("Files/Scripts/Shell/healthCheck.sh", "");

            var result = "";

            process.OutputDataReceived += (sender, outLine) =>
            {
                if (!string.IsNullOrEmpty(outLine.Data))
                    result += outLine.Data;
            };

            process.StartForDevice("");
            await process.WaitForExitAsync(token);

            if (result.Contains("Ready!"))
            {
                Log.Information($"HealthCheck - OK");
                return true;
            }

            Log.Error($"HealthCheck - FAIL");
            return false;
        }

        public async Task Start(Dictionary<string, string> parameters, CancellationToken token)
        {
            var deviceParameters = ProvideDeviceParameters(parameters);
            var deviceContext = await ProvideDeviceContext(deviceParameters, token);
            var device = deviceContext.DeviceParameters.Device;

            _devicesContexts.AddOrUpdate(device, deviceContext,
                (oldDevice, oldDeviceContexts) =>
                {
                    oldDeviceContexts?.Process?.Kill();
                    oldDeviceContexts?.Process?.Close();
                    return deviceContext;
                });

            var process = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? CreateProcess("Files\\Scripts\\Bat\\start.bat", $"{device} \"{PrepareScriptParameters(deviceContext)}\"")
                : CreateProcess("Files/Scripts/Shell/start.sh", $"{device} \"{PrepareScriptParameters(deviceContext)}\"");

            var logsToTrack = new List<string>();

            process.OutputDataReceived += (sender, outLine) =>
            {
                if (!string.IsNullOrEmpty(outLine.Data))
                {
                    var log = outLine.Data;
                    logsToTrack.Add(log);
                    Log.Debug($"[{device}] {log}");
                }
            };

            deviceContext.Process = process;
            process.StartForDevice(device);

            var dateTimeToStop = DateTime.UtcNow.AddMinutes(2);

            while (!logsToTrack.Any( x => x.Contains("READY_TO_TRACK_METRICS")) && DateTime.UtcNow < dateTimeToStop)
            {
                await Task.Delay(10, token);
            }

            Log.Information($"[{device}] PerformanceMetricsManager - measurement has started");

            await _influxDb.SaveAnnotationToInfluxDBAsync(
                "android.annotations",
                "[ Test STARTED ]",
                deviceContext.AnnotationTags,
                deviceContext.CommonTags);
        }

        public async Task Stop(Dictionary<string, string> parameters, CancellationToken token)
        {
            if (!parameters.ContainsKey("device"))
                throw new Exception("Stop measurement: Empty device");

            var device = parameters["device"];

            if (!_devicesContexts.TryGetValue(device, out var deviceContext))
            {
                Log.Error($"[{device}] Process for hardware metrics not found");
                throw new Exception($"[{device}] Process for hardware metrics not found");
            }

            var applicationMetricsHandler = new ApplicationMetricsHandler(deviceContext, _influxDb);
            await applicationMetricsHandler.ExtractAndSaveMetrics(token);

            var process = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? CreateProcess("Files\\Scripts\\Bat\\stop.bat", device)
                : CreateProcess("Files/Scripts/Shell/stop.sh", device);

            process.OutputDataReceived += (sender, outLine) =>
            {
                if (!string.IsNullOrEmpty(outLine.Data))
                    Log.Information($"[{device}] {outLine.Data}");
            };

            process.StartForDevice(device);

            await deviceContext.Process.WaitForExitAsync(token);
            await process.WaitForExitAsync(token);

            await _influxDb.SaveAnnotationToInfluxDBAsync(
                "android.annotations",
                "[ Test FINISHED ]",
                deviceContext.AnnotationTags,
                deviceContext.CommonTags);

            new HardwareMetricsHandler(deviceContext, _influxDb)
                .ExtractAndSaveMetrics(token);

            Log.Information($"[{device}] PerformanceMetricsManager - measurement has stopped");
        }

        private DeviceParameters ProvideDeviceParameters(Dictionary<string, string> parameters)
        {
            var deviceParameters = new DeviceParameters();

            if (!parameters.ContainsKey("device"))
            {
                Log.Error("Missed required parameter 'device'");
                throw new Exception("Missed required parameter 'device'");
            }

            var device = parameters["device"];
            deviceParameters.Device = device;
            parameters.Remove("device");

            if (!parameters.ContainsKey("app"))
            {
                Log.Error($"[{device}] Missed required parameter 'app'");
                throw new Exception($"[{device}] Missed required parameter 'app'");
            }

            deviceParameters.App = parameters["app"];
            parameters.Remove("app");

            if (parameters.ContainsKey("cpuTotal"))
            {
                if (parameters["cpuTotal"] == "no" || parameters["cpuTotal"] == "yes")
                    deviceParameters.CpuTotal = parameters["cpuTotal"];

                parameters.Remove("cpuTotal");
            }

            if (parameters.ContainsKey("cpuApp"))
            {
                if (parameters["cpuApp"] == "no" || parameters["cpuApp"] == "yes")
                    deviceParameters.CpuApp = parameters["cpuApp"];

                parameters.Remove("cpuApp");
            }

            if (parameters.ContainsKey("ramTotal"))
            {
                if (parameters["ramTotal"] == "no" || parameters["ramTotal"] == "yes")
                    deviceParameters.RamTotal = parameters["ramTotal"];

                parameters.Remove("ramTotal");
            }

            if (parameters.ContainsKey("ramApp"))
            {
                if (parameters["ramApp"] == "no" || parameters["ramApp"] == "yes")
                    deviceParameters.RamApp = parameters["ramApp"];

                parameters.Remove("ramApp");
            }

            if (parameters.ContainsKey("networkApp"))
            {
                if (parameters["networkApp"] == "no" || parameters["networkApp"] == "yes")
                    deviceParameters.NetworkApp = parameters["networkApp"];

                parameters.Remove("networkApp");
            }

            if (parameters.ContainsKey("batteryApp"))
            {
                if (parameters["batteryApp"] == "no" || parameters["batteryApp"] == "yes")
                    deviceParameters.BatteryApp = parameters["batteryApp"];

                parameters.Remove("batteryApp");
            }

            if (parameters.ContainsKey("framesApp"))
            {
                if (parameters["framesApp"] == "no" || parameters["framesApp"] == "yes")
                    deviceParameters.FramesApp = parameters["framesApp"];

                parameters.Remove("framesApp");
            }

            if (parameters.ContainsKey("space"))
            {
                deviceParameters.Space = parameters["space"];
                parameters.Remove("space");
            }

            if (parameters.ContainsKey("group"))
            {
                deviceParameters.Group = parameters["group"];
                parameters.Remove("group");
            }

            if (parameters.ContainsKey("label"))
            {
                deviceParameters.Label = parameters["label"];
                parameters.Remove("label");
            }

            if (parameters.Count > 0)
                Log.Warning(
                    $"[{device}] Unknown parameters will be skipped: {JsonConvert.SerializeObject(parameters)}");

            return deviceParameters;
        }

        private async Task<DeviceContext> ProvideDeviceContext(
            DeviceParameters deviceParameters,
            CancellationToken token)
        {
            var device = deviceParameters.Device;
            var deviceContext = new DeviceContext();
            var deviceInfoList = new List<string>();

            var rootCode = await CreateProcess(
                    "adb",
                    $"-s {device} shell \"sh -c 'su -c whoami > /dev/null' 3&> /dev/null; echo $?\"")
                .StartForDeviceAndGetOutput(device);

            if (rootCode != "0")
            {
                deviceParameters.NetworkApp = "no";
                deviceContext.DoesHaveRoot = "no";
                Log.Warning($"[{device}] ROOT access is not available, --networkApp metric will be skipped");
            }
            else
            {
                deviceContext.DoesHaveRoot = "yes";
            }

            var deviceBrand = await CreateProcess(
                    "adb",
                    $"-s {device} shell \"getprop ro.product.manufacturer\"")
                .StartForDeviceAndGetOutput(device);

            var deviceModel = await CreateProcess(
                    "adb",
                    $"-s {device} shell \"getprop ro.product.model\"")
                .StartForDeviceAndGetOutput(device);

            deviceContext.DeviceName = $"{deviceBrand} {deviceModel}";
            deviceContext.AndroidVersion = await CreateProcess(
                    "adb",
                    $"-s {device} shell \"getprop ro.build.version.release\"")
                .StartForDeviceAndGetOutput(device);

            deviceContext.DeviceParameters = deviceParameters;

            var annotationTags = new Dictionary<string, string>()
            {
                { "root", $"{deviceContext.DoesHaveRoot}" },
                { "deviceName", $"{deviceContext.DeviceName}" },
                { "androidVersion", $"{deviceContext.AndroidVersion}" },
                { "device", $"{deviceContext.DeviceParameters.Device}" },
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
                { "androidVersion", $"{deviceContext.AndroidVersion}" },
                { "deviceName", $"{deviceContext.DeviceName}" },
                { "device", $"{deviceContext.DeviceParameters.Device}" }
            };

            deviceContext.CommonTags = commonTags;

            Log.Debug(
                $"[{device}] Created device context: {JsonConvert.SerializeObject(deviceContext, Formatting.Indented)}");

            return deviceContext;
        }

        private string PrepareScriptParameters(DeviceContext deviceContext)
        {
            var finalParameters = $"--app={deviceContext.DeviceParameters.App} " +
                                  $"--cpuApp={deviceContext.DeviceParameters.CpuApp} " +
                                  $"--cpuTotal={deviceContext.DeviceParameters.CpuTotal} " +
                                  $"--ramTotal={deviceContext.DeviceParameters.RamTotal} " +
                                  $"--ramApp={deviceContext.DeviceParameters.RamApp} " +
                                  $"--networkApp={deviceContext.DeviceParameters.NetworkApp} " +
                                  $"--batteryApp={deviceContext.DeviceParameters.BatteryApp} " +
                                  $"--framesApp={deviceContext.DeviceParameters.FramesApp}";

            Log.Debug($"[{deviceContext.DeviceParameters.Device}] Parameters for phoneMetrics.sh: {finalParameters}");

            return finalParameters;
        }
    }
}