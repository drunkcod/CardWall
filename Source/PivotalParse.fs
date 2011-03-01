namespace CardWall

open System
open System.Globalization
open System.Text.RegularExpressions

module PivotalParse = 
    let dateTime =
        let timex = Regex("(CES?T|UTC)$", RegexOptions.Compiled)
        let timeOffset = MatchEvaluator(fun m -> 
            match m.Groups.[1].Value with
                | "UTC" -> "Z"
                | "CET" -> "+0100" 
                | "CEST" -> "+0200" 
                | x -> x)
        fun s ->
            let timeWithOffset = timex.Replace(s, timeOffset)
            DateTime.ParseExact(timeWithOffset, "yyyy/MM/dd HH:mm:ss K", CultureInfo.InvariantCulture)
