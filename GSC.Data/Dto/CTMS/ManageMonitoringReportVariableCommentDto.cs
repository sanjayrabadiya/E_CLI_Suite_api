using System;
using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.CTMS
{
    public class ManageMonitoringReportVariableCommentDto : BaseDto
    {
        private DateTime? _createdDate;
        public int ManageMonitoringReportVariableId { get; set; }
        public string Comment { get; set; }
        public string RoleName { get; set; }
        public string CreatedByName { get; set; }
        public int? ReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string ReasonName { get; set; }
        public string Note { get; set; }
        public CollectionSources? CollectionSource { get; set; }
        public string TimeZone { get; set; }
        public CtmsCommentStatus? CommentStatus { get; set; }
        public string CommentStatusName { get; set; }
        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }
        public string ValueName { get; set; }
        public string Value { get; set; }
        public string OldValue { get; set; }
    }
}