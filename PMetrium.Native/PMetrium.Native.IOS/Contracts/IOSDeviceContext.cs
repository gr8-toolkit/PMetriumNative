using System.Diagnostics;

namespace PMetrium.Native.IOS.Contracts
{
    public class IOSDeviceContext
    {
        public Process Process { get; set; }
        public IOSDeviceParameters DeviceParameters { get; set; }
        public Dictionary<string, string> AnnotationTags { get; set; } = new();
        public Dictionary<string, string> CommonTags { get; set; } = new();
        public List<string> EventsLogs { get; set; } = new();
        public Process EventsProcess { get; set; }
        public string ModelName { get; set; }
        public string IOSVersion { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}