using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System;

namespace GSC.Data.Entities.CTMS
{
    public class OverTimeMetrics : BaseEntity, ICommonAduit
    {
        public int PlanMetricsId { get; set; }
        public int ProjectId { get; set; }
        public bool? If_Active { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Planned { get; set; }
        public int? Actual { get; set; }
        public string TotalPlannig { get; set; }
        public int? ActualPlannedno { get; set; }
        public Master.Project Project { get; set; }
        public PlanningType PlanningType { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
