using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;

namespace CardWall.Controllers
{
    public class ChartsController : Controller
    {
        public ActionResult Index() {
            var burndown = new ChartSeries("", Color.CornflowerBlue, new[]{ 90, 80, 70, 60, 20 });
            var x = new ChartSeries("Burndown", Color.CornflowerBlue, new[]{ 75, 80, 70, 60, 2 });
            var yAxis = new ChartAxis(Axis.Y, new Tuple<int, int>(0, 100), new []{"Hello", "World" }, new int[]{0, 100});
            var chart = new LineChart(400, 300, 0, 100, new []{ yAxis }, new []{ x, burndown }, new []{
                ChartMarker.NewCircle(Color.CornflowerBlue, 0, -1, 10),
                ChartMarker.NewCircle(Color.White, 0, -1, 5)
            }, LineChartMode.XY);
            return View(chart);
        }

    }
}
