namespace PMetrium.Native.Common.Contracts
{
    public class AndroidDeviceParameters
    {
        public string Device { get; set; }
        public string App { get; set; }
        public bool CpuTotal { get; set; }
        public bool CpuApp { get; set; }
        public bool RamTotal { get; set; }
        public bool RamApp { get; set; }
        public bool NetworkTotal { get; set; }
        public bool NetworkApp { get; set; }
        public bool BatteryApp { get; set; }
        public bool FramesApp { get; set; }
        public string Space { get; set; }
        public string Group { get; set; }
        public string Label { get; set; }
    }
}