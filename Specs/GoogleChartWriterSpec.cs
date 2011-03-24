using System.Drawing;
using System.IO;
using Cone;

namespace CardWall.Specs
{
    [Describe(typeof(GoogleChartWriter))]
    public class GoogleChartWriterSpec
    {
        string Format(object arg) {
            var writer = new GoogleChartWriter(new StringWriter());
            return writer.Format(arg);
        }

        [DisplayAs("replace ' ' with '+'")]
        public void replace_space_with_plus() {
            Verify.That(() => Format("Hello World!") == "Hello+World!");
        }

        public void RGB_color_as_hex() {
            Verify.That(() => Format(Color.FromArgb(1, 2, 128)) == "010280");
        }

        [Row(Axis.X, "x")
        ,Row(Axis.Y, "y")
        ,Row(Axis.Top, "t")
        ,Row(Axis.Right, "r")]
        public void axis_format(Axis axis, string expected) {
            Verify.That(() => Format(axis) == expected);
        }

        [Row(ChartMode.Line, "lc")
        ,Row(ChartMode.SparkLines, "ls")
        ,Row(ChartMode.XYLine, "lxy")
        ,Row(ChartMode.StackedBars, "bvs")]
        public void line_chart_mode(ChartMode mode, string expected) {
            Verify.That(() => Format(mode) == expected);
        }

        public void LineStyle_Filled() {
            Verify.That(() => Format(LineStyle.NewFilled(3)) == "3");
        }

        public void LineStyle_Dashed() {
            Verify.That(() => Format(LineStyle.NewDashed(1, 2, 3)) == "1,2,3");
        }

        public void ChartMarker_Circle() {
            Verify.That(() => Format(ChartMarker.NewCircle(Color.Red, 1, 2, 3)) == "o,ff0000,1,2,3"); 
        }

        public void ChartMarker_FillToBottom() {
            Verify.That(() => Format(ChartMarker.NewFillToBottom(Color.Red, 1)) == "B,ff0000,1,0,0"); 
        }

        public void ChartMarker_FillBetween() {
            Verify.That(() => Format(ChartMarker.NewFillBetween(Color.Red, 1, 2)) == "b,ff0000,1,2,0"); 
        }
    }
}
