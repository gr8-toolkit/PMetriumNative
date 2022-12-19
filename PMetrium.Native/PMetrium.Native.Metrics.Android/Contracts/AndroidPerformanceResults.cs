using PMetrium.Native.Common.Contracts;

namespace PMetrium.Native.Metrics.Android.Contracts;

public class AndroidPerformanceResults
{
    public List<string> Events { get; set; } = new List<string>();
    public List<ComplexEvent> ComplexEvents { get; set; } = new List<ComplexEvent>();
    public Cpu Cpu { get; } = new Cpu();
    public Ram Ram { get; } = new Ram();
    public Network Network { get; } = new Network();
    public Battery Battery { get; } = new Battery();
    public Frames Frames { get; } = new Frames();
}

public class Cpu
{
    public Statistic TotalCpu_percentage { get; } = new Statistic();
    public Statistic ApplicationCpu_percentage { get; } = new Statistic();
}

public class Ram
{
    public double SystemRam_bytes { get; set; }
    public Statistic TotalUsedRam_bytes { get; } = new Statistic();
    public Statistic ApplicationPSSRam_bytes { get; } = new Statistic();
    public Statistic ApplicationPrivateRam_bytes { get; } = new Statistic();
}

public class Battery
{
    public double Application_mAh { get; set; }
}

public class Frames
{
    public double ApplicationRenderedFrames { get; set; }
    public double ApplicationJankyFrames { get; set; }
}

public class Network
{
    public NetworkSpeed NetworkSpeed { get; } = new NetworkSpeed();
    public NetworkTotal NetworkTotal { get; } = new NetworkTotal();
}

public class NetworkTotal
{
    public WiFiTotal WiFiTotal { get; } = new WiFiTotal();
    public MobileTotal MobileTotal { get; } = new MobileTotal();
}

public class WiFiTotal
{
    public DataTotal Total { get; } = new DataTotal();
    public DataTotal Application { get; } = new DataTotal();
}

public class MobileTotal
{
    public DataTotal Total { get; } = new DataTotal();
    public DataTotal Application { get; } = new DataTotal();
}

public class NetworkSpeed
{
    public WiFiSpeed WiFiSpeed { get; } = new WiFiSpeed();
    public MobileSpeed MobileSpeed { get; } = new MobileSpeed();
}

public class MobileSpeed
{
    public DataSpeed Total { get; } = new DataSpeed();
    public DataSpeed Application { get; } = new DataSpeed();
}

public class WiFiSpeed
{
    public DataSpeed Total { get; } = new DataSpeed();
    public DataSpeed Application { get; } = new DataSpeed();
}

public class DataSpeed
{
    public Statistic Rx_bytes_per_sec { get; } = new Statistic();
    public Statistic Tx_bytes_per_sec { get; } = new Statistic();
}

public class DataTotal
{
    public double Rx_bytes { get; set; }
    public double Tx_bytes { get; set; }
}

