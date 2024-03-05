using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using System;

namespace GSC.Data.Entities.CTMS
{
    public class CtmsMonitoring : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int StudyLevelFormId { get; set; }
        public DateTime? ScheduleStartDate { get; set; }
        public DateTime? ScheduleEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public StudyLevelForm StudyLevelForm { get; set; }
        public Master.Project Project { get; set; }
        public int? ParentId { get; set; }
        public bool? IfMissed { get; set; }
        public bool? IfReSchedule { get; set; }
        public bool? IfApplicable { get; set; }
    }
}
