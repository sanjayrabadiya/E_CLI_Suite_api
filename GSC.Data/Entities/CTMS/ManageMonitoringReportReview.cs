using GSC.Common.Base;
using System;
using System.Collections.Generic;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.CTMS
{
    public class ManageMonitoringReportReview : BaseEntity, ICommonAduit
    {
        public int ManageMonitoringReportId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsSendBack { get; set; }
        public int CompanyId { get; set; }
        public DateTime? SendBackDate { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApproveDate { get; set; }
        public string Message { get; set; }
        public ManageMonitoringReport ManageMonitoringReport { get; set; }
        public User User { get; set; }
    }
}
