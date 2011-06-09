#r @"Site\bin\CardWall.Core.dll"
#load "ChartDataConfiguration.fsx"

open System
open System.IO
open System.Collections.Generic
open System.Data
open System.Data.SqlClient
open System.Linq
open System.Text.RegularExpressions
open System.Xml.XPath
open System.Reflection
open CardWall

module Member =
    let getValue (mi:MemberInfo) x =
        match mi.MemberType with
        | MemberTypes.Field -> (mi :?> FieldInfo).GetValue(x)
        | MemberTypes.Property -> (mi :?> PropertyInfo).GetValue(x, null)
        | _ -> raise(InvalidOperationException())

let getStories date project = 
  match date with
  | Some(date) ->
    let snapshotPath date project = String.Format(@"R:\PivotalSnapshots\{0}\{1}.xml", date, project)
    let xml = XPathDocument(snapshotPath date project)
    xml.CreateNavigator()
    |> XPath.map "//story" (fun x -> x.ReadSubtree() |> Xml.read (PivotalStory()))
  | None ->
    let trackerToken = Environment.GetEnvironmentVariable("TrackerToken", EnvironmentVariableTarget.Machine)
    let tracker = PivotalTracker(trackerToken)
    tracker.Stories(project).Result

let stories =
  let getStories = getStories (Some("2011-05-30"))
  [("CSA3", 173053)
  ;("Cint Sampling", 13482)
  ;("Panelist API", 263407)
  ;("Cint Direct Sample", 11621)
  ;("Cint Direct Revenue", 23336)
  ;("Cint Panel Management", 115657)
  ;("Cint Automated Tracker", 29583)]
  |> Seq.collect (fun (x,p) -> getStories p |> Seq.map (fun s -> x,s))

let data = new ObjectDataReader<string * PivotalStory>(stories)

data.AddMember("Project", fun (x,_) -> x)
data.AddMember("Id", fun (_, x:PivotalStory) -> x.Id)
data.AddMember("Type", fun (_,x:PivotalStory) -> x.Type)
data.AddMember("CurrentState", fun (_,x:PivotalStory) -> x.CurrentState)
data.AddMember("Name", fun (_, x:PivotalStory) -> x.Name)
data.AddMember("RequestedBy", fun (_,x:PivotalStory) -> x.RequestedBy)
data.AddMember("OwnedBy", fun (_,x:PivotalStory) -> x.OwnedBy)
data.AddMember("CreatedAt", fun (_,x:PivotalStory) -> x.CreatedAt)
data.AddMember("AcceptedAt", fun (_,x:PivotalStory) -> x.AcceptedAt)

let db = new SqlConnection("Server=.;Database=Stuff;Integrated Security=SSPI")
let bulkCopy = new SqlBulkCopy(db)

bulkCopy.BulkCopyTimeout <- 0
bulkCopy.DestinationTableName <- "Stories"


for i = 0 to data.FieldCount - 1 do
    let name = data.GetName i
    bulkCopy.ColumnMappings.Add(name, name) |> ignore

db.Open()
bulkCopy.WriteToServer(data)
