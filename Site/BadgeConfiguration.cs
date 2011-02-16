using System.Xml;
using System.Xml.Serialization;
using CardWall.Models;

namespace CardWall
{
    [XmlRoot("Badges")]
    public class BadgeConfiguration : WebConfigurationSectionHandler
    {
        [XmlElement(ElementName = "Badge")]
        public CardBadge[] Badges { get; set; }
    }
}