using System.Xml.Serialization;
using CardWall.Models;

namespace CardWall
{
	[XmlRoot("Themes")]
	public class ThemeConfiguration : WebConfigurationSectionHandler
	{
		[XmlElement(ElementName = "Theme")]
		public Theme[] Themes { get; set; }
	}
}