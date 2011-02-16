using System.Xml;
using System.Xml.Serialization;
using CardWall.Models;

namespace CardWall
{
    [XmlRoot("QuickLinks")]
    public class QuickLinkConfiguration : WebConfigurationSectionHandler
    {
        [XmlElement(ElementName = "Link")]
        public QuickLink[] Links { get; set; }
    }
}