namespace CardWall

open System
open System.Runtime.CompilerServices
open System.Text

type IChartDataEncoding =
    abstract member MaxValue : int
    abstract member Encode : data:seq<int> -> string

[<Extension>]
module ChartDataEncoding =
    [<CompiledName("Scale"); Extension>]
    let scale (encoding:IChartDataEncoding) data max =
        data |> Seq.map (fun x -> encoding.MaxValue * x / max)

type GoogleExtendedEncoding() =
    [<Literal>] 
    let Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmonpqrstuvwxyz0123456789.-"
    
    member x.MaxValue = 4095

    interface IChartDataEncoding with
        member this.MaxValue = this.MaxValue
        member this.Encode data =
            let result = StringBuilder()
            data |> Seq.iter (fun value ->                
                let d,r = Math.DivRem(this.Check value, Alphabet.Length)
                result.Append([|Alphabet.[d]; Alphabet.[r]|]) |> ignore)
            result.ToString()

    member private this.Check value = 
        if value < 0 || value > this.MaxValue then
            raise(ArgumentOutOfRangeException("value", value.ToString() + " was out of range!"))
        else value
