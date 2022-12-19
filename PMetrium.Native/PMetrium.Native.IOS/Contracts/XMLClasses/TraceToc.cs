using System.Xml.Serialization;

namespace PMetrium.Native.IOS.Contracts.XMLClasses
{
    [XmlRoot(ElementName = "device")]
    public class Device
    {
        [XmlAttribute(AttributeName = "uuid")] public string Uuid { get; set; }

        [XmlAttribute(AttributeName = "model")]
        public string Model { get; set; }

        [XmlAttribute(AttributeName = "name")] public string Name { get; set; }

        [XmlAttribute(AttributeName = "os-version")]
        public string OSVersion { get; set; }

        [XmlAttribute(AttributeName = "platform")]
        public string Platform { get; set; }
    }

    [XmlRoot(ElementName = "host-device")]
    public class HostDevice
    {
        [XmlAttribute(AttributeName = "uuid")] public string Uuid { get; set; }

        [XmlAttribute(AttributeName = "model")]
        public string Model { get; set; }

        [XmlAttribute(AttributeName = "name")] public string Name { get; set; }

        [XmlAttribute(AttributeName = "os-version")]
        public string OSVersion { get; set; }

        [XmlAttribute(AttributeName = "platform")]
        public string Platform { get; set; }
    }

    [XmlRoot(ElementName = "target")]
    public class Target
    {
        [XmlElement(ElementName = "device")] public Device Device { get; set; }

        [XmlElement(ElementName = "host-device")]
        public HostDevice HostDevice { get; set; }

        [XmlElement(ElementName = "all-processes")]
        public string AllProcesses { get; set; }
    }

    [XmlRoot(ElementName = "summary")]
    public class Summary
    {
        [XmlElement(ElementName = "start-date")]
        public DateTime StartDate { get; set; }

        [XmlElement(ElementName = "end-date")] public DateTime EndDate { get; set; }
        [XmlElement(ElementName = "duration")] public string Duration { get; set; }

        [XmlElement(ElementName = "end-reason")]
        public string EndReason { get; set; }

        [XmlElement(ElementName = "instruments-version")]
        public string InstrumentsVersion { get; set; }

        [XmlElement(ElementName = "template-name")]
        public string TemplateName { get; set; }

        [XmlElement(ElementName = "recording-mode")]
        public string RecordingMode { get; set; }

        [XmlElement(ElementName = "time-limit")]
        public string TimeLimit { get; set; }
    }

    [XmlRoot(ElementName = "info")]
    public class Info
    {
        [XmlElement(ElementName = "target")] public Target Target { get; set; }
        [XmlElement(ElementName = "summary")] public Summary Summary { get; set; }
    }

    [XmlRoot(ElementName = "table")]
    public class Table
    {
        [XmlAttribute(AttributeName = "schema")]
        public string Schema { get; set; }

        [XmlAttribute(AttributeName = "frequency")]
        public string Frequency { get; set; }

        [XmlAttribute(AttributeName = "target-pid")]
        public string TargetPid { get; set; }

        [XmlAttribute(AttributeName = "codes")]
        public string Codes { get; set; }

        [XmlAttribute(AttributeName = "target")]
        public string Target { get; set; }

        [XmlAttribute(AttributeName = "callstack")]
        public string CallStack { get; set; }
    }

    [XmlRoot(ElementName = "data")]
    public class Data
    {
        [XmlElement(ElementName = "table")] public List<Table> Table { get; set; }
    }

    [XmlRoot(ElementName = "run")]
    public class Run
    {
        [XmlElement(ElementName = "info")] public Info Info { get; set; }
        [XmlElement(ElementName = "data")] public Data Data { get; set; }
        [XmlElement(ElementName = "tracks")] public string Tracks { get; set; }

        [XmlAttribute(AttributeName = "number")]
        public string Number { get; set; }
    }

    [XmlRoot(ElementName = "trace-toc")]
    public class TraceToc
    {
        [XmlElement(ElementName = "run")] public List<Run> Run { get; set; }
    }
}