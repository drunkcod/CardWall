namespace CardWall
open System
open System.Linq
open System.Text.RegularExpressions
open System.Xml.XPath

type Configuration = {
    ProjectId : int
    Date : string option
    Team : string
}

module Config = 
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
            | "team" -> { config with Team = m.Groups.[value].Value }
            | _ as x -> raise(new ArgumentException(x))
          | NonMatch -> raise(ArgumentException(arg))

    let load() = 
        fsi.CommandLineArgs
        |> Seq.skip 1
        |> Seq.fold parseArg { ProjectId = 173053; Date = None; Team = String.Empty }