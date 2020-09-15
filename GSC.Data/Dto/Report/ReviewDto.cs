using System;
using GSC.Data.Entities.Common;
using GSC.Helper;

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
    }

    public class ReviewSearchDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int?[] PeriodIds { get; set; }
        public int?[] SubjectIds { get; set; }
        public int?[] VisitIds { get; set; }
        public int?[] TemplateIds { get; set; }
        public int?[] ReviewStatus { get; set; }
        public int?[] StatusIds { get; set; }
    }
}