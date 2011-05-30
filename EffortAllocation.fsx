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

type TypedDataReader(xs:'a seq) =
  let items = xs.GetEnumerator()
  let fields = Dictionary<String, int>()
  let keys = List<'a -> obj>()
  let types = List()

  member this.Item with get name = this.[fields.[name]]
  member this.Item with get (index:int) = keys.[index] items.Current
  member this.FieldCount = fields.Count
  member this.GetName i = fields.Single(fun x -> x.Value = i).Key

  member this.Read() = items.MoveNext()

  member this.AddMember(name, f:'a -> 'b)=
    fields.Add(name, keys.Count)
    keys.Add(f >> box)
    types.Add(typeof<'b>)

  member private this.NotSupported() = raise(NotSupportedException())

  interface IDataReader with
    member this.Depth = 0
    member this.IsClosed = false
    member this.RecordsAffected = 0
    member this.Close() = ()
    member this.NextResult() = false
    member this.Dispose() = items.Dispose()
    member this.FieldCount = this.FieldCount
    member this.Item with get (index:int) = this.[index]
    member this.Item with get (name:string) = this.[name]
    member this.GetName i = this.GetName i
    member this.GetDataTypeName i = types.[i].Name
    member this.GetFieldType i = types.[i]
    member this.GetValue i = this.[i]
    member this.GetValues values = 
      let len = Math.Max(values.Length, keys.Count)
      for i = 0 to len - 1 do
        values.[i] <- this.[i]
      len
    member this.GetOrdinal name = fields.[name]
    member this.GetBoolean i = this.NotSupported()
    member this.GetByte i = this.NotSupported()
    member this.GetChar i = this.NotSupported()
    member this.GetGuid i = this.NotSupported()
    member this.GetInt16 i = this.NotSupported()
    member this.GetInt32 i = this.NotSupported()
    member this.GetInt64 i = this.NotSupported()
    member this.GetFloat i = this.NotSupported()
    member this.GetDouble i = this.NotSupported()
    member this.GetString i = this.NotSupported()
    member this.GetDecimal i = this.NotSupported()
    member this.GetDateTime i = this.NotSupported()
    member this.GetData i = this.NotSupported()
    member this.IsDBNull i = this.NotSupported()
    member this.Read() = this.Read()
    member this.GetSchemaTable() = this.NotSupported()
    member this.GetBytes(i, fieldOffset, buffer, bufferOffset, length) = this.NotSupported()
    member this.GetChars(i, fieldOffset, buffer, bufferOffset, length) = this.NotSupported()

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

let data = new TypedDataReader(stories)

data.AddMember("Project", fun (x,_) -> x)
data.AddMember("StoryId", fun (_, x) -> x.Id)
data.AddMember("StoryType", fun (_,x) -> x.Type)
data.AddMember("StoryState", fun (_,x) -> x.CurrentState)
data.AddMember("Name", fun (_, x) -> x.Name)
data.AddMember("RequestedBy", fun (_,x) -> x.RequestedBy)
data.AddMember("OwnedBy", fun (_,x) -> x.OwnedBy)
data.AddMember("CreatedAt", fun (_,x) -> x.CreatedAt)
data.AddMember("AcceptedAt", fun (_,x) -> if x.AcceptedAt = DateTime.MinValue then null else box x.AcceptedAt)

let db = new SqlConnection("Server=.;Database=Stuff;Integrated Security=SSPI")
let bulkCopy = new SqlBulkCopy(db)

bulkCopy.BulkCopyTimeout <- 0
bulkCopy.DestinationTableName <- "Stories"


for i = 0 to data.FieldCount - 1 do
    let name = data.GetName i
    bulkCopy.ColumnMappings.Add(name, name) |> ignore

db.Open()
bulkCopy.WriteToServer(data)
