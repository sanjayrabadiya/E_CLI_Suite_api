using System;
using GSC.Data.Dto.Attendance;
using System.Collections.Generic;
using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.Report
{
    public class ReviewDto : BaseDto
    {
        private DateTime? _createdDate;
        public int ScreeningEntryId { get; set; }
        public int ScreeningTemplateValueId { get; set; }
        public int ScreeningTemplateId { get; set; }
        public string ScreeningTemplateValue { get; set; }
        public string ScreeningNo { get; set; }
        public string VolunteerName { get; set; }
        public string Visit { get; set; }
        public string Value { get; set; }
        public QueryStatus? QueryStatus { get; set; }
        public string CreatedByName { get; set; }
        public int? ReviewLevel { get; set; }
        public string ReviewLevelName { get; set; }

        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }

        public string StatusName { get; set; }
        public string SubjectNo { get; set; }
        public string RandomizationNumber { get; set; }
        public int ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string SiteName { get; set; }
        public string SiteCode { get; set; }
        public string AttendanceDate { get; set; }
        public string ScreeningDate { get; set; }
        public string ReviewBy { get; set; }
        public DateTime? ReviewedOn { get; set; }
        // added for dynamic column 04/06/2023
        public List<WorkFlowReview> WorkFlowReviewList { get; set; }
    }
    // added for dynamic column 04/06/2023
    public class WorkFlowReview
    {
        public int LevelNo { get; set; }
        public string ReviewerRole { get; set; }
        public string ReviewerName { get; set; }
        public DateTime? ReviewedDate { get; set; }
    }
    //
    public class ReviewSearchDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public int?[] PeriodIds { get; set; }
        public int?[] SubjectIds { get; set; }
        public int?[] VisitIds { get; set; }
        public int?[] TemplateIds { get; set; }
        public int?[] ReviewStatus { get; set; }
        public int?[] StatusIds { get; set; }
    }
}