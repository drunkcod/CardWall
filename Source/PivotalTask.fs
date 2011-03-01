namespace CardWall

open System
open System.Xml
open System.Xml.Serialization

type PivotalTask() =
    [<DefaultValue>] val mutable private id : int
    [<DefaultValue>] val mutable private description : string
    [<DefaultValue>] val mutable private position : int
    [<DefaultValue>] val mutable private complete : bool
    [<DefaultValue>] val mutable private createdAt : DateTime

    member this.Id = this.id
    member this.Description = this.description
    member this.Position = this.position
    member this.IsComplete = this.complete
    member this.CreatedAt = this.createdAt

    interface IXmlSerializable with
        member this.GetSchema() = null
        member this.ReadXml reader = 
            reader 
            |> PivotalParse.readXmlElements (fun reader ->
                match reader.Name with
                | "id" -> this.id <- reader.ReadElementContentAsInt()
                | "description" -> this.description <- reader.ReadElementContentAsString()
                | "position" -> this.position <- reader.ReadElementContentAsInt()
                | "complete" -> this.complete <- reader.ReadElementContentAsBoolean()
                | "created_at" -> this.createdAt <- reader.ReadElementContentAsString() |> PivotalParse.dateTime
                | _ -> reader.Skip())

        member this.WriteXml writer = ()