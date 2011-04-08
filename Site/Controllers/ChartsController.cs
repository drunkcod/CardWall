using System;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using CardWall.Models;

namespace CardWall.Controllers
{
    class BurndownChartConfiguration 
    {
        public string Name;
        public string Label;
        public int Project;
        public string HistoricalDataPath;
    }

    public class ChartsController : Controller
    {
        public ActionResult Index(string team) {
            var burndownColor = Color.SteelBlue;
            var velocityColor = Color.YellowGreen;
            var scopeColor = Color.Orange;

            var configuration = GetConfiguration(team);
            
            var burndown = GetSouthBurndownData(configuration.Project, configuration.Label, configuration.HistoricalDataPath);
            var startDate = burndown.Min(item => item.Date);
            var endDate = new DateTime(2011, 6, 1);

            var totalDays = (int)Math.Ceiling((endDate - startDate).TotalDays);

            var encoding = new GoogleExtendedEncoding();
            var maxValue = encoding.MaxValue;
            
            var maxPoints = burndown.Max(item => item.TotalPoints);
            var chartMax = (int)Math.Round(maxPoints * 1.1);
            var yAxis = new ChartAxis(Axis.Y, new Tuple<int, int>(0, chartMax), new string[0], new int[0]);
            var velocityAxis = new ChartAxis(Axis.Right, new Tuple<int, int>(0, 20), new []{ "0", "5", "10", "15", "Velocity" }, new []{ 0, 5, 10, 15, 20 });
            
            var pointsRemaining = new ChartSeries("", Color.Transparent, burndown.Select(item => item.PointsRemaining).Scale(0, chartMax, 0, maxValue));
            var velocity = new ChartSeries("", Color.Transparent, burndown.Select(item => item.Velocity).Scale(0, 25, 0, maxValue));
            var scope = new ChartSeries("", Color.Transparent, burndown.Select(item => item.TotalPoints).Scale(0, chartMax, 0, maxValue));

            var xAxis = new ChartAxis(Axis.X, new Tuple<int, int>(0, 1), new[]{ startDate.ToShortDateString(), endDate.ToShortDateString() }, new[]{0, 1});
            var xs = burndown.Select(item => (int)(item.Date - startDate).TotalDays).Scale(0, totalDays, 0, maxValue);

            var pointsRemainingSeries = new ChartSeries("Points Remaining", burndownColor, xs);
            var velocitySeries = new ChartSeries("Velocity", velocityColor, xs);
            var scopeSeries = new ChartSeries("Scope", scopeColor, xs);
          
            var xBurnLine = new ChartSeries("", Color.FromArgb(128, Color.Firebrick), new []{ 0, maxValue });
            var yBurnLine = new ChartSeries("", Color.Transparent, new []{ maxValue, 0});

            var chart = new Chart(
                string.Format("{0} points remaining. Velocity {1}", burndown.PointsRemaining, burndown.Velocity), 800, 300,
                new []{ xAxis, yAxis, velocityAxis }, 
                new [] { 
                    pointsRemainingSeries, pointsRemaining,
                    velocitySeries, velocity,
                    scopeSeries, scope, 
                    xBurnLine, yBurnLine
                }, new []{
                ChartMarker.NewCircle(burndownColor, 0, MarkerPoints.All, 8),
                ChartMarker.NewCircle(Color.White, 0, MarkerPoints.All, 4)
            }, ChartMode.XYLine, encoding, new[]{ LineStyle.Default, LineStyle.Default, LineStyle.Default, LineStyle.NewDashed(2, 2, 4)});
            return View(new ChartView { Name = configuration.Name, DisplayMarkup = "<img src='" + chart.ToString() + "'/>" });
        }

        BurndownData GetSouthBurndownData(int project, string label, string historicalDataPath) 
        {
            var tracker = new PivotalTracker(Environment.GetEnvironmentVariable("TrackerToken", EnvironmentVariableTarget.Machine));
            var stories = 
                tracker.Stories(project).Result
                .Where(item => item.Labels.Contains(label, StringComparer.InvariantCultureIgnoreCase))
                .ToArray();
                
            var pointsRemaining = stories.Where(item => item.CurrentState != PivotalStoryState.Accepted).Sum(item => Math.Max(0, item.Estimate ?? 0)); 
            var totalPointsBurned = stories.Where(item => item.CurrentState == PivotalStoryState.Accepted).Sum(item => Math.Max(0, item.Estimate ?? 0)); 

            var burndownData = new BurndownData();
            burndownData.Load(Server.MapPath(historicalDataPath));
            burndownData.Add(DateTime.Today, pointsRemaining, totalPointsBurned);
            return burndownData;
        }

        BurndownChartConfiguration GetConfiguration(string team) {
            switch(team) {
                case "team north":
                    return new BurndownChartConfiguration {
                        Name = "Team North",
                        Project = 173053,
                        Label = team,
                        HistoricalDataPath = "TeamNorthBurndown.txt"
                    };
                case "incredible":
                    return new BurndownChartConfiguration {
                        Name = "Team Incredibles",
                        Project = 173053,
                        Label = team,
                        HistoricalDataPath = "TeamIncrediblesBurndown.txt"
                    };
                default:
                    return new BurndownChartConfiguration {
                        Name = "Team South",
                        Project = 173053,
                        Label = "team south",
                        HistoricalDataPath = "TeamSouthBurndown.txt"
                    };
            }
        }
    }
}
