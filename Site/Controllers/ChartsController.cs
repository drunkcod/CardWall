using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using CardWall.Models;

namespace CardWall.Controllers
{
    class XYSeries 
    {
        public ChartSeries X;
        public ChartSeries Y;
        public LineStyle Style = LineStyle.Default;
    }

    class BurndownChart 
    {
        readonly BurndownData burndown;
        readonly IChartDataEncoding encoding;

        public BurndownChart(BurndownData data, IChartDataEncoding encoding) {
            this.burndown = data;
            this.encoding = encoding;
        }

        public DateTime StartDate;
        public DateTime EndDate;
        public int VelocityMax;

        public XYSeries CreateBurnLine(Color color) {
            return new XYSeries {
                X = new ChartSeries("", color, new []{ 0, encoding.MaxValue }),
                Y = new ChartSeries("", Color.Transparent, new []{ encoding.MaxValue, 0}), 
                Style = LineStyle.NewDashed(2, 2, 4)
            };
        }

        public XYSeries CreateVelocitySeries(Color color) {
            var velocity = new ChartSeries("", Color.Transparent, encoding.Scale(burndown.Select(item => item.Velocity), VelocityMax));
            var velocitySeries = new ChartSeries("Velocity", color, XAxisPositions);

            return new XYSeries {
                X = velocitySeries, 
                Y = velocity
            };
        }

        public XYSeries CreateVelocityMeanSeries(Color color) {
            var xMeanVelocity = new ChartSeries("", color, new []{ 0, encoding.MaxValue});
            var yMeanVelocity = new ChartSeries("", Color.Transparent, encoding.Scale(new []{ MeanVelocity, MeanVelocity}, VelocityMax));

            return new XYSeries {
                X = xMeanVelocity,
                Y = yMeanVelocity,
                Style = LineStyle.NewDashed(2, 2, 4)
            };
        }

        public XYSeries CreateScopeSeries(Color color) {
            return new XYSeries {
                X = new ChartSeries("Scope", color, XAxisPositions),
                Y = new ChartSeries("", Color.Transparent, encoding.Scale(burndown.Select(x => x.TotalPoints), ChartMax)) 
            };
        }

        public XYSeries CreatePointsRemainingSeries(Color color) {
            return new XYSeries {
                X = new ChartSeries("Points Remaining", color, XAxisPositions),
                Y = new ChartSeries("", Color.Transparent, encoding.Scale(burndown.Select(x => x.PointsRemaining), ChartMax))
            };
        }

        public int MeanVelocity { get { return (int)Math.Round(burndown.Average(x => (float)x.Velocity)); } }
        public int ChartMax { get { return (int)Math.Round(burndown.Max(item => item.TotalPoints) * 1.1); } }
        int TotalDays { get { return (int)Math.Ceiling((EndDate - StartDate).TotalDays); } }

        IEnumerable<int> XAxisPositions { 
            get {
                return encoding.Scale(burndown.Select(item => (int)(item.Date - StartDate).TotalDays), TotalDays);
            } 
        }
    }

    public class ChartsController : Controller
    {
        public ActionResult Index(string team) {
            var burndownColor = Color.SteelBlue;
            var velocityColor = Color.YellowGreen;
            var scopeColor = Color.Orange;

            var configuration = GetConfiguration(team);           
            var burndown = GetBurndownData(configuration.Project, configuration.Label, configuration.HistoricalDataPath);
        
            var encoding = new GoogleExtendedEncoding();
            var burnChart = new BurndownChart(burndown, encoding) {
                StartDate = burndown.Min(item => item.Date),
                EndDate = configuration.EndDate ?? burndown.Max(item => item.Date),
                VelocityMax = 25
            };
 
            var yAxis = new ChartAxis(Axis.Y, new Tuple<int, int>(0, burnChart.ChartMax), new string[0], new int[0]);          
            var xAxis = new ChartAxis(Axis.X, new Tuple<int, int>(0, 1), new[]{ burnChart.StartDate.ToShortDateString(), burnChart.EndDate.ToShortDateString() }, new[]{0, 1});
            var velocityAxis = new ChartAxis(Axis.Right, new Tuple<int, int>(0, burnChart.VelocityMax), new []{ "0", "5", "10", "15", "20", "Velocity" }, new []{ 0, 5, 10, 15, 20, 25 });

            var burnLine = burnChart.CreateBurnLine(Color.FromArgb(128, Color.Firebrick));
            var velocity = burnChart.CreateVelocitySeries(velocityColor);
            var velocityMean = burnChart.CreateVelocityMeanSeries(Color.FromArgb(128, velocityColor));
            var scope = burnChart.CreateScopeSeries(scopeColor);
            var pointsRemaining = burnChart.CreatePointsRemainingSeries(burndownColor); 

            var chart = new Chart(
                string.Format("{0}: {1} points remaining. Velocity {2}. {3}", 
                    configuration.Title, 
                    burndown.PointsRemaining, 
                    burnChart.MeanVelocity, 
                    EstimateRemainingDisplay(burndown, burnChart.MeanVelocity)), 800, 300,
                new []{ xAxis, yAxis, velocityAxis },
                new [] { 
                    pointsRemaining,
                    velocity,
                    scope,
                    burnLine,
                    velocityMean
                }.SelectMany(x => new [] { x.X, x.Y }), 
                new []{
                    ChartMarker.NewCircle(burndownColor, 0, MarkerPoints.All, 8),
                    ChartMarker.NewCircle(Color.White, 0, MarkerPoints.All, 4)
                }, 
                ChartMode.XYLine, encoding, new[]{ pointsRemaining.Style, velocity.Style, scope.Style, burnLine.Style, velocityMean.Style});
            return View(new ChartView { Name = configuration.Title, DisplayMarkup = "<img src='" + chart.ToString() + "'/>" });
        }

		string EstimateRemainingDisplay(BurndownData burndown, int meanVelocity) {
			var iterationsRemaining = meanVelocity == 0 ? "inf" :  (burndown.PointsRemaining / meanVelocity).ToString();
			return string.Format("{0} iterations remaining.", iterationsRemaining);
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
            var burndownCharts = (BurndownChartConfigurationSection)ConfigurationManager.GetSection("BurndownCharts");
            var found = burndownCharts.Charts.FirstOrDefault(x => x.Key == team);
            if(found != null)
                return found;
            return GetConfiguration(burndownCharts.DefaultChart);
        }
    }
}
