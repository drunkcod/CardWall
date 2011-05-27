using System;
using System.Collections.Generic;
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

    class XYSeries 
    {
        public ChartSeries X;
        public ChartSeries Y;
        public LineStyle Style = LineStyle.Default;
    }

    public class ChartsController : Controller
    {
        public ActionResult Index(string team) {
            var burndownColor = Color.SteelBlue;
            var velocityColor = Color.YellowGreen;
            var scopeColor = Color.Orange;

            var configuration = GetConfiguration(team);
            
            var burndown = GetBurndownData(configuration.Project, configuration.Label, configuration.HistoricalDataPath);
            var startDate = burndown.Min(item => item.Date);
            var endDate = new DateTime(2011, 6, 1);

            var totalDays = (int)Math.Ceiling((endDate - startDate).TotalDays);

            var encoding = new GoogleExtendedEncoding();
            var maxValue = encoding.MaxValue;
            
            var maxPoints = burndown.Max(item => item.TotalPoints);
            var chartMax = (int)Math.Round(maxPoints * 1.1);
            var yAxis = new ChartAxis(Axis.Y, new Tuple<int, int>(0, chartMax), new string[0], new int[0]);
            
            var xAxis = new ChartAxis(Axis.X, new Tuple<int, int>(0, 1), new[]{ startDate.ToShortDateString(), endDate.ToShortDateString() }, new[]{0, 1});
            var xs = encoding.Scale(burndown.Select(item => (int)(item.Date - startDate).TotalDays), totalDays);

            var scope = CreateScopeSeries(burndown, encoding, xs, chartMax, scopeColor);

            var pointsRemaining = CreatePointsRemainingSeries(burndown, encoding, xs, chartMax, burndownColor); 

            var velocityMax = 25;
            var velocityAxis = new ChartAxis(Axis.Right, new Tuple<int, int>(0, velocityMax), new []{ "0", "5", "10", "15", "20", "Velocity" }, new []{ 0, 5, 10, 15, 20, 25 });
            var meanVelocity = (int)Math.Round(burndown.Average(item => item.Velocity * 1.0));

            var burnLine = CreateBurnLine(encoding);
            var velocity = CreateVelocitySeries(burndown, encoding, xs, velocityMax, velocityColor);
            var velocityMean = CreateVelocityMeanSeries(burndown, encoding, meanVelocity, velocityMax, velocityColor);

            var chart = new Chart(
                string.Format("{0} points remaining. Velocity {1}. {2}", burndown.PointsRemaining, meanVelocity, EstimateRemainingDisplay(burndown, meanVelocity)), 800, 300,
                new []{ xAxis, yAxis, velocityAxis }, 
                new [] { 
                    pointsRemaining.X, pointsRemaining.Y,
                    velocity.X, velocity.Y,
                    scope.X, scope.Y,
                    burnLine.X, burnLine.Y,
                    velocityMean.X, velocityMean.Y
                }, 
                new []{
                    ChartMarker.NewCircle(burndownColor, 0, MarkerPoints.All, 8),
                    ChartMarker.NewCircle(Color.White, 0, MarkerPoints.All, 4)
                }, 
                ChartMode.XYLine, encoding, new[]{ pointsRemaining.Style, velocity.Style, scope.Style, burnLine.Style, velocityMean.Style});
            return View(new ChartView { Name = configuration.Name, DisplayMarkup = "<img src='" + chart.ToString() + "'/>" });
        }

		string EstimateRemainingDisplay(BurndownData burndown, int meanVelocity) {
			var iterationsRemaining = meanVelocity == 0 ? "inf" :  (burndown.PointsRemaining / meanVelocity).ToString();
			return string.Format("{0} iterations remaining.", iterationsRemaining);
		}

        XYSeries CreateVelocitySeries(BurndownData burndown, GoogleExtendedEncoding encoding, IEnumerable<int> xs, int velocityMax, Color velocityColor) {
            var velocity = new ChartSeries("", Color.Transparent, encoding.Scale(burndown.Select(item => item.Velocity), velocityMax));
            var velocitySeries = new ChartSeries("Velocity", velocityColor, xs);

            return new XYSeries {
                X = velocitySeries, 
                Y = velocity
            };
        }

        XYSeries CreateVelocityMeanSeries(BurndownData burndown, GoogleExtendedEncoding encoding, int meanVelocity, int velocityMax, Color velocityColor) {
            var xMeanVelocity = new ChartSeries("", Color.FromArgb(128, velocityColor), new []{ 0, encoding.MaxValue });
            var yMeanVelocity = new ChartSeries("", Color.Transparent, encoding.Scale(new []{ meanVelocity, meanVelocity}, velocityMax));

            return new XYSeries {
                X = xMeanVelocity,
                Y = yMeanVelocity,
                Style = LineStyle.NewDashed(2, 2, 4)
            };
        }

        XYSeries CreateBurnLine(GoogleExtendedEncoding encoding) {
            return new XYSeries {
                X = new ChartSeries("", Color.FromArgb(128, Color.Firebrick), new []{ 0, encoding.MaxValue }),
                Y = new ChartSeries("", Color.Transparent, new []{ encoding.MaxValue, 0}), 
                Style = LineStyle.NewDashed(2, 2, 4)
            };
        }

        XYSeries CreateScopeSeries(BurndownData burndown, GoogleExtendedEncoding encoding, IEnumerable<int> xs, int chartMax, Color scopeColor) {
            return new XYSeries {
                X = new ChartSeries("Scope", scopeColor, xs),
                Y = new ChartSeries("", Color.Transparent, encoding.Scale(burndown.Select(item => item.TotalPoints), chartMax)) 
            };
        }

        XYSeries CreatePointsRemainingSeries(BurndownData burndown, GoogleExtendedEncoding encoding, IEnumerable<int> xs, int chartMax, Color burndownColor) {
            return new XYSeries {
                X = new ChartSeries("Points Remaining", burndownColor, xs),
                Y = new ChartSeries("", Color.Transparent, encoding.Scale(burndown.Select(item => item.PointsRemaining), chartMax))
            };
        }

        BurndownData GetBurndownData(int project, string label, string historicalDataPath) 
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
                case "north":                    
                    return new BurndownChartConfiguration {
                        Name = "Team North",
                        Project = 173053,
                        Label = "team north",
                        HistoricalDataPath = "TeamNorthBurndown.txt"
                    };
                case "incredible":
                    return new BurndownChartConfiguration {
                        Name = "Team Incredibles",
                        Project = 173053,
                        Label = "team incredible",
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
