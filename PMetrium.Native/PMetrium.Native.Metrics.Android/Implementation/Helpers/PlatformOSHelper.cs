using System.Diagnostics;

namespace PMetrium.Native.Metrics.Android.Implementation.Helpers
{
    public static class PlatformOSHelper
    {
        public static Process CreateProcess(string fileName, string arguments)
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }
    }
}