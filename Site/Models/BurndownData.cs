using System;
using System.Collections.Generic;
using System.Linq;

namespace CardWall.Models
{
    public class BurndownData 
    {
        readonly int pointsRemaining;

        public BurndownData() 
        {
            var tracker = new PivotalTracker(Environment.GetEnvironmentVariable("TrackerToken", EnvironmentVariableTarget.Machine));
            var stories = 
                tracker.Stories(173053).Result
                .Where(item => item.Labels.Contains("team south", StringComparer.InvariantCultureIgnoreCase))
                .Where(item => item.CurrentState != PivotalStoryState.Accepted);

            pointsRemaining = stories.Sum(item => Math.Max(0, item.Estimate ?? 0)); 
        }

        public int PointsRemaining { get { return pointsRemaining; } }

        public IEnumerable<BurndownDataPoint> Data 
        {
            get 
            {
                return new[]{
                    new BurndownDataPoint(DateTime.Parse("2011-02-21"), 24)
                    ,new BurndownDataPoint(DateTime.Parse("2011-02-28"), 16)
                    ,new BurndownDataPoint(DateTime.Parse("2011-03-08"), 19)
                    ,new BurndownDataPoint(DateTime.Parse("2011-03-14"), 25)
                    ,new BurndownDataPoint(DateTime.Parse("2011-03-15"), 93)
                    ,new BurndownDataPoint(DateTime.Today, pointsRemaining)
                };
            }
        }
    }
}