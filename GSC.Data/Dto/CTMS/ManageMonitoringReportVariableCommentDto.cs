using System;
using GSC.Data.Entities.Common;
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

        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }
    }
}