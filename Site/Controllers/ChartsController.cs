using System;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using CardWall.Models;
using System.Collections.Generic;
using System.IO;

namespace CardWall.Controllers
{
    public class ChartsController : Controller
    {
        public ActionResult Index(string team) {
            var burndownColor = Color.SteelBlue;

            var title = string.Empty;
            switch(team) {
                case "team north":
                    title = "Team North";
                    break;
                case "incredible":
                    title = "Team Incredibles";
                    break;
                default: 
                    team = "team south";
                    title = "Team South";
                    break;
            }

            var burndown = GetSouthBurndownData(team);
            var startDate = burndown.Min(item => item.Date);
            var endDate = new DateTime(2011, 6, 1);

            var totalDays = (int)Math.Ceiling((endDate - startDate).TotalDays);

            var encoding = new GoogleExtendedEncoding();
            var maxValue = encoding.MaxValue;
            
            var maxPoints = burndown.Max(item => item.PointsRemaining);
            var chartMax = (int)Math.Round(maxPoints * 1.1);
            var yAxis = new ChartAxis(Axis.Y, new Tuple<int, int>(0, chartMax), new string[0], new int[0]);
            var ys = new ChartSeries("", Color.Transparent, 
                burndown.Select(item => item.PointsRemaining).Scale(0, chartMax, 0, maxValue));

            var xAxis = new ChartAxis(Axis.X, new Tuple<int, int>(0, 1), new[]{ startDate.ToShortDateString(), endDate.ToShortDateString() }, new[]{0, 1});
            var xs = burndown.Select(item => (int)(item.Date - startDate).TotalDays).Scale(0, totalDays, 0, maxValue);
            var x = new ChartSeries("Points Remaining", burndownColor, xs);
          
            var xBurnLine = new ChartSeries("", Color.FromArgb(128, Color.Firebrick), new []{ 0, maxValue });
            var yBurnLine = new ChartSeries("", Color.Transparent, new []{ maxValue, 0});

            var chart = new LineChart(string.Format("{0} points remaining", burndown.PointsRemaining), 800, 300, new []{ xAxis, yAxis }, new []{ x, ys, xBurnLine, yBurnLine}, new []{
                ChartMarker.NewCircle(burndownColor, 0, -1, 8),
                ChartMarker.NewCircle(Color.White, 0, -1, 4)
            }, LineChartMode.XY, encoding, new[]{ LineStyle.Default, LineStyle.NewDashed(2, 2, 4) });
            return View(new ChartView { Name = title, DisplayMarkup = "<img src='" + chart.ToString() + "'/>" });
        }

        BurndownData GetSouthBurndownData(string label) 
        {
            var tracker = new PivotalTracker(Environment.GetEnvironmentVariable("TrackerToken", EnvironmentVariableTarget.Machine));
            var stories = 
                tracker.Stories(173053).Result
                .Where(item => item.Labels.Contains(label, StringComparer.InvariantCultureIgnoreCase))
                .Where(item => item.CurrentState != PivotalStoryState.Accepted);

            var pointsRemaining = stories.Sum(item => Math.Max(0, item.Estimate ?? 0)); 

            return new BurndownData(LoadBurndownData(Server.MapPath("TeamSouthBurndown.txt"))){
                { DateTime.Today, pointsRemaining }
            };
        }

        IEnumerable<BurndownDataPoint> LoadBurndownData(string path) {
            using(var input = new StreamReader(System.IO.File.OpenRead(path))) {
                for(string line; (line = input.ReadLine()) != null;) {
                    var parts = line.Split(';');
                    yield return new BurndownDataPoint(DateTime.Parse(parts[0]), int.Parse(parts[1]));
                }                    
            }
        }
    }
}
