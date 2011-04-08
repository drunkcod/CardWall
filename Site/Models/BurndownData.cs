using System;
using System.Collections.Generic;
using System.Linq;

namespace CardWall.Models
{
    public class BurndownData : IEnumerable<BurndownDataPoint>
    {
        readonly List<BurndownDataPoint> dataPoints = new List<BurndownDataPoint>();

        public BurndownData() { }
        public BurndownData(IEnumerable<BurndownDataPoint> data) {
            dataPoints.AddRange(data);
        }

        public int PointsRemaining { get { return dataPoints.Last().PointsRemaining; } }

        public void Add(DateTime date, int pointsRemaining, int totalPointsBurned) {
            dataPoints.Add(new BurndownDataPoint(date, pointsRemaining, totalPointsBurned));
        }

        IEnumerator<BurndownDataPoint> IEnumerable<BurndownDataPoint>.GetEnumerator() {
            return dataPoints.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return dataPoints.GetEnumerator();
        }
    }
}