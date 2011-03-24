using System;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using CardWall.Models;
using System.Collections.Generic;
using System.IO;

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

            var configuration = GetConfiguration(team);
            
            var burndown = GetSouthBurndownData(configuration.Project, configuration.Label, configuration.HistoricalDataPath);
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

            var chart = new Chart(string.Format("{0} points remaining", burndown.PointsRemaining), 800, 300, new []{ xAxis, yAxis }, new []{ x, ys, xBurnLine, yBurnLine}, new []{
                ChartMarker.NewCircle(burndownColor, 0, MarkerPoints.All, 8),
                ChartMarker.NewCircle(Color.White, 0, MarkerPoints.All, 4)
            }, ChartMode.XYLine, encoding, new[]{ LineStyle.Default, LineStyle.NewDashed(2, 2, 4) });
            return View(new ChartView { Name = configuration.Name, DisplayMarkup = "<img src='" + chart.ToString() + "'/>" });
        }

        BurndownData GetSouthBurndownData(int project, string label, string historicalDataPath) 
        {
            var tracker = new PivotalTracker(Environment.GetEnvironmentVariable("TrackerToken", EnvironmentVariableTarget.Machine));
            var stories = 
                tracker.Stories(project).Result
                .Where(item => item.Labels.Contains(label, StringComparer.InvariantCultureIgnoreCase))
                .Where(item => item.CurrentState != PivotalStoryState.Accepted);

            var pointsRemaining = stories.Sum(item => Math.Max(0, item.Estimate ?? 0)); 

            return new BurndownData(LoadBurndownData(Server.MapPath(historicalDataPath))){
                { DateTime.Today, pointsRemaining }
            };
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

        IEnumerable<BurndownDataPoint> LoadBurndownData(string path) {
            using(var input = new StreamReader(System.IO.File.OpenRead(path))) {
                for(string line; (line = input.ReadLine()) != null;) {
                    yield return BurndownDataPoint.Parse(line);
                }                    
            }
        }
    }
}
