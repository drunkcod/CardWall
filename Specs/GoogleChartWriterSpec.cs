using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cone;
using System.IO;
using System.Drawing;

namespace CardWall.Specs
{
    [Describe(typeof(GoogleChartWriter))]
    public class GoogleChartWriterSpec
    {
        string Write(object arg) {
            var result = new StringWriter();
            var writer = new GoogleChartWriter(result);

            writer.Write("{0}", arg);
            return result.ToString();
        }

        [DisplayAs("replace ' ' with '+'")]
        public void replace_space_with_plus() {
            Verify.That(() => Write("Hello World!") == "Hello+World!");
        }

        public void RGB_color_as_hex() {
            Verify.That(() => Write(Color.FromArgb(1, 2, 128)) == "010280");
        }

        [Row(Axis.X, "x")
        ,Row(Axis.Y, "y")
        ,Row(Axis.Top, "t")
        ,Row(Axis.Right, "r")]
        public void axis_format(Axis axis, string expected) {
            Verify.That(() => Write(axis) == expected);
        }

        [Row(LineChartMode.Default, "lc")
        ,Row(LineChartMode.SparkLines, "ls")
        ,Row(LineChartMode.XY, "lxy")]
        public void line_chart_mode(LineChartMode mode, string expected) {
            Verify.That(() => Write(mode) == expected);
        }
    }
}
