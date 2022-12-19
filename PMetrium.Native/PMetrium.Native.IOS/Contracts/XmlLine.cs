namespace PMetrium.Native.IOS.Contracts;

public class XmlLine
{
    public int? Id { get; set; }
    public int? Ref { get; set; }
    public string Text { get; set; }
    public List<XmlLine> InternalXmlLines { get; set; }
}