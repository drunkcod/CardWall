namespace CardWall

open System
open System.Text

type IChartDataEncoding =
    abstract member Encode : data:seq<int> -> string

type GoogleExtendedEncoding() =
    [<Literal>] 
    let Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmonpqrstuvwxyz0123456789.-"
    
    member x.MaxValue = 4095

    member this.Scale(data, max) =  
        data |> Seq.map (fun x -> this.MaxValue * x / max)

    interface IChartDataEncoding with
        member this.Encode data =
            let result = StringBuilder()
            data |> Seq.iter (fun value ->                
                let d,r = Math.DivRem(this.Check value, Alphabet.Length)
                result.Append([|Alphabet.[d]; Alphabet.[r]|]) |> ignore)
            result.ToString()

    member private this.Check value = 
        if value < 0 || value > this.MaxValue then
            raise(Exception(value.ToString() + " was out of range!"))
        else value
