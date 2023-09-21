using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

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
        public Data.Entities.Master.Project Project { get; set; }
        public int? ParentId { get; set; }
        public bool? If_Missed { get; set; }
        public bool? If_ReSchedule { get; set; }
        public bool? If_Applicable { get; set; }

    }
}
