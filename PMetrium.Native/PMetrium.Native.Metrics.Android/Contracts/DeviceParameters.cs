using System.ComponentModel;

namespace PMetrium.Native.Metrics.Android.Contracts
{
    public class DeviceParameters
    {
        public string Device { get; set; }
        public string App { get; set; }
        public string CpuTotal { get; set; } = "yes";
        public string CpuApp { get; set; } = "yes";
        public string RamTotal { get; set; } = "yes";
        public string RamApp { get; set; } = "yes";
        [Description("Requires ROOT")] public string NetworkApp { get; set; } = "yes";
        public string BatteryApp { get; set; } = "yes";
        public string FramesApp { get; set; } = "yes";
        public string Space { get; set; } = "default";
        public string Group { get; set; } = "default";
        public string Label { get; set; } = "default";
    }
}