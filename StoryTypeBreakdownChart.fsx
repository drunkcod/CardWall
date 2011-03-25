#r @"Site\bin\CardWall.Core.dll"

open System
open System.IO
open System.Linq
open System.Text.RegularExpressions
open System.Xml.XPath
open System.Drawing
open System.Globalization
open CardWall

type BreakdownChartItem = {
    Date : DateTime
    OpenBugs : int
    ClosedBugs : int
    OpenChores : int
    ClosedChores : int
    OpenFeatures : int
    ClosedFeatures : int
} with
    member this.ClosedTotal = this.ClosedBugs + this.ClosedChores + this.ClosedFeatures

let inputPath = fsi.CommandLineArgs.[1]

let data = 
    File.ReadAllLines(inputPath)
    |> Seq.skip 1
    |> Seq.map (fun line -> 
        let parts = line.Split([|';'|])
        {
            Date = DateTime.Parse(parts.[0])
            OpenBugs = Int32.Parse(parts.[1])
            ClosedBugs = Int32.Parse(parts.[2])
            OpenChores = Int32.Parse(parts.[3])
            ClosedChores = Int32.Parse(parts.[4])
            OpenFeatures = Int32.Parse(parts.[5])
            ClosedFeatures = Int32.Parse(parts.[6])
        })
    |> Seq.cache

let velocityData = 
    data |> Seq.pairwise
    |> Seq.map (fun (x, y) -> y.ClosedTotal - x.ClosedTotal)
    |> Seq.append (Seq.singleton 0)

let encoding = GoogleExtendedEncoding()

let scale max f = Seq.map (f >> (fun x -> x * encoding.MaxValue / max))

let barScal = scale 100

let calendar = GregorianCalendar()

let throughputColor = Color.Indigo
let chart = {
    Title = "Story type breakdown"
    Width = 800
    Height = 350
    Axes = 
        [
            {
                Axis = Axis.Y
                Range = (0, 100)
                Labels = ["50"; "90"; "Stories"]
                Positions = [50; 90; 100]
            }
            {
                Axis = Axis.Right
                Range = (0, 20)
                Labels = ["10"; "Throughput"]
                Positions = [10; 20]
            }
            {
                Axis = Axis.X
                Range = (0, data |> Seq.length)
                Labels = data |> Seq.map (fun x -> calendar.GetWeekOfYear(x.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday).ToString())
                Positions = []
            }
        ]
    Series = 
        [
            { 
                Name = "Open Features"
                Color = Color.YellowGreen
                Data = data |> barScal (fun x -> x.OpenFeatures)
            }
            { 
                Name = "Open Chores"
                Color = Color.Orange
                Data = data |> barScal (fun x -> x.OpenChores)
            }
            { 
                Name = "Open Bugs"
                Color = Color.Firebrick
                Data = data |> barScal (fun x -> x.OpenBugs)
            }
            {
                Name = "Throughput"
                Color = throughputColor
                Data = velocityData |> scale 20 id
            }
        ]
    Markers = 
        [
            LineMarker(throughputColor, 3, StartAt(1), LineWidth(3))
        ]
    Mode = ChartMode.StackedBars
    DataEncoding = encoding
    LineStyles = [LineStyle.Default] }

Console.WriteLine("<img src='{0}'/>", chart.ToString())
