using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cone;
using System.IO;

namespace CardWall.Specs
{
    [Describe(typeof(GoogleChartWriter))]
    public class GoogleChartWriterSpec
    {
        [DisplayAs("replace ' ' with '+'")]
        public void replace_space_with_plus() {
            var result = new StringWriter();
            var writer = new GoogleChartWriter(result);

            writer.Write("{0}", "Hello World!");
            Verify.That(() => result.ToString() == "Hello+World!");
        }
    }
}
