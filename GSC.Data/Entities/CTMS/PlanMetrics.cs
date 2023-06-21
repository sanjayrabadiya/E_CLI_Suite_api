using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class PlanMetrics : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int Forecast { get; set; }
        public int Planned { get; set; }
        public int Actual { get; set; }
        public DateTime? PlnStartDate { get; set; }
        public DateTime? PlnEndDate { get; set; }
        public Master.Project Project { get; set; }
        public MetricsType MetricsType { get; set; }
        public OverTimeMetrics OverTimeMetrics { get; set; }

    }
}
