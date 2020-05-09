namespace GSC.Reports.Shared
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using Telerik.Reporting;
    using Telerik.Reporting.Drawing;

    /// <summary>
    /// Summary description for NoDataFound.
    /// </summary>
    public partial class NoDataFound : Telerik.Reporting.Report
    {
        public NoDataFound()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        public NoDataFound(string strMessage)
        {
            InitializeComponent();
            textBox1.Value = strMessage;
            textBox1.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(30D);
        }

        public NoDataFound(Exception ex)
        {
            InitializeComponent();
            textBox1.Value = ex.Message;
            textBox1.Style.Font.Size = Telerik.Reporting.Drawing.Unit.Point(12D);
        }
    }
}