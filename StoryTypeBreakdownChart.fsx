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
    member this.OpenTotal = this.OpenBugs + this.OpenChores + this.OpenFeatures

let inputPath = fsi.CommandLineArgs.[1]

let readRows maxCount = 
  let lines = File.ReadAllLines(inputPath)
  if lines.Length < maxCount then
    lines
  else 
    let lines' = Array.zeroCreate maxCount
    Array.blit lines (lines.Length - maxCount - 1) lines' 0 maxCount
    lines'

let rowData = 
    readRows 22
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
    rowData |> Seq.pairwise
    |> Seq.map (fun (x, y) -> y.ClosedTotal - x.ClosedTotal)

let data = rowData |> Seq.skip 1

let encoding = GoogleExtendedEncoding()

let scale max f = Seq.map (f >> (fun x -> x * encoding.MaxValue / max))

let top = max 100 (data |> Seq.map (fun x -> x.OpenTotal) |> Seq.max)

let barScal = scale top

let calendar = GregorianCalendar()

let throughputColor = Color.Indigo
let maxThroughput = 40
let chart = {
    Title = "Story type breakdown"
    Width = 800
    Height = 350
    Axes = 
        [
            {
                Axis = Axis.Y
                Range = (0, top + 10)
                Labels = ["50"; "90"; string top; "Stories"]
                Positions = [50; 90; top; top + 10]
            }
            {
                Axis = Axis.Right
                Range = (0, maxThroughput)
                Labels = ["10"; "Throughput"]
                Positions = [10; maxThroughput]
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
                Data = velocityData |> scale maxThroughput id
            }
        ]
    Markers = 
        [
            LineMarker(throughputColor, 3, StartAt(0), LineWidth(3))
        ]
    Mode = ChartMode.StackedBars
    DataEncoding = encoding
    LineStyles = [LineStyle.Default] }

Console.WriteLine("<img src='{0}'/>", chart.ToString())
