using GSC.Common.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class StudyPlanTask: BaseEntity
    {
        public int StudyPlanId { get; set; }
        public int? TaskId { get; set; }
        public string TaskName { get; set; }
        public int ParentId { get; set; }
        public bool isMileStone { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Progress { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
    }
}
