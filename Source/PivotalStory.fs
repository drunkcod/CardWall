namespace CardWall

open System
open System.Collections.Generic
open System.Xml
open System.Xml.Serialization

type PivotalStory() =
    [<DefaultValue>] val mutable private id : int
    [<DefaultValue>] val mutable private projectId : int
    let mutable createdAt = DateTime.MinValue
    let mutable acceptedAt = DateTime.MinValue
    let mutable storyType = PivotalStoryType.Unknown
    let mutable  estimate = Nullable<int>()
    let mutable  state = PivotalStoryState.Unknown
    let mutable  url = String.Empty
    let mutable  name = String.Empty
    let mutable  ownedBy = String.Empty
    let mutable labels = [||] : string[]
    let tasks = List<PivotalTask>()

    member this.Id = this.id
    member this.ProjectId = this.projectId
    member this.CreatedAt = createdAt
    member this.AcceptedAt = acceptedAt
    member this.Type = storyType
    member this.Estimate = estimate
    member this.CurrentState = state
    member this.Url = url
    member this.Name = name
    member this.OwnedBy = ownedBy
    member this.Labels = labels
    member this.Tasks = tasks

    interface IXmlSerializable with
        member this.GetSchema() = null
        member this.ReadXml reader = 
            reader 
            |> PivotalParse.readXmlElements (fun reader ->
                match reader.Name with
                | "id" -> this.id <- reader.ReadElementContentAsInt()
                | "project_id" -> this.projectId <- reader.ReadElementContentAsInt()
                | "created_at" -> createdAt <- PivotalParse.dateTime(reader.ReadElementContentAsString())
                | "accepted_at" -> acceptedAt <- PivotalParse.dateTime(reader.ReadElementContentAsString())
                | "story_type" -> storyType <- PivotalParse.storyType(reader.ReadElementContentAsString())
                | "estimate" -> estimate <- Nullable(reader.ReadElementContentAsInt())
                | "current_state" -> state <- PivotalParse.storyState(reader.ReadElementContentAsString())
                | "url" -> url <- reader.ReadElementContentAsString()
                | "name" -> name <- reader.ReadElementContentAsString()
                | "owned_by" -> ownedBy <- reader.ReadElementContentAsString()
                | "labels" -> labels <- reader.ReadElementContentAsString().Split([|','|])
                | "tasks" ->
                    while reader.Read() && not (reader.Name = "tasks" && reader.NodeType = XmlNodeType.EndElement) do
                        if reader.Name = "task" then
                            let task = PivotalTask()
                            (task :> IXmlSerializable).ReadXml(reader)
                            tasks.Add(task)
                | _ -> reader.Skip())
        member this.WriteXml writer = ()