namespace CardWall

open System
open System.Collections.Generic
open System.Xml
open System.Xml.Serialization

type PivotalStoryType = 
    | Unknown = 0
    | Bug = 1
    | Chore = 2
    | Feature = 3

type PivotalStoryState =
    | Unknown = 0
    | Unscheduled = 1
    | Unstarted = 2
    | Started = 3
    | Finished = 4
    | Delivered = 5
    | Accepted = 6

type PivotalStory() =
    [<DefaultValue>] val mutable private id : int
    [<DefaultValue>] val mutable private projectId : int
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
    member this.Type = storyType
    member this.Estimate = estimate
    member this.CurrentState = state
    member this.Url = url
    member this.Name = name
    member this.OwnedBy = ownedBy
    member this.Labels = labels
    member this.Tasks = tasks

    static member private ParseStoryType =
        function
        | "feature" -> PivotalStoryType.Feature
        | "bug" -> PivotalStoryType.Bug
        | "chore" -> PivotalStoryType.Chore
        | _ -> PivotalStoryType.Unknown

    static member private ParseStoryState =
        function
        | "unscheduled" -> PivotalStoryState.Unscheduled
        | "unstarted" -> PivotalStoryState.Unstarted
        | "started" -> PivotalStoryState.Started
        | "finished" -> PivotalStoryState.Finished
        | "delivered" -> PivotalStoryState.Delivered
        | "accepted" -> PivotalStoryState.Accepted
        | _ -> PivotalStoryState.Unknown

    interface IXmlSerializable with
        member this.GetSchema() = null
        member this.ReadXml reader = 
            reader 
            |> PivotalParse.readXmlElements (fun reader ->
                match reader.Name with
                | "id" -> this.id <- reader.ReadElementContentAsInt()
                | "project_id" -> this.projectId <- reader.ReadElementContentAsInt()
                | "story_type" -> storyType <- PivotalStory.ParseStoryType(reader.ReadElementContentAsString())
                | "estimate" -> estimate <- Nullable(reader.ReadElementContentAsInt())
                | "current_state" -> state <- PivotalStory.ParseStoryState(reader.ReadElementContentAsString())
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