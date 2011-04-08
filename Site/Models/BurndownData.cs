using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CardWall.Models
{
    public class BurndowChartData 
    {
        public DateTime Date;
        public int PointsRemaining;
        public int Velocity;
        public int TotalPoints;
    }

    public class BurndownData : IEnumerable<BurndowChartData>
    {
        readonly List<BurndownDataPoint> dataPoints = new List<BurndownDataPoint>();

        public BurndownData() { }
        public BurndownData(IEnumerable<BurndownDataPoint> data) {
            dataPoints.AddRange(data);
        }

        public void Load(string path) {
            using(var input = new StreamReader(System.IO.File.OpenRead(path))) {
                for(string line; (line = input.ReadLine()) != null;) {
                    dataPoints.Add(BurndownDataPoint.Parse(line));
                }                    
            }
        }

        public int PointsRemaining { get { return dataPoints.Last().PointsRemaining; } }

        public int Velocity { get { return GetChartData().ElementAt(dataPoints.Count - 3).Velocity; } }

        public void Add(DateTime date, int pointsRemaining, int totalPointsBurned) {
            dataPoints.Add(new BurndownDataPoint(date, pointsRemaining, totalPointsBurned));
        }

        IEnumerator<BurndowChartData> IEnumerable<BurndowChartData>.GetEnumerator() {
            return GetChartData().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetChartData().GetEnumerator();
        }

        IEnumerable<BurndowChartData> GetChartData() {
            for(var i = 1; i < dataPoints.Count; ++i) {
                var x = dataPoints[i - 1];
                var y = dataPoints[i];
                yield return new BurndowChartData { 
                    Date = y.Date,
                    PointsRemaining = y.PointsRemaining,
                    Velocity = y.TotalPointsBurned - x.TotalPointsBurned,
                    TotalPoints = y.PointsRemaining + y.TotalPointsBurned
                };
            }
        }

    }
}