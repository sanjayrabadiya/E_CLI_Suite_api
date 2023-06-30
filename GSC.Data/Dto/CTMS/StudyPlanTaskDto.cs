using GSC.Data.Entities.Common;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.CTMS
{

    public class StudyPlanTaskGridDto
    {
        public int StudyPlanId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? EndDateDay { get; set; }
        public List<StudyPlanTaskDto> StudyPlanTask { get; set; }

    }

    public class StudyPlanTaskDto : BaseAuditDto
    {
        public string TaskName { get; set; }
        public int? TaskId { get; set; }
        public int StudyPlanId { get; set; }
        public int? ParentId { get; set; }
        public bool? IsMileStone { get; set; }
        public int Duration { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StartDateDay { get; set; }
        public int DurationDay { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? EndDateDay { get; set; }
        public int Progress { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public int TaskOrder { get; set; }
        public string Predecessor { get; set; }
        public int? DependentTaskId { get; set; }
        public ActivityType? ActivityType { get; set; }
        public int OffSet { get; set; }
        public bool IsManual { get; set; }
        public RefrenceType RefrenceType { get; set; }
        public int CreatedBy { get; set; }
        public string Status { get; set; }
        public bool? PreApprovalStatus { get; set; }
        public bool? ApprovalStatus { get; set; }
        public string FileName { get; set; }
        public string DocumentPath { get; set; }
        public string Site { get; set; }
    }

    public class StudyPlanTaskChartDto
    {
        public int All { get; set; }
        public int Complete { get; set; }
        public int DueDate { get; set; }
        public int DeviatedDate { get; set; }
        public int NotStartedDate { get; set; }
        public int OnGoingDate { get; set; }

    }

    public class StudyPlanTaskChartReportDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TaskName { get; set; }
        public int Duration { get; set; }
        public int NoOfDeviatedDay { get; set; }

    }
}
