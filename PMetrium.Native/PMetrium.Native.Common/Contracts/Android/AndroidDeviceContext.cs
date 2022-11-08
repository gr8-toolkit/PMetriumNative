using System.Diagnostics;

namespace PMetrium.Native.Common.Contracts
{
    public class AndroidDeviceContext
    {
        public Process Process { get; set; }
        public AndroidDeviceParameters DeviceParameters { get; set; }
        public Dictionary<string, string> AnnotationTags { get; set; } = new ();
        public Dictionary<string, string> CommonTags { get; set; } = new ();
        public string DeviceName { get; set; } 
        public string AndroidVersion { get; set; }
    }
}
