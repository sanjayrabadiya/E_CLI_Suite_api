namespace GSC.Reports.Reports.ProjectDesign
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using Telerik.Reporting;
    using Telerik.Reporting.Drawing;

    /// <summary>
    /// Summary description for ProjectVariables.
    /// </summary>
    public partial class ProjectVariables : Telerik.Reporting.Report
    {
        public ProjectVariables()
        {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        private void ProjectVariables_NeedDataSource(object sender, EventArgs e)
        {
            var report = (Telerik.Reporting.Processing.Report)sender;

            int id = Convert.ToInt32(report.Parameters["Id"].Value);

            var dsData = new ProjectDesignSql().GetProjectVariable(id);

            table1.DataSource = dsData.Tables[0];
        }
    }
}