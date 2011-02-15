using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Serialization;
using CardWall.Models;

namespace CardWall
{  
    [XmlRoot("Badges")]
    public class BadgeConfiguration : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            var serializer = new XmlSerializer(GetType());
            var config = (BadgeConfiguration)serializer.Deserialize(new XmlNodeReader(section));
            var path = config.FilePath;
            if(string.IsNullOrEmpty(path))
                return config;
            var httpConfig = configContext as HttpConfigurationContext;
            if(httpConfig != null)
                path = HttpContext.Current.Server.MapPath(httpConfig.VirtualPath + "/" + path);
            
            using(var file = File.OpenRead(path))
                return serializer.Deserialize(file);
        }

        [XmlAttribute("file")]
        public string FilePath { get; set; }

        [XmlElement(ElementName = "Badge")]
        public CardBadge[] Badges { get; set; }

    }
}