namespace CardWall

open System.Xml.XPath

module XPath =
    let map (xpath:string) f (navigator:XPathNavigator) =
        let nodes = navigator.Select(xpath)
        seq { while nodes.MoveNext() do yield f(nodes.Current) }

[<AutoOpen>]
module XPathExtensions =
    type System.Xml.XPath.XPathNavigator with
        member this.NodeValueOrDefault (xpath:string, defaultValue:string) = 
            match this.SelectSingleNode(xpath) with
            | null -> defaultValue
            | node -> node.Value