using System.Xml.Serialization;

namespace CardWall.Models
{
    public class QuickLink 
    {
        [XmlAttribute("path")]
        public string Path;

        [XmlAttribute("projects")]
        public string Projects;
    }
}