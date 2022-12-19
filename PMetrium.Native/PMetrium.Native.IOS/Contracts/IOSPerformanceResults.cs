using PMetrium.Native.Common.Contracts;

namespace PMetrium.Native.IOS.Contracts;

public class IOSPerformanceResults
{
    public List<string> Events { get; set; } = new List<string>();
    public List<ComplexEvent> ComplexEvents { get; set; } = new List<ComplexEvent>();

    public System System { get; set; } = new System();
    public Application Application { get; set; } = new Application();
}

public class System
{
    public Cpu Cpu { get; } = new Cpu();
    public Ram Ram { get; } = new Ram();
    public Disk Disk { get; } = new Disk();
    public SystemNetwork SystemNetwork { get; } = new SystemNetwork();
    public Frames Frames { get; } = new Frames();
    public Gpu Gpu { get; } = new Gpu();
}

public class Application
{
    public Cpu Cpu { get; } = new Cpu();
    public Ram Ram { get; } = new Ram();
    
    public ApplicationNetwork ApplicationNetwork { get; } = new ApplicationNetwork();
}

public class Frames
{
    public Statistic Fps { get; } = new Statistic();
}

public class Gpu
{
    public Statistic GpuUtilization_percentage { get; } = new Statistic();
}

public class Cpu
{
    public Statistic UsedCpu_percentage { get; } = new Statistic();
}

public class Ram
{
    public Statistic UsedRam_bytes { get; } = new Statistic();
}

public class Disk
{
    public Statistic DataRead_bytes_per_sec { get; } = new Statistic();
    public Statistic DataWritten_bytes_per_sec { get; } = new Statistic();
    public Statistic ReadsIn_per_sec { get; } = new Statistic();
    public Statistic WritesOut_per_sec { get; } = new Statistic();
}

public class SystemNetwork
{
    public Statistic DataReceived_bytes_per_sec { get; } = new Statistic();
    public Statistic DataSent_bytes_per_sec { get; } = new Statistic();
    public Statistic PacketsIn_per_sec { get; } = new Statistic();
    public Statistic PacketsOut_per_sec { get; } = new Statistic();
}

public class ApplicationNetwork
{
    public Statistic DataReceived_bytes_per_sec { get; } = new Statistic();
    public Statistic DataSent_bytes_per_sec { get; } = new Statistic();
    public Statistic PacketsIn_per_sec { get; } = new Statistic();
    public Statistic PacketsOut_per_sec { get; } = new Statistic();
}