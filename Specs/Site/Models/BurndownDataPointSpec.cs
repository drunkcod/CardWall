using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cone;
using CardWall.Models;

namespace CardWall.Specs.Site.Models
{
    [Describe(typeof(BurndownDataPoint))]
    public class BurndownDataPointSpec
    {
        public void parsing_roundtrip() {
            var original = new BurndownDataPoint(new DateTime(1976, 8, 27), 42);
            var clone = BurndownDataPoint.Parse(original.ToString());

            Verify.That(() => clone.Date == original.Date);
            Verify.That(() => clone.PointsRemaining == original.PointsRemaining);

        }
    }
}
