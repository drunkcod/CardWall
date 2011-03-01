using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Cone;
using System.Xml;
using System.Resources;
using System.Xml.XPath;
using System.IO;

namespace CardWall
{
    [Describe(typeof(PivotalStory))]
    public class PivotalStorySpec
    {
        [Context("XML serialization")]
        public class XmlSerialization
        {
            PivotalStory Story;

            [BeforeAll]
            public void ReadSampleStory() {
                Story = new PivotalStory();
                using(var xml = XmlReader.Create(GetType().Assembly.GetManifestResourceStream("CardWall.Specs.SampleStory.xml")))
                (Story as IXmlSerializable).ReadXml(xml);
            }

            public void Id() { Verify.That(() => Story.Id == 42); }
            
            public void ProjectId() { Verify.That(() => Story.ProjectId == 12345); }
            
            public void StoryType() { Verify.That(() => Story.Type == PivotalStoryType.Feature); }
            
            public void Estimate() { Verify.That(() => Story.Estimate == 1); }
            
            public void CurrentState() { Verify.That(() => Story.CurrentState == PivotalStoryState.Accepted); }
            
            public void Url() { Verify.That(() => Story.Url == "http://www.pivotaltracker.com/story/show/42"); }
            
            public void Name() { Verify.That(() => Story.Name == "More power to shields"); }
            
            public void OwnedBy() { Verify.That(() => Story.OwnedBy == "Montgomery Scott"); }
            
            public void Labels() { 
                Verify.That(() => Story.Labels.Count() == 3);
                Verify.That(() => Story.Labels[0] == "label 1");
                Verify.That(() => Story.Labels[1] == "label 2");
                Verify.That(() => Story.Labels[2] == "label 3");
            }

            public void Tasks() {
                Verify.That(() => Story.Tasks.Count() == 2);
            }

            public void multiple_stories() {
                var xml = new XPathDocument(GetType().Assembly.GetManifestResourceStream("CardWall.Specs.SampleStories.xml"));
                var storyNodes = xml.CreateNavigator().Select("//story");
                var stories = new List<PivotalStory>();
                while(storyNodes.MoveNext()) {
                    Console.WriteLine(storyNodes.Current.OuterXml);
                    var item = new PivotalStory();
                    (item as IXmlSerializable).ReadXml(storyNodes.Current.ReadSubtree());
                    stories.Add(item);
                }

                Verify.That(() => stories.Count == 2);
                Verify.That(() => stories[0].Id == 1);
                Verify.That(() => stories[1].Id == 2);
            }
        }
    }
}
