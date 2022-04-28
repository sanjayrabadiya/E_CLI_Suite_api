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
    public class CtmsMonitoringStatus : BaseEntity, ICommonAduit
    {
        public int CtmsMonitoringId { get; set; }
        public MonitoringSiteStatus? Status { get; set; }
        public CtmsMonitoring CtmsMonitoring { get; set; }
    }
}
