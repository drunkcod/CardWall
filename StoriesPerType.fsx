#r @"Site\bin\CardWall.Core.dll"

open System
open System.Linq
open System.Text.RegularExpressions
open System.Xml.XPath
open CardWall

type Config = {
    ProjectId : int
    Date : string option
}

let (|Match|NonMatch|) (m:Match) = if m.Success then Match(m) else NonMatch

let parseArg =
    let r = Regex("^--(?<key>[^\s]+?)=(?<value>.+)$")
    let key = r.GroupNumberFromName("key")
    let value = r.GroupNumberFromName("value")
    fun config arg ->
      match r.Match(arg) with
      | Match(m) ->
        match m.Groups.[key].Value with
        | "date" -> { config with Date = Some(m.Groups.[value].Value) }
        | "project" -> { config with ProjectId = int(m.Groups.[value].Value) }
        | _ as x -> raise(new ArgumentException(x))
      | NonMatch -> raise(ArgumentException(arg))

let config = 
    fsi.CommandLineArgs
    |> Seq.skip 1
    |> Seq.fold parseArg { ProjectId = 173053; Date = None }

let date, stories =
  match config.Date with
  | Some(date) ->
      let snapshotPath date project = String.Format(@"R:\PivotalSnapshots\{0}\{1}.xml", date, project)
      let xml = XPathDocument(snapshotPath date config.ProjectId)
      date, xml.CreateNavigator()
      |> XPath.map "//story" (fun x -> x.ReadSubtree() |> Xml.read (PivotalStory()))
  | None ->
    let trackerToken = Environment.GetEnvironmentVariable("TrackerToken", EnvironmentVariableTarget.Machine)
    let tracker = PivotalTracker(trackerToken)
    DateTime.Today.ToShortDateString(), tracker.Stories(config.ProjectId).Result

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
        let getOrZero = function None -> 0 | Some(x) -> x
        let closedCount = getOrZero <| counts.TryFind(true) 
        let openCount = getOrZero <| counts.TryFind(false)
        (openCount, closedCount)
    | None -> (0, 0))
|> Seq.iter (fun (o, c)-> Console.Write("{0};{1};", o, c))
Console.WriteLine()
