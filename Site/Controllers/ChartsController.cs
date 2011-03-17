using System;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using CardWall.Models;

namespace CardWall.Controllers
{
    public class ChartsController : Controller
    {
        public ActionResult Index() {
            var burndown = new BurndownData();
            var burndownColor = Color.SteelBlue;

            var startDate = burndown.Data.Min(item => item.Date);
            var endDate = new DateTime(2011, 6, 1);

            var totalDays = (int)Math.Ceiling((endDate - startDate).TotalDays);

            var encoding = new GoogleExtendedEncoding();
            var maxValue = encoding.MaxValue;
            
            var maxPoints = burndown.Data.Max(item => item.PointsRemaining);
            var yAxis = new ChartAxis(Axis.Y, new Tuple<int, int>(0, maxPoints + 10), new string[0], new int[0]);
            var ys = new ChartSeries("", Color.Transparent, 
                burndown.Data.Select(item => item.PointsRemaining).Scale(0, maxPoints + 10, 0, maxValue));

            var xAxis = new ChartAxis(Axis.X, new Tuple<int, int>(0, 1), new[]{ startDate.ToShortDateString(), endDate.ToShortDateString() }, new[]{0, 1});
            var x = new ChartSeries("Points Remaining", burndownColor, 
                burndown.Data.Select(item => (int)(item.Date - startDate).TotalDays).Scale(0, totalDays, 0, maxValue));
          
            var xBurnLine = new ChartSeries("", Color.FromArgb(128, Color.Firebrick), new []{ 0, maxValue });
            var yBurnLine = new ChartSeries("", Color.Transparent, new []{ maxValue, 0});

            var chart = new LineChart(string.Format("{0} points remaining", burndown.PointsRemaining), 800, 300, new []{ xAxis, yAxis }, new []{ x, ys, xBurnLine, yBurnLine}, new []{
                ChartMarker.NewCircle(burndownColor, 0, -1, 8),
                ChartMarker.NewCircle(Color.White, 0, -1, 4)
            }, LineChartMode.XY, encoding, new[]{ LineStyle.Default, LineStyle.NewDashed(2, 2, 4) });
            return View(chart);
        }
    }
}
