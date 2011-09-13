#r @"Site\bin\CardWall.Core.dll"
#r "System.Transactions"
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
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.DerivedPatterns

type Configuration = {
    Date : DateTime option
    Path : String option
    Project : int
    ConnectionString : string
}

module Config = 
    let (|Match|NonMatch|) (m:Match) = if m.Success then Match(m) else NonMatch

    let parseArg =
        let r = Regex("^--(?<key>[^\s]+?)=(?<value>.+)$")
        let key = r.GroupNumberFromName("key")
        let value = r.GroupNumberFromName("value")
        fun (config:Configuration) arg ->
          match r.Match(arg) with
          | Match(m) ->
            match m.Groups.[key].Value with
            | "date" -> { config with Date = Some(DateTime.Parse(m.Groups.[value].Value)) }
            | "file" -> { config with Path = Some(m.Groups.[value].Value) }
            | _ as x -> raise(new ArgumentException(x))
          | NonMatch -> raise(ArgumentException(arg))

    let load() = 
        fsi.CommandLineArgs
        |> Seq.skip 1
        |> Seq.fold parseArg { Date = None; Path = None; Project = 173053; ConnectionString = "Server=.;Database=Tracker;Integrated Security=SSPI" }

module Member =
       
    let getValue (mi:MemberInfo) x =
        match mi.MemberType with
        | MemberTypes.Field -> (mi :?> FieldInfo).GetValue(x)
        | MemberTypes.Property -> (mi :?> PropertyInfo).GetValue(x, null)
        | _ -> raise(InvalidOperationException())

let getStories path project =
  match path with
  | Some(path:string) ->
    let xml = XPathDocument(path)
    xml.CreateNavigator()
    |> XPath.map "//story" (fun x -> x.ReadSubtree() |> Xml.read (PivotalStory()))
  | None ->
    let trackerToken = Environment.GetEnvironmentVariable("TrackerToken", EnvironmentVariableTarget.Machine)
    let tracker = PivotalTracker(trackerToken)
    tracker.Stories(project).Result

let config = Config.load()

let snapshotPath (date:DateTime) project = String.Format(@"R:\PivotalSnapshots\{0}\{1}.xml", date.ToShortDateString(), project)

let getPath (x:Configuration) =
    if x.Path.IsSome then
        x.Path
    else if x.Date.IsSome then
        Some(snapshotPath (Option.get x.Date) x.Project)
    else None 

let importStories() = 
    let stories =
      let getStories = getStories (getPath config)
      [("CSA3", 173053)]
      |> Seq.collect (fun (x,p) -> getStories p)

    let data = new ObjectDataReader<_>(stories)

    let getSnapshotDate =     
        let now = DateTime.Now
        fun (x:Configuration) -> 
            if x.Date.IsSome then
                Option.get x.Date
            else if x.Path.IsSome then
                let fi = FileInfo((Option.get x.Path))
                fi.CreationTime
            else now

    data.AddMember("SnapshotDate", fun x -> getSnapshotDate config)
    data.AddMember("Project", fun (x:PivotalStory) -> x.ProjectId)
    data.AddMember("Id", fun (x:PivotalStory) -> x.Id)
    data.AddMember("Type", fun (x:PivotalStory) -> x.Type)
    data.AddMember("CurrentState", fun (x:PivotalStory) -> x.CurrentState)
    data.AddMember("Estimate", fun (x:PivotalStory) -> x.Estimate)
    data.AddMember("Name", fun (x:PivotalStory) -> x.Name)
    data.AddMember("RequestedBy", fun (x:PivotalStory) -> x.RequestedBy)
    data.AddMember("OwnedBy", fun (x:PivotalStory) -> x.OwnedBy)
    data.AddMember("CreatedAt", fun (x:PivotalStory) -> x.CreatedAt)
    data.AddMember("AcceptedAt", fun (x:PivotalStory) -> x.AcceptedAt)

    let db = new SqlConnection(config.ConnectionString)
    let bulkCopyStories = new SqlBulkCopy(db, DestinationTableName = "#Stories")

    for i = 0 to data.FieldCount - 1 do
        let name = data.GetName i
        bulkCopyStories.ColumnMappings.Add(name, name) |> ignore

    let bulkCopyLables = new SqlBulkCopy(db, DestinationTableName = "#StoryLabels")
    let labelData = new ObjectDataReader<_>(stories |> Seq.collect (fun x -> x.Labels |> Seq.map (fun l -> x.Id, l)))

    labelData.AddMember("StoryId", fst)
    labelData.AddMember("Label", snd)

    db.Open()
    let cmd = db.CreateCommand()
    cmd.CommandText <- "
        create table #Stories(
	    [SnapshotDate] datetime,
	    [Project] int,
	    [Id] int,
	    [Type] varchar(max),
	    [CurrentState] varchar(max),
	    [Estimate] int,
	    [Name] varchar(max),
	    [RequestedBy] varchar(max),
	    [OwnedBy] varchar(max),
	    [CreatedAt] datetime,
	    [AcceptedAt] datetime)"

    cmd.ExecuteNonQuery() |> ignore

    cmd.CommandText <- "
        create table #StoryLabels(
            StoryId int,
            Label varchar(256))"
    cmd.ExecuteNonQuery() |> ignore
    
    bulkCopyStories.WriteToServer(data)
    bulkCopyLables.WriteToServer(labelData)

    cmd.CommandText <- "
        begin tran
            insert Stories select * from #Stories
            
            insert Labels 
            select distinct Label from #StoryLabels 
            where Label not in(select Label from Labels)

            insert StoryLabels
            select distinct StoryId, Labels.Id
            from #StoryLabels
            inner join #Stories on #Stories.Id = #StoryLabels.StoryId
            inner join Labels on Labels.Label = #StoryLabels.Label
        commit"
    cmd.ExecuteNonQuery() |> ignore

importStories()
