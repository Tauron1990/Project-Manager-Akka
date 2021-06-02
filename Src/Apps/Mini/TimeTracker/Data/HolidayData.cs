using System;
using System.Collections.Immutable;

namespace TimeTracker.Data
{
    public sealed record Laender(string Name, string Abkuerzung);

    public sealed record FeiertagInfo(string Name, ImmutableList<Laender> Laender);

    public sealed record Feiertage(DateTime Datum, FeiertagInfo FeiertagInfo);

    //[XmlRoot(ElementName = "Bundesland", Namespace = "http://schemas.datacontract.org/2004/07/FeiertagAPI")]
    //public class Bundesland
    //{
    //    [XmlElement(ElementName = "Abkuerzung", Namespace = "http://schemas.datacontract.org/2004/07/FeiertagAPI")]
    //    public string Abkuerzung { get; set; }

    //    [XmlElement(ElementName = "Name", Namespace = "http://schemas.datacontract.org/2004/07/FeiertagAPI")]
    //    public string Name { get; set; }
    //}

    //[XmlRoot(ElementName = "Laender", Namespace = "http://schemas.datacontract.org/2004/07/FeiertagAPI")]
    //public class Laender
    //{
    //    [XmlElement(ElementName = "Bundesland", Namespace = "http://schemas.datacontract.org/2004/07/FeiertagAPI")]
    //    public Bundesland Bundesland { get; set; }
    //}

    //[XmlRoot(ElementName = "Feiertag", Namespace = "http://schemas.datacontract.org/2004/07/FeiertagAPI")]
    //public class Feiertag
    //{
    //    [XmlElement(ElementName = "Laender", Namespace = "http://schemas.datacontract.org/2004/07/FeiertagAPI")]
    //    public Laender Laender { get; set; }

    //    [XmlElement(ElementName = "Name", Namespace = "http://schemas.datacontract.org/2004/07/FeiertagAPI")]
    //    public string Name { get; set; }
    //}

    //[XmlRoot(ElementName = "FeiertagDatum", Namespace = "http://schemas.datacontract.org/2004/07/FeiertagAPI")]
    //public class FeiertagDatum
    //{
    //    [XmlElement(ElementName = "Datum", Namespace = "http://schemas.datacontract.org/2004/07/FeiertagAPI")]
    //    public string Datum { get; set; }

    //    [XmlElement(ElementName = "Feiertag", Namespace = "http://schemas.datacontract.org/2004/07/FeiertagAPI")]
    //    public Feiertag Feiertag { get; set; }
    //}

    //[XmlRoot(ElementName = "ArrayOfFeiertagDatum", Namespace = "http://schemas.datacontract.org/2004/07/FeiertagAPI")]
    //public class ArrayOfFeiertagDatum
    //{
    //    [XmlElement(ElementName = "FeiertagDatum", Namespace = "http://schemas.datacontract.org/2004/07/FeiertagAPI")]
    //    public List<FeiertagDatum> FeiertagDatum { get; set; }

    //    //[XmlAttribute(AttributeName = "i", Namespace = "http://www.w3.org/2000/xmlns/")]
    //    //public string I { get; set; }

    //    //[XmlAttribute(AttributeName = "xmlns")]
    //    //public string Xmlns { get; set; }
    //}
}