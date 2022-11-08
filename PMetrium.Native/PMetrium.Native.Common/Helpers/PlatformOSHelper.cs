using System.Diagnostics;
using System.Reflection;

namespace PMetrium.Native.Common.Helpers
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

        public static string? WorkingDirectory => Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
    }
}