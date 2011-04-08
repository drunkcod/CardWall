namespace CardWall
open System
open System.Globalization

type BurndownDataPoint(date, pointsRemaining, totalPointsBurned) =
    [<Literal>]
    static let Separator = ';'

    member this.Date : DateTime = date
    member this.PointsRemaining : int = pointsRemaining
    member this.TotalPointsBurned : int = totalPointsBurned

    static member Parse(input:string) =
        let parts = input.Split([| Separator |])
        BurndownDataPoint(DateTime.Parse(parts.[0], CultureInfo.InvariantCulture), int(parts.[1]), int(parts.[2]))

    override this.ToString() =
        String.Format("{0}{1}{2}{1}{3}", date.ToShortDateString(), Separator, pointsRemaining, totalPointsBurned);