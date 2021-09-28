using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Shared.Extension;
using System;

namespace GSC.Data.Entities.CTMS
{
    public class ManageMonitoringVisit : BaseEntity, ICommonAduit
    {
        public int ActivityId { get; set; }
        public int ProjectId { get; set; }
        public DateTime? ScheduleStartDate { get; set; }

        public DateTime? ScheduleEndDate { get; set; }

        public DateTime? ActualStartDate { get; set; }

        public DateTime? ActualEndDate { get; set; }

        public int? CompanyId { get; set; }
        public Activity Activity { get; set; }
        public Data.Entities.Master.Project Project { get; set; }
    }
}