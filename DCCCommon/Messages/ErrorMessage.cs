namespace DCCCommon.Messages
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Error")]
    public class ErrorMessage
    {
        [XmlElement(ElementName = "Description")]
        public string ErrorDescription { get; set; }
    }
}