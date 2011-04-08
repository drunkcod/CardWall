#r @"Site\bin\CardWall.Core.dll"
#load "ChartDataConfiguration.fsx"

open System
open System.Linq
open System.Xml.XPath
open CardWall

let config = Config.load()

let projectId = config.ProjectId

let snapshotPath date project = String.Format(@"R:\PivotalSnapshots\{0}\{1}.xml", date, project)

let date, stories =
  match config.Date with
  | Some(date) ->
      let snapshotPath date project = String.Format(@"R:\PivotalSnapshots\{0}\{1}.xml", date, project)
      let xml = XPathDocument(snapshotPath date config.ProjectId)
      DateTime.Parse(date), xml.CreateNavigator()
      |> XPath.map "//story" (fun x -> x.ReadSubtree() |> Xml.read (PivotalStory()))
      |> Seq.cache

  | None ->
    let trackerToken = Environment.GetEnvironmentVariable("TrackerToken", EnvironmentVariableTarget.Machine)
    let tracker = PivotalTracker(trackerToken)
    DateTime.Today, tracker.Stories(config.ProjectId).Result |> Seq.cache

let teamStories =
    stories
    |> Seq.filter (fun x -> x.Labels |> Seq.exists (fun x -> x.ToLowerInvariant() = "team south"))
    |> Seq.filter (fun x -> x.Estimate.HasValue)

let pointsRemaining = 
    teamStories 
    |> Seq.filter (fun x -> x.CurrentState <> PivotalStoryState.Accepted)
    |> Seq.sumBy (fun x -> Math.Max(0, x.Estimate.Value))

let pointsBurned = 
    teamStories 
    |> Seq.filter (fun x -> x.CurrentState = PivotalStoryState.Accepted)
    |> Seq.sumBy (fun x -> Math.Max(0, x.Estimate.Value))

Console.WriteLine(BurndownDataPoint(date, pointsRemaining, pointsBurned))
