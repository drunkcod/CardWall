using System;
using System.Text.RegularExpressions;
using System.Globalization;

namespace CardWall.Models
{
    public struct BurndownDataPoint 
    {
        const char Separator = ';';
        public readonly DateTime Date;
        public readonly int PointsRemaining;

        public BurndownDataPoint(DateTime date, int pointsRemaining) {
            this.Date = date;
            this.PointsRemaining = pointsRemaining;
        }

        public override string ToString() {
            return string.Format("{0}{1}{2}", Date.ToShortDateString(), Separator, PointsRemaining);
        }

        public static BurndownDataPoint Parse(string input) {
            var parts = input.Split(new[]{ Separator }, StringSplitOptions.RemoveEmptyEntries);
            return new BurndownDataPoint(DateTime.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1]));
        }
    }
}