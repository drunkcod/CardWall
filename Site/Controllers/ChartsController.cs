using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;

namespace CardWall.Controllers
{
    struct BurndownData 
    {
        public readonly DateTime Date;
        public readonly int PointsRemaining;

        public BurndownData(DateTime date, int pointsRemaining) {
            this.Date = date;
            this.PointsRemaining = pointsRemaining;
        }
    }

    static class IEnumerableExtensions
    {
        public static IEnumerable<int> Scale(this IEnumerable<int> source, int sourceOffset, int sourceRange, int targetOffset, int targetRange) {
            return source.Select(x => targetRange * x / sourceRange + targetOffset - sourceOffset); 
        }
    }

    public class ChartsController : Controller
    {
        public ActionResult Index() {
            var tracker = new PivotalTracker(Environment.GetEnvironmentVariable("TrackerToken", EnvironmentVariableTarget.Machine));

            var stories = 
                tracker.Stories(173053).Result
                .Where(item => item.Labels.Contains("team south", StringComparer.InvariantCultureIgnoreCase))
                .Where(item => item.CurrentState != PivotalStoryState.Accepted).ToArray();

            var todaysPoints = stories.Sum(item => Math.Max(0, item.Estimate ?? 0)); 

            var data = new[]{
                new BurndownData(DateTime.Parse("2011-02-21"), 24)
                ,new BurndownData(DateTime.Parse("2011-02-28"), 16)
                ,new BurndownData(DateTime.Parse("2011-03-08"), 19)
                ,new BurndownData(DateTime.Parse("2011-03-14"), 25)
                ,new BurndownData(DateTime.Parse("2011-03-15"), 93)
                ,new BurndownData(DateTime.Today, todaysPoints)
            };
         
            var startDate = data.Min(item => item.Date);
            var endDate = new DateTime(2011, 6, 1);

            var totalDays = (int)Math.Ceiling((endDate - startDate).TotalDays);

            var encoding = new GoogleExtendedEncoding();
            var maxValue = encoding.MaxValue;
            
            var maxPoints = data.Max(item => item.PointsRemaining);
            var yAxis = new ChartAxis(Axis.Y, new Tuple<int, int>(0, maxPoints + 10), new string[0], new int[0]);
            var ys = new ChartSeries("", Color.White, 
                data.Select(item => item.PointsRemaining).Scale(0, maxPoints + 10, 0, maxValue));

            var xAxis = new ChartAxis(Axis.X, new Tuple<int, int>(0, 1), new[]{ startDate.ToShortDateString(), endDate.ToShortDateString() }, new[]{0, 1});
            var x = new ChartSeries("Points Remaining", Color.CornflowerBlue, 
                data.Select(item => (int)(item.Date - startDate).TotalDays).Scale(0, totalDays, 0, maxValue));
          
            var xBurnLine = new ChartSeries("", Color.FromArgb(128, Color.Firebrick), new []{ 0, maxValue });
            var yBurnLine = new ChartSeries("", Color.White, new []{ maxValue, 0});

            var chart = new LineChart(string.Format("{0} points remaining", todaysPoints), 800, 300, new []{ xAxis, yAxis }, new []{ x, ys, xBurnLine, yBurnLine}, new []{
                ChartMarker.NewCircle(Color.CornflowerBlue, 0, -1, 8),
                ChartMarker.NewCircle(Color.White, 0, -1, 4)
            }, LineChartMode.XY, encoding, new[]{ LineStyle.Default, LineStyle.NewDashed(2, 2, 4) });
            return View(chart);
        }

    }
}
