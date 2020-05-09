namespace GSC.Reports.Reports.ProjectDesign
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using Telerik.Reporting;
    using Telerik.Reporting.Drawing;

    /// <summary>
    /// Summary description for ProjectVisit.
    /// </summary>
    public partial class ProjectVisit : Telerik.Reporting.Report
    {
        public ProjectVisit()
        {
            InitializeComponent();
        }

        private void ProjectVisit_NeedDataSource(object sender, EventArgs e)
        {
            var report = (Telerik.Reporting.Processing.Report)sender;

            int id = Convert.ToInt32(report.Parameters["Id"].Value);

            var dsData = new ProjectDesignSql().GetProjectVisit(id);

            report.DataSource = dsData.Tables[0];
        }
    }
}