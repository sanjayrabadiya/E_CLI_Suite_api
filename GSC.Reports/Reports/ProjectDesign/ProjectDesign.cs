using GSC.Reports.General;
using System.Data;

namespace GSC.Reports.Reports.ProjectDesign
{
    public partial class ProjectDesign : Telerik.Reporting.Report, IReport
    {
        private ReportParam reportParam;
        public ProjectDesign(ReportParam param)
        {
            InitializeComponent();
            reportParam = param;
            PrepareData();
        }

        private void PrepareData()
        {
            DataSet dsQuotation = new ProjectDesignSql().GetProjectDesign(reportParam.RecordId);

            HasData = dsQuotation != null && dsQuotation.Tables.Count > 0 && dsQuotation.Tables[0].Rows.Count > 0;

            if (!HasData)
                return;

            this.DataSource = dsQuotation.Tables[0];
            
        }

        public bool HasData { get; set; }
    }
}