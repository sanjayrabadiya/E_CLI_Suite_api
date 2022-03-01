using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class CtmsMonitoringReportVariableValue : BaseEntity, ICommonAduit
    {
        public int CtmsMonitoringReportId { get; set; }
        public int StudyLevelFormVariableId { get; set; }
        public string Value { get; set; }
        public CtmsCommentStatus? QueryStatus { get; set; }
        public bool IsNa { get; set; }
        public CtmsMonitoringReport CtmsMonitoringReport { get; set; }
        public StudyLevelFormVariable StudyLevelFormVariable { get; set; }
        public List<CtmsMonitoringReportVariableValueChild> Children { get; set; }
        public ICollection<CtmsMonitoringReportVariableValueQuery> Queryies { get; set; }
    }
}
