namespace CardWall
open System.Xml.Serialization

module Xml =
    let read (target:#IXmlSerializable) source = 
        target.ReadXml(source)
        target
