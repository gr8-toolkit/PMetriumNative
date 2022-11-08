using System.Diagnostics;
using Serilog;

namespace PMetrium.Native.Common.Helpers.Extensions
{
    public static class ProcessExtensions
    {
        public static void StartForDevice(this Process process, string device)
        {
            process.ErrorDataReceived += (sender, outLine) =>
            {
                if (!string.IsNullOrEmpty(outLine.Data))
                    Log.Error(string.IsNullOrEmpty(device) ? $"{outLine.Data}" : $"[{device}] {outLine.Data}");
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        public static async Task StartProcessAndWait(this Process process)
        {
            process.ErrorDataReceived += (sender, outLine) =>
            {
                if (!string.IsNullOrEmpty(outLine.Data))
                    Log.Error(outLine.Data);
            };

            process.Start();
            process.BeginErrorReadLine();

            var source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(60));

            await process.WaitForExitAsync(source.Token);
        }

        public static async Task<string> StartForDeviceAndGetOutput(
            this Process process,
            string device,
            CancellationToken token)
        {
            var errors = "";

            process.ErrorDataReceived += (sender, outLine) =>
            {
                if (!string.IsNullOrEmpty(outLine.Data))
                    errors += outLine.Data;
            };

            var output = "";

            process.OutputDataReceived += (sender, outLine) =>
            {
                if (!string.IsNullOrEmpty(outLine.Data))
                    output += outLine.Data;
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(token);

            if (!string.IsNullOrEmpty(errors))
                Log.Error(string.IsNullOrEmpty(device) ? $"{errors}" : $"[{device}] {errors}");

            return output;
        }
    }
}