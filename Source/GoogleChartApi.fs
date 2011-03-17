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

type Axis = 
    | X = 0 
    | Y = 1 
    | Top = 2
    | Right = 3

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

type LineChartMode = 
    | Default = 1 
    | SparkLines = 2
    | XY = 3

type Format =
    | FormatSingle of string * obj[]
    | FormatMultiple of (string * obj[]) * (string * obj[]) seq
    with
        static member single(format:string, [<ParamArray>]args:obj[]) = FormatSingle(format, args)
        static member args(format:string, [<ParamArray>]args:obj[]) = format, args

type GoogleChartWriter(writer:TextWriter) =
    member this.Write(format:string, [<ParamArray>]args:obj[]) =
        if args <> null then
            for i = 0 to args.Length - 1 do
                args.[i] <- 
                    match args.[i] with
                    | :? string as s -> s.Replace(' ', '+')
                    | :? Color as c ->
                        if c.A = 255uy then
                            String.Format("{0:x2}{1:x2}{2:x2}", c.R, c.G, c.B)
                        else String.Format("{0:x2}{1:x2}{2:x2}{3:x2}", c.R, c.G, c.B, c.A)
                    | :? Axis as axis ->
                        match axis with
                        | Axis.X -> "x"
                        | Axis.Y -> "y"
                        | Axis.Top -> "t"
                        | Axis.Right -> "r"
                        | _ -> raise(new ArgumentException())
                    | :? LineChartMode as mode ->
                        match mode with
                        | LineChartMode.Default -> "lc"
                        | LineChartMode.SparkLines -> "ls"
                        | LineChartMode.XY -> "lxy"
                        | _ -> raise(new ArgumentException())
                    | x -> x.ToString()
                    |> (fun x -> x :> obj)
        writer.Write(format, args) |> ignore

type LineChart = {
    Title : string
    Width : int
    Height : int 
    Axes : ChartAxis seq 
    Series : ChartSeries seq 
    Markers : ChartMarker seq 
    Mode : LineChartMode 
    DataEncoding : IChartDataEncoding 
    LineStyles : LineStyle seq } with
    
    override x.ToString() =
        use s = new StringWriter()
        let result = GoogleChartWriter(s) 
               
        result.Write(GoogleChartApi.BaseUrl)
        result.Write("?cht={0}", x.Mode)

        result.Write("&chtt={0}", x.Title)

        let append(format, args) = result.Write(format, args)
        let rec appendF = function
            | FormatSingle(format, args) -> append(format,args)
            | FormatMultiple(first, rest) ->
                append first
                rest |> Seq.iter append

        let appendFormat first next items =
            items
            |> Seq.zip (Seq.initInfinite (fun x -> if x = 0 then first else next))
            |> Seq.iter (fun (sep, x) ->
                result.Write(sep:string) |> ignore
                appendF x)

        append <| Format.args("&chs={0}x{1}", x.Width, x.Height)

        let numberedAxes = x.Axes |> Seq.mapi (fun n axis -> (n, axis))

        numberedAxes
        |> Seq.choose (fun (n, axis) ->
            match axis.Range with
            | (0, 100) -> None
            | (min, max) -> Some(Format.single("{0},{1},{2}", n, min, max)))
        |> appendFormat "&chxr=" "|"

        numberedAxes
        |> Seq.choose (fun (n, axis) ->
            if Seq.isEmpty axis.Labels then None
            else Some(FormatMultiple(Format.args("{0}:", n), axis.Labels |> Seq.map (fun x -> Format.args("|{0}", x)))))
        |> appendFormat "&chxl=" "|"

        numberedAxes
        |> Seq.choose (fun (n, axis) ->
            if Seq.isEmpty axis.Positions then None
            else Some(FormatMultiple(Format.args("{0}", n), axis.Positions |> Seq.map (fun x -> Format.args(",{0}", x)))))
        |>  appendFormat "&chxp=" "|"

        x.Axes |> Seq.map (fun x -> Format.single("{0}", x.Axis))
        |> appendFormat "&chxt=" ","

        x.Markers
        |> Seq.map (function
            | Circle(color, series, whichPoints, size) -> Format.single("o,{0},{1},{2},{3}", color, series, whichPoints, size)
            | FillToBottom(color, series) -> Format.single("B,{0},{1},0,0", color, series)
            | FillBetween(color, startSeries, endSeries) -> Format.single("b,{0},{1},{2},0", color, startSeries, endSeries)) 
        |> appendFormat "&chm=" "|"

        x.Series 
        |> Seq.choose (fun x -> if x.Name = "" then None else Some(Format.single(x.Name)))
        |> appendFormat "&chdl=" "|"

        x.Series
        |> Seq.choose (fun series -> 
            if series.Color = Color.Transparent then None
            else Some(Format.single("{0}", series.Color)))
        |> appendFormat "&chco=" ","

        x.Series 
        |> Seq.map (fun series -> Format.single(x.DataEncoding.Encode(series.Data))) 
        |> appendFormat "&chd=e:" ","

        x.LineStyles
        |> Seq.map (function
            | Filled(width) -> Format.single("{0}", width)
            | Dashed(width, dash, space) -> Format.single("{0},{1},{2}", width, dash, space))
        |> appendFormat "&chls=" "|"
            
        s.ToString()