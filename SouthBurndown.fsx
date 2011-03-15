#r @"Site\bin\CardWall.Core.dll"

open System
open System.Linq
open CardWall

let tracker = PivotalTracker(Environment.GetEnvironmentVariable("TrackerToken", EnvironmentVariableTarget.Machine))

let projectId = 173053

tracker.Stories(projectId).Result
|> Seq.filter (fun x -> x.Labels |> Seq.exists (fun x -> x.ToLowerInvariant() = "team south"))
|> Seq.filter (fun x -> x.CurrentState <> PivotalStoryState.Accepted)
|> Seq.filter (fun x -> x.Estimate.HasValue && x.Estimate.Value > 0)
|> Seq.sumBy (fun x -> x.Estimate.Value)
|> Console.WriteLine