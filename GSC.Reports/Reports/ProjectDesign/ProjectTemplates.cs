namespace GSC.Reports.Reports.ProjectDesign
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using Telerik.Reporting;
    using Telerik.Reporting.Drawing;

    public partial class ProjectTemplates : Telerik.Reporting.Report
    {
        public ProjectTemplates()
        {
            InitializeComponent();
        }

        private void ProjectTemplates_NeedDataSource(object sender, EventArgs e)
        {
            var report = (Telerik.Reporting.Processing.Report)sender;

            int id = Convert.ToInt32(report.Parameters["Id"].Value);

            var dsData = new ProjectDesignSql().GetProjectTemplate(id);

            report.DataSource = dsData.Tables[0];
        }
    }
}