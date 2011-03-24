#r @"Site\bin\CardWall.Core.dll"

open System
open System.Linq
open System.Text.RegularExpressions
open System.Xml.XPath
open System.Drawing
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

let data = 
    [
        {
            Date = new DateTime(2011, 03, 8)
            OpenBugs = 0
            ClosedBugs = 0
            OpenChores = 14
            ClosedChores = 3
            OpenFeatures = 76
            ClosedFeatures = 32 
        }
        {
            Date = new DateTime(2011, 03, 14)
            OpenBugs = 0
            ClosedBugs = 2
            OpenChores = 12
            ClosedChores = 6
            OpenFeatures = 76
            ClosedFeatures = 34 
        }
        {
            Date = new DateTime(2011, 03, 21)
            OpenBugs = 2
            ClosedBugs = 2
            OpenChores = 11
            ClosedChores = 10
            OpenFeatures = 72
            ClosedFeatures = 44
        }
    ]

let velocityData = 
    data |> Seq.pairwise
    |> Seq.map (fun (x, y) -> y.ClosedTotal - x.ClosedTotal)
    |> Seq.append (Seq.singleton 0)

let encoding = GoogleExtendedEncoding()

let scale max f = Seq.map (f >> (fun x -> x * encoding.MaxValue / max))

let barScal = scale 100

let chart = {
    Title = "Story type breakdown"
    Width = 800
    Height = 350
    Axes = 
        [
            {
                Axis = Axis.Y
                Range = (0, 100)
                Labels = ["0"; "50"; "90"; "Stories"]
                Positions = [0; 50; 90; 100]
            }
            {
                Axis = Axis.Right
                Range = (0, 20)
                Labels = ["0"; "15"; "Throughput"]
                Positions = [0; 15; 20]
            }
        ]
    Series = 
        [
            { 
                Name = "Open Features"
                Color = Color.GreenYellow
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
                Color = Color.MidnightBlue
                Data = velocityData |> scale 20 id
            }
        ]
    Markers = 
        [
            LineMarker(Color.MidnightBlue, 3, StartAt(1))
        ]
    Mode = ChartMode.StackedBars
    DataEncoding = encoding
    LineStyles = [LineStyle.Default] }

Console.WriteLine("<img src='{0}'/>", chart.ToString())
