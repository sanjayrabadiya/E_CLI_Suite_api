using System;
using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.Report
{
    public class QueryManagementDto : BaseDto
    {
        private DateTime? _closedDate;

        private DateTime? _createdDate;

        private DateTime? _modifiedDate;
        public int ScreeningTemplateValueId { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int ScreeningEntryId { get; set; }
        public string ScreeningTemplateValue { get; set; }
        public string Visit { get; set; }
        public string FieldName { get; set; }
        public string AcknowledgementStatus { get; set; }
        public string QueryClosedBy { get; set; }
        public string Value { get; set; }
        public int? ReasonId { get; set; }
        public string ReasonOth { get; set; }
        public bool IsSubmitted { get; set; }
        public string QueryDescription { get; set; }
        public int CollectionSource { get; set; }
        public string ReasonName { get; set; }
        public QueryStatus? QueryStatus { get; set; }
        public string CreatedByName { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string ModifieedByName { get; set; }
        public string ClosedByName { get; set; }

        public string Note { get; set; }

        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }

        public DateTime? ModifiedDate
        {
            get => _modifiedDate?.UtcDateTime();
            set => _modifiedDate = value?.UtcDateTime();
        }

        public DateTime? ClosedDate
        {
            get => _closedDate?.UtcDateTime();
            set => _closedDate = value?.UtcDateTime();
        }

        public string ValueName { get; set; }
        public string OldValue { get; set; }
        public string StatusName { get; set; }
        public short QueryLevel { get; set; }
        public string VolunteerName { get; set; }
        public string SubjectNo { get; set; }
        public string RandomizationNumber { get; set; }
        public int projectId { get; set; }
        public string ProjectCode { get; set; }
    }

    public class QuerySearchDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int?[] PeriodIds { get; set; }
        public int?[] SubjectIds { get; set; }
        public int?[] VisitIds { get; set; }
        public int?[] TemplateIds { get; set; }
        public int?[] QueryGenerateBy { get; set; }
        public int? DataEntryBy { get; set; }
        public int? Status { get; set; }
    }
}