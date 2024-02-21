using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class StudyPlanTask : BaseEntity, ICommonAduit
    {
        public int StudyPlanId { get; set; }
        public int? TaskId { get; set; }
        public string TaskName { get; set; }
        public int? ParentId { get; set; }
        public bool isMileStone { get; set; }
        public int Duration { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Progress { get; set; }
        public int TaskOrder { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public int? DependentTaskId { get; set; }
        public ActivityType? ActivityType { get; set; }
        public int OffSet { get; set; }
        public int? ProjectId { get; set; }
        public RefrenceType? RefrenceType { get; set; }

        [ForeignKey("ParentId")]
        public StudyPlanTask Parent { get; set; }

        [ForeignKey("DependentTaskId")]
        public StudyPlanTask DependentTask { get; set; }
        public StudyPlan StudyPlan { get; set; }
        public bool? PreApprovalStatus { get; set; }
        public bool? ApprovalStatus { get; set; }
        public string FileName { get; set; }
        public string DocumentPath { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? TotalCost { get; set; }
    }
}
