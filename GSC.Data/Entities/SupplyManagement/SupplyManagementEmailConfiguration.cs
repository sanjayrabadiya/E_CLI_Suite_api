﻿using GSC.Common.Base;
using GSC.Helper;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementEmailConfiguration : BaseEntity
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }

        public string EmailBody { get; set; }
        public SupplyManagementEmailTriggers Triggers { get; set; }
        public SupplyManagementEmailRecurrenceType? RecurrenceType { get; set; }
        public int Days { get; set; }
        public bool IsActive { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public Entities.Master.Project Project { get; set; }

        public string IpAddress { get; set; }

        public string TimeZone { get; set; }


    }
}
