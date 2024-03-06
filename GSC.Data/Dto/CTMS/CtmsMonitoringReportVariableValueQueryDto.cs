using System;
using System.Collections.Generic;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.CTMS
{
    public class CtmsMonitoringReportVariableValueQueryDto : BaseDto
    {
        private DateTime? _createdDate;
        public int CtmsMonitoringReportVariableValueId { get; set; }
        public string Value { get; set; }
        public int? ReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string ReasonName { get; set; }
        public CtmsCommentStatus? QueryStatus { get; set; }
        public string CreatedByName { get; set; }
        public string QueryBy { get; set; }
        public string Note { get; set; }
        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }
        public string ValueName { get; set; }
        public string OldValue { get; set; }
        public string StatusName { get; set; }
        public short QueryLevel { get; set; }
        public short UserName { get; set; }
        public short UserRole { get; set; }
        public bool IsSystem { get; set; }
        public string TimeZone { get; set; }
        public int? QueryParentId { get; set; }
        public int? AvgOpenQueries { get; set; }
        public DateTime? PreviousQueryDate { get; set; }
        public ICollection<CtmsMonitoringReportVariableValueChild> Children { get; set; }
        public CollectionSources? CollectionSource { get; set; }
        public string Activity { get; set; }
        public string Site { get; set; }
    }
}