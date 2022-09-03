using System.Diagnostics;

namespace PMetrium.Native.Metrics.Android.Contracts
{
    public class DeviceContext
    {
        public Process Process { get; set; }
        public DeviceParameters DeviceParameters { get; set; }
        public Dictionary<string, string> AnnotationTags { get; set; } = new ();
        public Dictionary<string, string> CommonTags { get; set; } = new ();
        public string DoesHaveRoot { get; set; }
        public string DeviceName { get; set; } 
        public string AndroidVersion { get; set; }
    }
}
