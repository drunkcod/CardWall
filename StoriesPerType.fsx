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

let getOrDefault d = 
    function
    | Some(x) -> x
    | None -> d

let date = config.Date |> getOrDefault (DateTime.Today.ToShortDateString())

let snapshotPath date project = String.Format(@"R:\PivotalSnapshots\{0}\{1}.xml", date, project)
let xml = XPathDocument(snapshotPath date config.ProjectId)

let stories = 
    xml.CreateNavigator()
    |> XPath.map "//story" (fun x -> x.ReadSubtree() |> Xml.read (PivotalStory()))

let typeColumns = [PivotalStoryType.Bug; PivotalStoryType.Chore; PivotalStoryType.Feature]
let columnFilter =
    let wanted  = Set(typeColumns)
    fun (x:PivotalStory) -> wanted.Contains(x.Type)

let lookup = 
    Map(stories
    |> Seq.filter columnFilter
    |> Seq.groupBy (fun x -> x.Type))

Console.Write("{0};", date)
typeColumns
|> Seq.map (fun x -> 
    match Map.tryFind x lookup with 
    | Some(x) -> 
        let counts = Map(x |> Seq.countBy (fun x -> x.CurrentState = PivotalStoryState.Accepted))
        let getOrZero = getOrDefault 0
        let closedCount = getOrZero <| counts.TryFind(true) 
        let openCount = getOrZero <| counts.TryFind(false)
        (openCount, closedCount)
    | None -> (0, 0))
|> Seq.iter (fun (o, c)-> Console.Write("{0};{1};", o, c))
Console.WriteLine()