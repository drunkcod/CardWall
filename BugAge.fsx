#r @"Site\bin\CardWall.Core.dll"

open System
open System.Linq
open System.Text.RegularExpressions
open System.Xml.XPath
open CardWall

(*
let tracker = PivotalTracker(Environment.GetEnvironmentVariable("TrackerToken", EnvironmentVariableTarget.Machine))
tracker.Stories(projectId).Result
*)

type Config = {
    ProjectId : int
    Date : string option
}

let defaultConfig = { 
    ProjectId = 173053
    Date = None 
}

let parseArg config arg =
    let r = Regex("^--([^\s]+?)=(.+)$")
    let m = r.Match(arg)
    if m.Success = false then
        raise(new ArgumentException(arg))
    else 
        match m.Groups.[1].Value with
        | "date" -> { config with Date = Some(m.Groups.[2].Value) }
        | _ as x -> raise(new ArgumentException(x))

let config = 
    fsi.CommandLineArgs
    |> Seq.skip 1
    |> Seq.fold parseArg defaultConfig

let snapshotPath date project = String.Format(@"R:\PivotalSnapshots\{0}\{1}.xml", date, project)
let xml = XPathDocument(snapshotPath (Option.get config.Date) config.ProjectId)

let stories = 
    xml.CreateNavigator()
    |> XPath.map "//story" (fun x -> x.ReadSubtree() |> Xml.read (PivotalStory()))

stories
|> Seq.filter (fun x -> x.Type = PivotalStoryType.Bug && x.CurrentState = PivotalStoryState.Accepted)
|> Seq.map (fun x -> x.AcceptedAt.Subtract(x.CreatedAt))
|> Seq.countBy (fun x -> Math.Ceiling(x.TotalDays))
|> Seq.iter (fun (x, c) -> Console.WriteLine("{0} {1}", x, c))