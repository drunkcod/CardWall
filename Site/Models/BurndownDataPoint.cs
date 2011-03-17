using System;

namespace CardWall.Models
{
    public struct BurndownDataPoint 
    {
        public readonly DateTime Date;
        public readonly int PointsRemaining;

        public BurndownDataPoint(DateTime date, int pointsRemaining) {
            this.Date = date;
            this.PointsRemaining = pointsRemaining;
        }
    }
}