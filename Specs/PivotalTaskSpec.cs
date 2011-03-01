using System.Xml;
using System.Xml.Serialization;
using Cone;
using System;

namespace CardWall.Specs
{
    [Describe(typeof(PivotalTask))]
    public class PivotalTaskSpec
    {
        [Context("XML serialization")]
        public class XmlSerialization
        {
            PivotalTask Task;

            [BeforeAll]
            public void ReadSampleStory() {
                Task = new PivotalTask();
                using(var xml = XmlReader.Create(GetType().Assembly.GetManifestResourceStream("CardWall.Specs.SampleTask.xml")))
                (Task as IXmlSerializable).ReadXml(xml);
            }

            public void Id() { Verify.That(() => Task.Id == 1); }

            public void Description() { Verify.That(() => Task.Description == "find shields"); }
            
            public void Position() { Verify.That(() => Task.Position == 1); }

            public void IsComplete() { Verify.That(() => Task.IsComplete == false); }

            public void CreatedAt() { Verify.That(() => Task.CreatedAt == new DateTime(2008, 12, 10, 0, 0, 0).ToLocalTime()); }
        }
    }
}
