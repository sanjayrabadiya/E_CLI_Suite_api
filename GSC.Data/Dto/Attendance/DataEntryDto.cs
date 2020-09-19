using System.Collections.Generic;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Helper;

namespace GSC.Data.Dto.Attendance
{
    public class DataCaptureGridDto
    {
        public int AttendanceId { get; set; }
        public int? ScreeningEntryId { get; set; }
        public int ProjectId { get; set; }
        public int ProjectDesignPeriodId { get; set; }
        public string VolunteerName { get; set; }
        public string SubjectNo { get; set; }
        public string RandomizationNumber { get; set; }
        public List<DataEntryVisitDto> Visits { get; set; }
        public DashboardQueryStatusDto QueryStatus { get; set; }
        public DashboardStudyStatusDto VisitSummary { get; set; }
        public WorkFlowLevelDto WorkflowDetail { get; set; }
    }

    public class DataEntryVisitDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<DataEntryTemplateCountDto> TemplateCounts { get; set; }
        public int TotalQueries { get; set; }
        public List<DataEntryTemplateQueryCountDto> TemplateQueries { get; set; }
    }

    public class DataEntryVisitSummaryDto
    {
        public string VisitName { get; set; }
        public int ScreeningVisitId { get; set; }
        public int RecordId { get; set; }
        public int PendingCount { get; set; }
        public int InProcess { get; set; }
        public int Submitted { get; set; }
        public int Reviewed { get; set; }
        public int Completed { get; set; }
        public int TotalQueries { get; set; }
    }

    public class DataEntryVisitTemplateDto
    {
        public int ScreeningEntryId { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string VisitName { get; set; }
        public string SubjectName { get; set; }
    }

    public class DataEntryTemplateCountDto
    {
        public ScreeningTemplateStatus Status { get; set; }
        public string StatusName { get; set; }
        public int Count { get; set; }
        public List<DataEntryTemplateDto> Templates { get; set; }
    }

    public class DataEntryTemplateDto
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
    }

    public class DataEntryTemplateQueryCountDto
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public List<DataEntryQueryDto> Queries { get; set; }
    }

    public class DataEntryQueryDto
    {
        public QueryStatus? Status { get; set; }
        public string StatusName { get; set; }
        public int Count { get; set; }
    }
}