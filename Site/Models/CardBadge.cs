using System.Xml.Serialization;

namespace CardWall.Models
{
    public class CardBadge
    {
        [XmlAttribute("key")]
        public string Key;

        [XmlAttribute("name")]
        public string Name;
        
        [XmlAttribute("url")]
        public string Url;
    }
}