namespace PMetrium.Native.Common.Contracts;

public class ComplexEvent
{
    public DateTime Timestamp { get; set; }
    public string Name { get; set; }
    public double Latency { get; set; }
}