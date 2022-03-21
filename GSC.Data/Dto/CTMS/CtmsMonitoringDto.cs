using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.CTMS
{
    public class CtmsMonitoringDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int StudyLevelFormId { get; set; }
        public DateTime? ScheduleStartDate { get; set; }
        public DateTime? ScheduleEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public int? ParentId { get; set; }
    }

    public class CtmsMonitoringGridDto : BaseAuditDto
    {
        public int StudyLevelFormId { get; set; }
        public int? CtmsMonitoringReportId { get; set; } = 0;
        public bool IsReviewerApprovedForm { get; set; }
        public string ProjectName { get; set; }
        public string ReportStatus { get; set; }
        public string ActivityName { get; set; }
        public string VariableTemplateName { get; set; }
        public DateTime? ScheduleStartDate { get; set; }
        public DateTime? ScheduleEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public int? ParentId { get; set; }
    }
}
