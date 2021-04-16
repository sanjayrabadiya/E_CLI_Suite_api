using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class StudyPlanTask: BaseEntity, ICommonAduit
    {
        public int StudyPlanId { get; set; }
        public int? TaskId { get; set; }
        public string TaskName { get; set; }
        public int? ParentId { get; set; }
        public bool isMileStone { get; set; }
        public int Duration { get; set;}
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Progress { get; set; }
        public int TaskOrder { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }       
        public int? DependentTaskId { get; set;}
        public ActivityType? ActivityType { get; set; }
        public int OffSet { get; set; }
    }
}
