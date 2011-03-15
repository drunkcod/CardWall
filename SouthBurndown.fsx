#r @"Site\bin\CardWall.Core.dll"

open System
open System.Linq
open System.Xml.XPath
open CardWall

(*
let tracker = PivotalTracker(Environment.GetEnvironmentVariable("TrackerToken", EnvironmentVariableTarget.Machine))
tracker.Stories(projectId).Result
*)
let projectId = 173053

let snapshotPath date project = String.Format(@"R:\PivotalSnapshots\{0}\{1}.xml", date, project)

let date = "2011-02-21"

let xml = XPathDocument(snapshotPath date projectId)

let stories = 
    xml.CreateNavigator()
    |> XPath.map "//story" (fun x -> x.ReadSubtree() |> Xml.read (PivotalStory()))

stories
|> Seq.filter (fun x -> x.Labels |> Seq.exists (fun x -> x.ToLowerInvariant() = "team south"))
|> Seq.filter (fun x -> x.CurrentState <> PivotalStoryState.Accepted)
|> Seq.filter (fun x -> x.Estimate.HasValue)
|> Seq.sumBy (fun x -> Math.Max(0, x.Estimate.Value))
|> fun x -> Console.WriteLine("{0}, {1}", date, x)