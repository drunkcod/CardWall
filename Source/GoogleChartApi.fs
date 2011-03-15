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
        let hex (c:Color) = 
            if c.A = 255uy then
                String.Format("{0:x2}{1:x2}{2:x2}", c.R, c.G, c.B)
            else String.Format("{0:x2}{1:x2}{2:x2}{3:x2}", c.R, c.G, c.B, c.A)

        result.AppendFormat("&chs={0}x{1}", x.Width, x.Height) |> ignore

        let format = ref "&chxt={0}"
        x.Axes |> Seq.iter (fun axis -> 
            result.AppendFormat(!format, axisToString axis.Axis) |> ignore
            format := ",{0}")

        format := "&chxr={0},{1},{2}"
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

        let sep = ref "&chm="
        x.Markers |> Seq.iter (fun marker ->
            match marker with
            | Circle(color, series, whichPoints, size) -> result.AppendFormat("{0}o,{1},{2},{3},{4}", !sep, hex color, series, whichPoints, size)
            | FillToBottom(color, series) -> result.AppendFormat("{0}B,{1},{2},0,0", !sep, hex color, series)
            | FillBetween(color, startSeries, endSeries) -> result.AppendFormat("{0}b,{1},{2},{3},0", !sep, hex color, startSeries, endSeries)
            |> ignore
            sep := "|")

        let format = ref "&chdl={0}"
        x.Series 
        |> Seq.filter (fun x -> x.Name <> "")
        |> Seq.iter (fun series ->
            result.AppendFormat(!format, series.Name) |> ignore
            format := "|{0}")

        let format = ref "&chco={0}"
        x.Series
        |> Seq.filter (fun series -> series.Color <> Color.White)
        |> Seq.iter (fun series ->
            result.AppendFormat(!format, hex series.Color) |> ignore
            format := ",{0}")

        let all = x.Series |> Seq.collect (fun x -> x.Data)

        let format = ref "&chd=e:{0}"
        x.Series |> Seq.iter (fun series -> 
            result.AppendFormat(!format, x.DataEncoding.Encode(series.Data)) |> ignore
            format := ",{0}")

        let sep = ref "&chls="
        x.LineStyles 
        |> Seq.iter (fun style -> 
            match style with
            | Filled(width) -> result.AppendFormat("{0}{1}", !sep, width)
            | Dashed(width, dash, space) -> result.AppendFormat("{0}{1},{2},{3}", !sep, width, dash, space)
            |> ignore
            sep := "|")
            
            
        result.ToString()