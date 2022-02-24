using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class CtmsMonitoringReportVariableValueChild : BaseEntity, ICommonAduit
    {
        public int CtmsMonitoringReportVariableValueId { get; set; }
        public int VariableValueId { get; set; }
        public string Value { get; set; }
        public CtmsMonitoringReportVariableValue CtmsMonitoringReportVariableValue { get; set; }
        public VariableValue VariableValue { get; set; }
    }
}
