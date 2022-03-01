using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class CtmsMonitoringReport : BaseEntity, ICommonAduit
    {
        public int CtmsMonitoringId { get; set; }
        public MonitoringReportStatus? ReportStatus { get; set; }
        public CtmsMonitoring CtmsMonitoring { get; set; }
    }

    public class CtmsMonitoringReportBasic
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int StudyLevelFormId { get; set; }
        public int VariableTemplateId { get; set; }
    }
}
