using System;

namespace GSC.Data.Dto.Screening
{
    public class ScheduleDueReport
    {
        public Int64 Id { get; set; }
        public string studyCode { get; set; }
        public string siteCode { get; set; }
        public string initial { get; set; }
        public string screeningNo { get; set; }
        public string randomizationNumber { get; set; }
        public string visitName { get; set; }
        public string templateName { get; set; }
        public DateTime? scheduleDate { get; set; }
        public string scheduleDateExcel { get; set; }
    }

    public class ScheduleDueReportSearchDto
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public int?[] SubjectIds { get; set; }
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
        public int?[] PeriodIds { get; set; }
        public int?[] VisitIds { get; set; }
        public int?[] TemplateIds { get; set; }
    }
}
