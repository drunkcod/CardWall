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
}

let data = 
    [{
        Date = new DateTime(2011, 03, 14)
        OpenBugs = 0
        ClosedBugs = 2
        OpenChores = 12
        ClosedChores = 6
        OpenFeatures = 76
        ClosedFeatures = 34 
    };{
        Date = new DateTime(2011, 03, 21)
        OpenBugs = 2
        ClosedBugs = 2
        OpenChores = 11
        ClosedChores = 10
        OpenFeatures = 72
        ClosedFeatures = 44
    }]

let encoding = GoogleExtendedEncoding()

let chart = {
    Title = "Foo"
    Width = 400
    Height = 400
    Axes = []
    Series = 
        [
            { 
                Name = "Open Bugs"
                Color = Color.Firebrick
                Data = data |> Seq.map (fun x -> x.OpenBugs)
            }
        ]
    Markers = []
    Mode = ChartMode.StackedBars
    DataEncoding = encoding
    LineStyles = [LineStyle.Default] }

Console.WriteLine(chart.ToString())
