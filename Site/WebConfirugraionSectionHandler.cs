using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Serialization;

namespace CardWall
{
    public class WebConfigurationSectionHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            var serializer = new XmlSerializer(GetType());
            var config = (WebConfigurationSectionHandler)serializer.Deserialize(new XmlNodeReader(section));
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
    }
}