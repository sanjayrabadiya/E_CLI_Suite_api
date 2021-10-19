using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.CTMS
{
    public class ManageMonitoringVisitDto : BaseDto
    {
        public int ActivityId { get; set; }

        public int ProjectId { get; set; }

        public DateTime? ScheduleStartDate { get; set; }

        public DateTime? ScheduleEndDate { get; set; }

        public DateTime? ActualStartDate { get; set; }

        public DateTime? ActualEndDate { get; set; }

        public int? CompanyId { get; set; }
        public string ActivityName { get; set; }
        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public string CompanyName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
    }

    public class ManageMonitoringVisitGridDto : BaseAuditDto
    {
        public string Activity { get; set; }
        public string Project { get; set; }
        public DateTime ScheduleStartDate { get; set; }
        public DateTime ScheduleEndDate { get; set; }
        public DateTime ActualStartDate { get; set; }
        public DateTime ActualEndDate { get; set; }
    }
}