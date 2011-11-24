using System.Xml.Serialization;

namespace CardWall.Models
{
	public class Theme
	{
		[XmlAttribute("key")]
		public string Key;

		[XmlAttribute("printImageUrl")]
		public string PrintImageUrl;
	}
}