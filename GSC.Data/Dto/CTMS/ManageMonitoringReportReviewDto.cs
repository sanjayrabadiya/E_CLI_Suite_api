using GSC.Data.Entities.Common;
using GSC.Data.Entities.UserMgt;
using GSC.Shared.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.CTMS
{
    public class ManageMonitoringReportReviewDto : BaseDto
    {
        public int ManageMonitoringReportId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsSendBack { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public DateTime? SendBackDate { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApproveDate { get; set; }
        public string Message { get; set; }
        public bool IsRights { get; set; }
        public List<ManageMonitoringReportReviewDto> users { get; set; }
        public User User { get; set; }
    }

    public class ManageMonitoringReportReviewHistory : BaseAuditDto
    {
        public int ManageMonitoringReportId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsSendBack { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public DateTime? SendBackDate { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApproveDate { get; set; }
        public string DocumentName { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public string Reason { get; set; }
        public string ReasonOth { get; set; }
    }
}
