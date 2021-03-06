﻿namespace CardWall

open System
open System.Drawing
open System.IO
open System.Text

module GoogleChartApi =
    [<Literal>]
    let BaseUrl = "http://chart.apis.google.com/chart"

type Axis = 
    | X = 0 
    | Y = 1 
    | Top = 2
    | Right = 3

type ChartSeries = {
    Name : string 
    Color : Color
    Data : int seq }

type MarkerPoints =
    | All
    | StartAt of int

type LineWidth =
    | Default
    | LineWidth of int

type ChartMarker =
    | Circle of Color * int * MarkerPoints * int
    | FillToBottom of Color * int
    | FillBetween of Color * int * int
    | LineMarker of Color * int * MarkerPoints * LineWidth

type ChartAxis = { 
    Axis: Axis
    Range : int * int
    Labels : string seq 
    Positions : int seq }

type LineStyle =
    | Filled of int
    | Dashed of int * int * int
    with
        static member Default = Filled(1)

type ChartMode = 
    | Line = 1 
    | SparkLines = 2
    | XYLine = 3
    | StackedBars = 4

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
                args.[i] <- this.Format args.[i] :> obj
        writer.Write(this.Format(format), args) |> ignore

    member this.Format(arg:obj) =
        match arg with
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
        | :? ChartMode as mode ->
            match mode with
            | ChartMode.Line -> "lc"
            | ChartMode.SparkLines -> "ls"
            | ChartMode.XYLine -> "lxy"
            | ChartMode.StackedBars -> "bvs"
            | _ -> raise(new ArgumentException())
        | :? LineStyle as style ->
            match style with
            | Filled(width) -> width.ToString()
            | Dashed(width, dash, space) -> String.Format("{0},{1},{2}", width, dash, space)
        | :? LineWidth as width ->
            match width with 
            | Default -> "1"
            | LineWidth(x) -> x.ToString()
        | :? ChartMarker as marker ->
            match marker with
            | Circle(color, series, whichPoints, size) -> String.Format("o,{0},{1},{2},{3}", this.Format color, series, this.Format whichPoints, size)
            | FillToBottom(color, series) -> String.Format("B,{0},{1},0,0", this.Format color, series)
            | FillBetween(color, startSeries, endSeries) -> String.Format("b,{0},{1},{2},0", this.Format color, startSeries, endSeries) 
            | LineMarker(color, series, points, width) -> String.Format("D,{0},{1},{2},{3}", this.Format color, series, this.Format points, this.Format width)
        | :? MarkerPoints as points ->
            match points with
            | All -> "-1"
            | StartAt(n) -> String.Format("{0}::", n)            
        | x -> x.ToString()

type Chart = {
    Title : string
    Width : int
    Height : int 
    Axes : ChartAxis seq
    Series : ChartSeries seq 
    Markers : ChartMarker seq 
    Mode : ChartMode 
    DataEncoding : IChartDataEncoding 
    LineStyles : LineStyle seq } with
    
    override x.ToString() =
        use s = new StringWriter()
        let result = GoogleChartWriter(s) 
               
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
                result.Write(sep:string)
                appendF x)

        result.Write(GoogleChartApi.BaseUrl)
        result.Write("?cht={0}", x.Mode)
        result.Write("&chtt={0}", x.Title)
        result.Write("&chs={0}x{1}", x.Width, x.Height)

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

        x.LineStyles 
        |> Seq.map (fun x -> Format.single("{0}", x))
        |> appendFormat "&chls=" "|"

        x.Markers
        |> Seq.map (fun x -> Format.single("{0}", x))
        |> appendFormat "&chm=" "|"

        x.Series 
        |> Seq.choose (fun x -> if x.Name = "" then None else Some(Format.single(x.Name)))
        |> appendFormat "&chdl=" "|"

        x.Series
        |> Seq.choose (fun series -> 
            if series.Color = Color.Transparent then None
            else Some(Format.single("{0}", series.Color)))
        |> appendFormat "&chco=" ","      

        let dataSeries = (x.Series |> Seq.length) - (x.Markers |> Seq.filter (function LineMarker(_) -> true | _ -> false) |> Seq.length)

        x.Series 
        |> Seq.map (fun series -> Format.single(x.DataEncoding.Encode(series.Data))) 
        |> appendFormat (String.Format("&chd=e{0}:", dataSeries)) ","
           
        s.ToString()