using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Interactive;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Report
{
    public class TocIndexCreate
    {
        public PdfPage TocPage { get; set; }
        public PointF Point { get; set; }
        public PdfBookmark bookmark { get; set; }
    }
}
