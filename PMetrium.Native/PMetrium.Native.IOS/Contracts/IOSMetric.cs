using PMetrium.Native.IOS.Contracts;

namespace TestXml.Contracts;

public class IOSMetric<T>
{
    public T MetricType { get; set; }
    public XmlLine MetricData { get; set; }
}