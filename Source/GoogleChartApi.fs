namespace CardWall

open System
open System.Drawing
open System.IO
open System.Text

module GoogleChartApi =
    [<Literal>]
    let BaseUrl = "http://chart.apis.google.com/chart"

type IChartDataEncoding =
    abstract member Encode : data:seq<int> -> string

type GoogleExtendedEncoding() =
    [<Literal>] 
    let Alphabet = "ABCDEFGHIJKLMNOPQSRTUVWXYZabcdefghijklmonpqrstuvwxyz0123456789.-"
    
    member x.MaxValue = 4095

    interface IChartDataEncoding with
        member x.Encode data =
            let result = StringBuilder()
            data |> Seq.iter (fun value ->                
                let d,r = Math.DivRem(x.Check value, Alphabet.Length)
                result.Append([|Alphabet.[d]; Alphabet.[r]|]) |> ignore)
            result.ToString()

    member x.Check value = 
        if value < 0 || value > x.MaxValue then
            raise(new ArgumentOutOfRangeException(x.ToString()))
        else value

type Axis = X | Y | Top | Left

type ChartMarker =
    | Circle of Color * int * int * int
    | FillToBottom of Color * int
    | FillBetween of Color * int * int

type ChartAxis = { 
    Axis: Axis
    Range : int * int
    Labels : string seq 
    Positions : int seq }

type ChartSeries = {
    Name : string 
    Color : Color
    Data : int seq }

type LineStyle =
    | Filled of int
    | Dashed of int * int * int
    with
        static member Default = Filled(1)

type LineChartMode = Default | SparkLines | XY

type IWriter =
    abstract Write : format:string * [<ParamArray>]args:obj[] -> IWriter

type LineChart = {
    Width : int
    Height : int 
    Axes : ChartAxis seq 
    Series : ChartSeries seq 
    Markers : ChartMarker seq 
    Mode : LineChartMode 
    DataEncoding : IChartDataEncoding 
    LineStyles : LineStyle seq } with
    override x.ToString() =
        let axisToString = function
            | X -> "x"
            | Y -> "y"
            | Top -> "t"
            | Left -> "r"

        let modeString = function
            | Default -> "lc"
            | SparkLines -> "ls"
            | XY -> "lxy"

        let result = StringBuilder(GoogleChartApi.BaseUrl).AppendFormat("?cht={0}", modeString x.Mode)

        let newWriter (first:string) (next:string) =
            let appendFormat(sep:string, format, args) = result.Append(sep).AppendFormat(format, args) |> ignore
            let next' = 
                { new IWriter with
                    member this.Write(format, args) = 
                        appendFormat(next, format, args)
                        this }
            { new IWriter with
                member this.Write(format, args) =
                    appendFormat(first, format, args) 
                    next' }

        let hex (c:Color) = 
            if c.A = 255uy then
                String.Format("{0:x2}{1:x2}{2:x2}", c.R, c.G, c.B)
            else String.Format("{0:x2}{1:x2}{2:x2}{3:x2}", c.R, c.G, c.B, c.A)

        result.AppendFormat("&chs={0}x{1}", x.Width, x.Height) |> ignore

        let format = ref "&chxr={0},{1},{2}"
        x.Axes |> Seq.iteri (fun n axis ->
            match axis.Range with
            | (0, 100) -> ()
            | (min, max) ->
                result.AppendFormat(!format, n, min, max) |> ignore
                format := "|{0},{1},{2}")

        format := "&chxl={0}:"
        x.Axes |> Seq.iteri (fun n axis -> 
            if not(Seq.isEmpty axis.Labels) then 
                result.AppendFormat(!format, n) |> ignore
                axis.Labels |> Seq.iter (fun x -> result.AppendFormat("|{0}", x) |> ignore)
                format := "|{0}:")

        format := "&chxp={0}"
        x.Axes |> Seq.iteri (fun n axis -> 
            if not(Seq.isEmpty axis.Positions) then 
                result.AppendFormat(!format, n) |> ignore
                axis.Positions |> Seq.iter (fun x -> result.AppendFormat(",{0}", x) |> ignore)
                format := "|{0}")

        x.Axes
        |> Seq.fold (fun (writer:IWriter) axis -> writer.Write(axisToString axis.Axis)) (newWriter "&chxt=" ",")
        |> ignore

        x.Markers 
        |> Seq.fold (fun (writer:IWriter) marker ->
            match marker with
            | Circle(color, series, whichPoints, size) -> writer.Write("o,{0},{1},{2},{3}", hex color, series, whichPoints, size)
            | FillToBottom(color, series) -> writer.Write("B,{0},{1},0,0", hex color, series)
            | FillBetween(color, startSeries, endSeries) -> writer.Write("b,{0},{1},{2},0", hex color, startSeries, endSeries)) (newWriter "&chm=" "|")
        |> ignore

        x.Series 
        |> Seq.filter (fun x -> x.Name <> "")
        |> Seq.fold (fun (writer:IWriter) series -> writer.Write(series.Name)) (newWriter "&chdl=" "|")
        |> ignore

        x.Series
        |> Seq.filter (fun series -> series.Color <> Color.White)
        |> Seq.fold (fun (writer:IWriter) series -> writer.Write(hex series.Color)) (newWriter "&chco=" ",")
        |> ignore

        x.Series 
        |> Seq.fold (fun (writer:IWriter) series -> 
            writer.Write(x.DataEncoding.Encode(series.Data))) (newWriter "&chd=e:" ",")
        |> ignore

        x.LineStyles 
        |> Seq.fold (fun (writer:IWriter) style -> 
            match style with
            | Filled(width) -> writer.Write("{0}", width)
            | Dashed(width, dash, space) -> writer.Write("{0},{1},{2}", width, dash, space)) (newWriter "&chls=" "|")
        |> ignore
            
        result.ToString()