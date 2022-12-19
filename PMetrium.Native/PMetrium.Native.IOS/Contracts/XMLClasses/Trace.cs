using System.Xml.Serialization;

namespace PMetrium.Native.IOS.Contracts.XMLClasses;

[XmlRoot(ElementName="trace-query-result")]
public class Trace {
	[XmlElement(ElementName="node")]
	public Node Node { get; set; }
}

[XmlRoot(ElementName="col")]
public class Col {
	[XmlElement(ElementName="mnemonic")]
	public string Mnemonic { get; set; }
	[XmlElement(ElementName="name")]
	public string Name { get; set; }
	[XmlElement(ElementName="engineering-type")]
	public string Engineeringtype { get; set; }
}

[XmlRoot(ElementName="schema")]
public class Schema {
	[XmlElement(ElementName="col")]
	public List<Col> Col { get; set; }
	[XmlAttribute(AttributeName="name")]
	public string Name { get; set; }
}

[XmlRoot(ElementName="start-time")]
public class Starttime {
	[XmlAttribute(AttributeName="id")]
	public string Id { get; set; }
	[XmlAttribute(AttributeName="fmt")]
	public string Fmt { get; set; }
	[XmlText]
	public string Text { get; set; }
}

[XmlRoot(ElementName="duration")]
public class Duration {
	[XmlAttribute(AttributeName="id")]
	public string Id { get; set; }
	[XmlAttribute(AttributeName="fmt")]
	public string Fmt { get; set; }
	[XmlText]
	public string Text { get; set; }
}

[XmlRoot(ElementName="cpu-percent-loads")]
public class Cpupercentloads {
	[XmlAttribute(AttributeName="id")]
	public string Id { get; set; }
	[XmlAttribute(AttributeName="fmt")]
	public string Fmt { get; set; }
	[XmlText]
	public string Text { get; set; }
}

[XmlRoot(ElementName="size-in-bytes")]
public class Sizeinbytes {
	[XmlAttribute(AttributeName="id")]
	public string Id { get; set; }
	[XmlAttribute(AttributeName="fmt")]
	public string Fmt { get; set; }
	[XmlText]
	public string Text { get; set; }
	[XmlAttribute(AttributeName="ref")]
	public string Ref { get; set; }
}

[XmlRoot(ElementName="event-count")]
public class Eventcount {
	[XmlAttribute(AttributeName="id")]
	public string Id { get; set; }
	[XmlAttribute(AttributeName="fmt")]
	public string Fmt { get; set; }
	[XmlText]
	public string Text { get; set; }
	[XmlAttribute(AttributeName="ref")]
	public string Ref { get; set; }
}

[XmlRoot(ElementName="network-size-in-bytes")]
public class Networksizeinbytes {
	[XmlAttribute(AttributeName="id")]
	public string Id { get; set; }
	[XmlAttribute(AttributeName="fmt")]
	public string Fmt { get; set; }
	[XmlText]
	public string Text { get; set; }
}

[XmlRoot(ElementName="row")]
public class Row {
	[XmlElement(ElementName="start-time")]
	public Starttime Starttime { get; set; }
	[XmlElement(ElementName="duration")]
	public Duration Duration { get; set; }
	[XmlElement(ElementName="sentinel")]
	public List<string> Sentinel { get; set; }
	[XmlElement(ElementName="cpu-percent-loads")]
	public Cpupercentloads Cpupercentloads { get; set; }
	[XmlElement(ElementName="size-in-bytes")]
	public List<Sizeinbytes> Sizeinbytes { get; set; }
	[XmlElement(ElementName="event-count")]
	public List<Eventcount> Eventcount { get; set; }
	[XmlElement(ElementName="network-size-in-bytes")]
	public List<Networksizeinbytes> Networksizeinbytes { get; set; }
	[XmlElement(ElementName="size-in-bytes-per-second")]
	public List<Sizeinbytespersecond> Sizeinbytespersecond { get; set; }
	[XmlElement(ElementName="events-per-second")]
	public List<Eventspersecond> Eventspersecond { get; set; }
	[XmlElement(ElementName="network-size-in-bytes-per-second")]
	public List<Networksizeinbytespersecond> Networksizeinbytespersecond { get; set; }
}

[XmlRoot(ElementName="size-in-bytes-per-second")]
public class Sizeinbytespersecond {
	[XmlAttribute(AttributeName="id")]
	public string Id { get; set; }
	[XmlAttribute(AttributeName="fmt")]
	public string Fmt { get; set; }
	[XmlText]
	public string Text { get; set; }
	[XmlAttribute(AttributeName="ref")]
	public string Ref { get; set; }
}

[XmlRoot(ElementName="events-per-second")]
public class Eventspersecond {
	[XmlAttribute(AttributeName="id")]
	public string Id { get; set; }
	[XmlAttribute(AttributeName="fmt")]
	public string Fmt { get; set; }
	[XmlText]
	public string Text { get; set; }
	[XmlAttribute(AttributeName="ref")]
	public string Ref { get; set; }
}

[XmlRoot(ElementName="network-size-in-bytes-per-second")]
public class Networksizeinbytespersecond {
	[XmlAttribute(AttributeName="id")]
	public string Id { get; set; }
	[XmlAttribute(AttributeName="fmt")]
	public string Fmt { get; set; }
	[XmlText]
	public string Text { get; set; }
	[XmlAttribute(AttributeName="ref")]
	public string Ref { get; set; }
}

[XmlRoot(ElementName="node")]
public class Node {
	[XmlElement(ElementName="schema")]
	public Schema Schema { get; set; }
	[XmlElement(ElementName="row")]
	public List<Row> Row { get; set; }
	[XmlAttribute(AttributeName="xpath")]
	public string Xpath { get; set; }
}



