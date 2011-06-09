using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;

namespace CardWall
{
    public class BurndownChartConfiguration
    {
        [XmlAttribute("key")]
        public string Key;
        [XmlAttribute("title")]
        public string Title;
        [XmlElement("label")]
        public string Label;
        [XmlElement("project")]
        public int Project;
        [XmlElement("data-path")]
        public string HistoricalDataPath;
        [XmlElement("end-date")]
        public DateTime? EndDate;

        [XmlIgnore]
        public bool EndDateSpecified { get { return EndDate.HasValue; } }
    }

    [XmlRoot("BurndownCharts")]
    public class BurndownChartConfigurationSection : IConfigurationSectionHandler
    {
        [XmlElement("chart")]
        public List<BurndownChartConfiguration> Charts = new List<BurndownChartConfiguration>();
        [XmlAttribute("default")]
        public string DefaultChart;

        object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section) {
            return new XmlSerializer(GetType()).Deserialize(new XmlNodeReader(section));
        }
    }}